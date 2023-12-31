﻿using SwarmsOfGhosts.App.Gameplay.Randomize;
using SwarmsOfGhosts.App.Gameplay.Restart;
using UniRx;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class PlayerSpawnSystem : SystemBase
    {
        private RandomSystem _randomSystem;
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        private EntityQuery _playerQuery;

        private readonly ReactiveProperty<bool> _isPlayerAlive = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsPlayerAlive => _isPlayerAlive;

        [BurstCompile]
        protected override void OnCreate()
        {
            _randomSystem = World.GetOrCreateSystem<RandomSystem>();

            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>());

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity entity, in PlayerSpawnTag _) =>
                {
                    EntityManager.AddComponent<PlayerSpawnCounter>(entity);
                })
                .Run();

            var randomArray = _randomSystem.RandomArray;

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((
                    int nativeThreadIndex,
                    int entityInQueryIndex,
                    ref PlayerSpawnCounter counter,
                    in Translation spawnTranslation,
                    in PlayerSpawnSettings spawnSettings,
                    in PlayerSettings playerSettings,
                    in PlayerSpawnTag _) =>
                {
                    var random = randomArray[nativeThreadIndex];

                    InstantiatePlayer(entityInQueryIndex, ref beginSimulationCommandBuffer,
                        ref counter, spawnTranslation, spawnSettings, playerSettings, ref random);

                    randomArray[nativeThreadIndex] = random;
                })
                .ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private static void InstantiatePlayer(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            ref PlayerSpawnCounter counter,
            in Translation spawnTranslation,
            in PlayerSpawnSettings spawnSettings,
            in PlayerSettings playerSettings,
            ref Random random)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, spawnSettings.Prefab);
            beginSimulationCommandBuffer.SetName(entityInQueryIndex, entity, $"Player {counter.Value}");

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = spawnTranslation.Value });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new PlayerMovementSpeed { Value = playerSettings.Speed });

            beginSimulationCommandBuffer.AddComponent<PlayerMovement>(entityInQueryIndex, entity);

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new PlayerHealth { Value = playerSettings.Health, Max = playerSettings.Health });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = new float4(random.NextFloat3(), 1.0f) });

            counter.Value++;
        }

        [BurstCompile]
        protected override void OnUpdate() =>
            _isPlayerAlive.Value = _playerQuery.ToEntityArray(Allocator.Temp).Length > 0;

        [BurstCompile]
        protected override void OnStopRunning()
        {
            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in PlayerTag _) =>
            {
                endSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in PlayerSpawnTag _) =>
            {
                endSimulationCommandBuffer.RemoveComponent<PlayerSpawnCounter>(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            _isPlayerAlive.Value = false;

        }
    }
}