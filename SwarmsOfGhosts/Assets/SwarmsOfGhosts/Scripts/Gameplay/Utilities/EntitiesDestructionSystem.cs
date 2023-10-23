using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Utilities
{
    // NOTE:
    // 1. destroyTag is added by any system to an EndSimulationCommandBuffer
    // 2. Next frame after EndSimulationCommandBuffer executes, this system
    // updates as part of the InitializationSystemGroup and requests entity
    // destruction in BeginSimulationCommandBuffer
    // 3. BeginSimulationCommandBuffer then executes on the same frame.

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup ))]
    public partial class EntitiesDestructionSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        [BurstCompile]
        protected override void OnCreate()
        {
            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in DestroyTag destroyTag) =>
            {
                beginSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}