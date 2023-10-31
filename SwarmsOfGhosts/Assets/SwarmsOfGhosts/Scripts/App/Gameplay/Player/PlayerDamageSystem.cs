using System;
using SwarmsOfGhosts.App.Gameplay.Enemy;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Restart;
using SwarmsOfGhosts.App.Gameplay.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateBefore(typeof(EnemyDamageSystem))]
    public partial class PlayerDamageSystem : SystemBase
    {
        private struct EquatableTriggerEvent : IEquatable<EquatableTriggerEvent>
        {
            public TriggerEvent TriggerEvent;
            public bool Equals(EquatableTriggerEvent other) => TriggerEvent.CompareTo(other.TriggerEvent) == 0;

            public override int GetHashCode()
            {
                var hash = 17;
                hash = hash * 23 + TriggerEvent.EntityA.GetHashCode();
                hash = hash * 23 + TriggerEvent.EntityB.GetHashCode();
                hash = hash * 23 + TriggerEvent.BodyIndexA.GetHashCode();
                hash = hash * 23 + TriggerEvent.BodyIndexB.GetHashCode();
                hash = hash * 23 + TriggerEvent.ColliderKeyA.Value.GetHashCode();
                hash = hash * 23 + TriggerEvent.ColliderKeyB.Value.GetHashCode();
                return hash;
            }
        }

        private PauseSystem _pauseSystem;
        private StepPhysicsWorld _stepPhysicsWorld;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        private NativeList<EquatableTriggerEvent> _currentTriggerEvents;
        private NativeParallelHashMap<EquatableTriggerEvent, float> _previousTriggerEvents;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();
            _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _currentTriggerEvents = new NativeList<EquatableTriggerEvent>(Allocator.Persistent);

            _previousTriggerEvents =
                new NativeParallelHashMap<EquatableTriggerEvent, float>(128, Allocator.Persistent);

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            _currentTriggerEvents.Clear();

            var deltaTime = Time.DeltaTime;

            Dependency = new TriggerPhysicsEventsJob
            {
                EnemyGroup = GetComponentDataFromEntity<EnemyTag>(true),
                PlayerGroup = GetComponentDataFromEntity<PlayerTag>(true),
                TriggerEvents = _currentTriggerEvents
            }.Schedule(_stepPhysicsWorld.Simulation, Dependency);

            var endSimulationCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
            Dependency = new PlayerDamageJob
            {
                EnemyGroup = GetComponentDataFromEntity<EnemyTag>(true),
                PlayerGroup = GetComponentDataFromEntity<PlayerTag>(true),
                DestroyGroup = GetComponentDataFromEntity<DestroyTag>(true),
                DamageGroup = GetComponentDataFromEntity<EnemyDamage>(true),
                HealthGroup = GetComponentDataFromEntity<PlayerHealth>(),
                DeltaTime = deltaTime,
                PreviousTriggerEvents = _previousTriggerEvents,
                CurrentTriggerEvents = _currentTriggerEvents,
                EndSimulationCommandBuffer = endSimulationCommandBuffer
            }.Schedule(Dependency);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private struct TriggerPhysicsEventsJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<EnemyTag> EnemyGroup;
            [ReadOnly] public ComponentDataFromEntity<PlayerTag> PlayerGroup;
            public NativeList<EquatableTriggerEvent> TriggerEvents;

            [BurstCompile]
            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;
                var isEntityAEnemy = EnemyGroup.HasComponent(entityA);
                var isEntityAPlayer = PlayerGroup.HasComponent(entityA);

                var isEntityBEnemy = EnemyGroup.HasComponent(entityB);
                var isEntityBPlayer = PlayerGroup.HasComponent(entityB);

                var (player, enemy) =
                    (isEntityAPlayer && isEntityBEnemy)
                        ? (entityA, entityB)
                        : (isEntityBPlayer && isEntityAEnemy)
                            ? (entityB, entityA)
                            : (Entity.Null, Entity.Null);

                if (player == Entity.Null || enemy == Entity.Null)
                    return;

                TriggerEvents.Add(new EquatableTriggerEvent { TriggerEvent = triggerEvent });
            }
        }

        [BurstCompile]
        private struct PlayerDamageJob : IJob
        {
            [ReadOnly] public ComponentDataFromEntity<EnemyTag> EnemyGroup;
            [ReadOnly] public ComponentDataFromEntity<PlayerTag> PlayerGroup;
            [ReadOnly] public ComponentDataFromEntity<DestroyTag> DestroyGroup;
            [ReadOnly] public ComponentDataFromEntity<EnemyDamage> DamageGroup;
            public ComponentDataFromEntity<PlayerHealth> HealthGroup;
            public float DeltaTime;

            public NativeParallelHashMap<EquatableTriggerEvent, float> PreviousTriggerEvents;
            public NativeList<EquatableTriggerEvent> CurrentTriggerEvents;

            public EntityCommandBuffer EndSimulationCommandBuffer;

            [BurstCompile]
            public void Execute()
            {
                var newPreviousTriggerEvents = new NativeParallelHashMap<EquatableTriggerEvent, float>(
                    CurrentTriggerEvents.Length, Allocator.Temp);

                foreach (var currentTriggerEvent in CurrentTriggerEvents)
                {
                    var enemy = EnemyGroup.HasComponent(currentTriggerEvent.TriggerEvent.EntityA)
                        ? currentTriggerEvent.TriggerEvent.EntityA
                        : currentTriggerEvent.TriggerEvent.EntityB;

                    var player = PlayerGroup.HasComponent(currentTriggerEvent.TriggerEvent.EntityA)
                        ? currentTriggerEvent.TriggerEvent.EntityA
                        : currentTriggerEvent.TriggerEvent.EntityB;

                    var damage = DamageGroup[enemy];
                    if (PreviousTriggerEvents.ContainsKey(currentTriggerEvent))
                    {
                        var timer = PreviousTriggerEvents[currentTriggerEvent];
                        timer += DeltaTime;
                        if (timer < damage.Cooldown)
                        {
                            newPreviousTriggerEvents.Add(currentTriggerEvent, timer);
                            break;
                        }
                    }

                    var health = HealthGroup[player];
                    var newHealth = health.Value - damage.Value;
                    if (newHealth < 0)
                         newHealth = 0;

                    health.Value = newHealth;
                    HealthGroup[player] = health;

                    if (newHealth <= math.EPSILON)
                    {
                        if (!DestroyGroup.HasComponent(player))
                            EndSimulationCommandBuffer.AddComponent<DestroyTag>(player);
                    }

                    newPreviousTriggerEvents.Add(currentTriggerEvent, 0);
                }

                PreviousTriggerEvents.Clear();

                foreach (var kvp in newPreviousTriggerEvents)
                    PreviousTriggerEvents.Add(kvp.Key, kvp.Value);

                newPreviousTriggerEvents.Dispose();
            }
        }

        [BurstCompile]
        protected override void OnDestroy()
        {
            _currentTriggerEvents.Dispose();
            _previousTriggerEvents.Dispose();
        }
    }
}