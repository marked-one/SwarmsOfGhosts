using SwarmsOfGhosts.Gameplay.Pause;
using SwarmsOfGhosts.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Utilities
{
    // NOTE:
    // 1. destroyTag is added by any system to an EndSimulationCommandBuffer
    // 2. Next frame after EndSimulationCommandBuffer executes, this system
    // updates as part of the InitializationSystemGroup and requests entity
    // destruction in BeginInitializationEntityCommandBufferSystem
    // 3. BeginInitializationEntityCommandBufferSystem then executes on the same frame.

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class EntityDestructionSystem : SystemBase
    {
        private PauseSystem _pauseSystem;
        private BeginInitializationEntityCommandBufferSystem _beginInitializationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

            _beginInitializationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var beginSimulationCommandBuffer =
                _beginInitializationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in DestroyTag _) =>
            {
                beginSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _beginInitializationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}