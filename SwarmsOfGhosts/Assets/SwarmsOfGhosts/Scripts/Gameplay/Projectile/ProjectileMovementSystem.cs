using SwarmsOfGhosts.Gameplay.Player;
using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial class ProjectileMovementSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((
                Entity entity, int entityInQueryIndex,
                ref Translation translation, in Rotation rotation,
                in ProjectileStartPosition startPosition,
                in ProjectileSpeed speed,
                in ProjectileDestroyDistance destroyDistance) =>
            {
                var forward = math.normalize(math.forward(rotation.Value));
                translation.Value.xz += forward.xz * speed.Value * deltaTime;

                var path = translation.Value - startPosition.Value;
                var distance = math.length(path);
                if (distance <= destroyDistance.Value)
                    return;

                endSimulationCommandBuffer.AddComponent<DestroyTag>(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}