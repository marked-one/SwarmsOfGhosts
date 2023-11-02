using SwarmsOfGhosts.App.Gameplay.Destruction;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Player;
using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.App.Gameplay.Projectile
{
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial class ProjectileMovementSystem : SystemBase
    {
        private PauseSystem _pauseSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var deltaTime = Time.DeltaTime;

            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            var destroyGroup = GetComponentDataFromEntity<DestroyTag>(true);
            Entities
                .WithReadOnly(destroyGroup)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref Translation translation, in Rotation rotation,
                    in ProjectileStartPosition startPosition,
                    in ProjectileSpeed speed,
                    in ProjectileDestroyDistance destroyDistance,
                    in ProjectileTag _) =>
                {
                    var forward = math.normalize(math.forward(rotation.Value));
                    translation.Value.xz += forward.xz * speed.Value * deltaTime;

                    var path = translation.Value - startPosition.Value;
                    var distance = math.length(path);
                    if (distance <= destroyDistance.Value)
                        return;

                    if (!destroyGroup.HasComponent(entity))
                        endSimulationCommandBuffer.AddComponent<DestroyTag>(entityInQueryIndex, entity);
                })
                .ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}