using SwarmsOfGhosts.Gameplay.Restart;
using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class PlayerSpawnSystem : SystemBase
    {
        private RandomSystem _randomSystem;
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _randomSystem = World.GetOrCreateSystem<RandomSystem>();

            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

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
        protected override void OnUpdate() { }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, in PlayerTag _) => EntityManager.DestroyEntity(entity))
                .Run();

            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, in PlayerSpawnTag _) =>
                {
                    EntityManager.RemoveComponent<PlayerSpawnCounter>(entity);
                })
                .Run();
        }
    }
}