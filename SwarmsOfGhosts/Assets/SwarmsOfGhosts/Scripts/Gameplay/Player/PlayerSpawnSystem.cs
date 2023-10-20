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
            _randomSystem = World.GetExistingSystem<RandomSystem>();

            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
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
                    in Translation spawnTranslation,
                    in PlayerSpawnSettings spawnSettings,
                    in PlayerSettings playerSettings,
                    in PlayerSpawnTag playerSpawnTag) =>
                {
                    var random = randomArray[nativeThreadIndex];
                    InstantiatePlayer(entityInQueryIndex, ref beginSimulationCommandBuffer,
                        spawnTranslation, spawnSettings, playerSettings, ref random);
                })
                .ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private static void InstantiatePlayer(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation spawnTranslation,
            in PlayerSpawnSettings spawnSettings,
            in PlayerSettings playerSettings,
            ref Random random)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, spawnSettings.Prefab);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = spawnTranslation.Value });

            beginSimulationCommandBuffer.AddComponent<PlayerMovementSpeed>(entityInQueryIndex, entity);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity, new PlayerMovementSpeed
                { Value = playerSettings.Speed });

            beginSimulationCommandBuffer.AddComponent<URPMaterialPropertyBaseColor>(entityInQueryIndex, entity);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = new float4(random.NextFloat3(), 1.0f) });
        }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}