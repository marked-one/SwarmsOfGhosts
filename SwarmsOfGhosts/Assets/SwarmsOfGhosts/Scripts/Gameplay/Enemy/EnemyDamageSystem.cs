using SwarmsOfGhosts.Gameplay.Pause;
using SwarmsOfGhosts.Gameplay.Player;
using SwarmsOfGhosts.Gameplay.Projectile;
using SwarmsOfGhosts.Gameplay.Restart;
using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateBefore(typeof(EnemyGrowthSystem))]
    public partial class EnemyDamageSystem : SystemBase
    {
        private PauseFacadeSystem _pauseSystem;
        private StepPhysicsWorld _stepPhysicsWorld;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseFacadeSystem>();
            _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
            RequireSingletonForUpdate<PlayerTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var endSimulationCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
            Dependency = new TriggerPhysicsEventsJob
            {
                EnemyGroup = GetComponentDataFromEntity<EnemyTag>(true),
                ProjectileGroup = GetComponentDataFromEntity<ProjectileTag>(true),
                DamageGroup = GetComponentDataFromEntity<ProjectileDamage>(true),
                DestroyGroup = GetComponentDataFromEntity<DestroyTag>(true),
                EnemyHealthGroup = GetComponentDataFromEntity<EnemyHealth>(),
                PlayerHealthGroup = GetComponentDataFromEntity<PlayerHealth>(),
                Player = GetSingletonEntity<PlayerTag>(),
                EndSimulationCommandBuffer = endSimulationCommandBuffer
            }.Schedule(_stepPhysicsWorld.Simulation, Dependency);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private struct TriggerPhysicsEventsJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<EnemyTag> EnemyGroup;
            [ReadOnly] public ComponentDataFromEntity<ProjectileTag> ProjectileGroup;
            [ReadOnly] public ComponentDataFromEntity<ProjectileDamage> DamageGroup;
            [ReadOnly] public ComponentDataFromEntity<DestroyTag> DestroyGroup;
            public ComponentDataFromEntity<EnemyHealth> EnemyHealthGroup;
            public ComponentDataFromEntity<PlayerHealth> PlayerHealthGroup;
            public Entity Player;
            public EntityCommandBuffer EndSimulationCommandBuffer;

            [BurstCompile]
            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var isEntityAEnemy = EnemyGroup.HasComponent(entityA);
                var isEntityAProjectile = ProjectileGroup.HasComponent(entityA);

                var entityB = triggerEvent.EntityB;
                var isEntityBEnemy = EnemyGroup.HasComponent(entityB);
                var isEntityBProjectile = ProjectileGroup.HasComponent(entityB);

                var (enemy, projectile) =
                    (isEntityAEnemy && isEntityBProjectile)
                        ? (entityA, entityB)
                        : (isEntityAProjectile && isEntityBEnemy)
                            ? (entityB, entityA)
                            : (Entity.Null, Entity.Null);

                if (enemy == Entity.Null || projectile == Entity.Null)
                    return;

                var health = EnemyHealthGroup[enemy];
                var damage = DamageGroup[projectile];
                var newHealth = health.Value - damage.Value;
                if (newHealth < 0)
                    newHealth = 0;

                health.Value = newHealth;
                EnemyHealthGroup[enemy] = health;

                if (!DestroyGroup.HasComponent(projectile))
                    EndSimulationCommandBuffer.AddComponent<DestroyTag>(projectile);

                if (newHealth > math.EPSILON)
                    return;

                var playerHealth = PlayerHealthGroup[Player];
                playerHealth.Value = playerHealth.Max;
                PlayerHealthGroup[Player] = playerHealth;

                if (!DestroyGroup.HasComponent(enemy))
                    EndSimulationCommandBuffer.AddComponent<DestroyTag>(enemy);
            }
        }
    }
}