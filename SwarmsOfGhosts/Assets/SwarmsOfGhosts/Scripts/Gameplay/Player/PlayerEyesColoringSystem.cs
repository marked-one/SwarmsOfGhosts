using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerEyesColoringSystem : SystemBase
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
            var random = _randomSystem.MainThreadRandom;

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            var color = new float4(random.NextFloat3(), 1.0f);

            Entities
                .ForEach((
                    int nativeThreadIndex,
                    int entityInQueryIndex,
                    Entity entity,
                    in PlayerEyeTag playerEyeTag) =>
                {
                    beginSimulationCommandBuffer.AddComponent<URPMaterialPropertyBaseColor>(entityInQueryIndex, entity);
                    beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                        new URPMaterialPropertyBaseColor { Value = color });
                })
                .ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency); ;
        }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}