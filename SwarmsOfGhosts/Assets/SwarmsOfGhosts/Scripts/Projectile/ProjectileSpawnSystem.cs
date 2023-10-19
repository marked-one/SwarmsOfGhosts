using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Projectile
{
    [BurstCompile]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class ProjectileSpawnSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in ProjectileSettings settings) =>
            {
                endSimulationCommandBuffer.AddComponent<ProjectileSpawnTimer>(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((
                int entityInQueryIndex,
                ref ProjectileSpawnTimer timer,
                in Translation translation,
                in LocalToWorld localToWorld,
                in ProjectileSettings settings) =>
            {
                var waitTime = settings.SpawnInterval;
                var time = timer.Value;
                time += deltaTime;
                if (time < waitTime)
                {
                    timer.Value = time;
                }
                else
                {
                    timer.Value = 0f;

                    SpawnProjectile(
                        ref beginSimulationCommandBuffer,
                        entityInQueryIndex,
                        translation,
                        localToWorld,
                        settings);
                }
            }).ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        private static void SpawnProjectile(
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            int entityInQueryIndex,
            in Translation translation,
            in LocalToWorld localToWorld,
            in ProjectileSettings settings)
        {
            var worldPosition = math.transform(localToWorld.Value, translation.Value);
            var worldRotation = quaternion.LookRotation(localToWorld.Forward, localToWorld.Up);

            var projectile = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, settings.Prefab);

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, projectile,
                new Translation { Value = worldPosition });

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, projectile,
                new Rotation { Value = worldRotation });

            beginSimulationCommandBuffer.AddComponent<ProjectileStartPosition>(entityInQueryIndex,
                projectile);

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, projectile,
                new ProjectileStartPosition { Value = worldPosition });

            beginSimulationCommandBuffer.AddComponent<ProjectileSpeed>(entityInQueryIndex, projectile);

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, projectile,
                new ProjectileSpeed { Value = settings.Speed });

            beginSimulationCommandBuffer.AddComponent<ProjectileDestroyDistance>(entityInQueryIndex,
                projectile);

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, projectile,
                new ProjectileDestroyDistance { Value = settings.DestroyDistance });
        }
    }
}