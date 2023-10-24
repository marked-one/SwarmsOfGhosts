using SwarmsOfGhosts.Gameplay.Enemy;
using SwarmsOfGhosts.Gameplay.Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [BurstCompile]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class ProjectileSpawnSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        private EntityQuery _battleGroundQuery;

        [BurstCompile]
        protected override void OnCreate()
        {
            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _battleGroundQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<BattleGroundScale>());

            var enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            RequireForUpdate(enemyQuery);
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
            var battleGroundEntities = _battleGroundQuery
                .ToEntityArrayAsync(Allocator.TempJob, out var battleGroundJobHandle);

            var battleGroundTranslations = _battleGroundQuery
                .ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var battleGroundTranslationsJobHandle);

            var battleGroundScales = _battleGroundQuery
                .ToComponentDataArrayAsync<BattleGroundScale>(Allocator.TempJob, out var battleGroundScalesJobHandle);

            var jobHandles = new NativeArray<JobHandle>(4, Allocator.Temp);
            jobHandles[0] = Dependency;
            jobHandles[1] = battleGroundJobHandle;
            jobHandles[2] = battleGroundTranslationsJobHandle;
            jobHandles[3] = battleGroundScalesJobHandle;
            var combinedJobHandle = JobHandle.CombineDependencies(jobHandles);

            var deltaTime = Time.DeltaTime;

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Dependency = Entities
                .WithDisposeOnCompletion(battleGroundEntities)
                .WithDisposeOnCompletion(battleGroundTranslations)
                .WithDisposeOnCompletion(battleGroundScales)
                .ForEach((
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

                        var worldPosition = math.transform(localToWorld.Value, translation.Value);
                        var isInBattleGround = false;
                        for (var i = 0; i < battleGroundEntities.Length; i++)
                        {
                            var battleGroundTranslation = battleGroundTranslations[i];
                            var battleGroundScale = battleGroundScales[i];
                            var pathToBattleGroundCenter = worldPosition - battleGroundTranslation.Value;
                            var battleGroundRadius = battleGroundScale.Value / 2;
                            var distanceToBattleGroundCenter = math.length(pathToBattleGroundCenter);
                            if (distanceToBattleGroundCenter <= battleGroundRadius)
                                isInBattleGround = true;
                        }

                        if (isInBattleGround)
                        {
                            var worldRotation = quaternion.LookRotation(localToWorld.Forward, localToWorld.Up);
                            InstantiateProjectile(entityInQueryIndex, ref beginSimulationCommandBuffer,
                                worldPosition, worldRotation, settings);
                        }
                    }
                })
                .ScheduleParallel(combinedJobHandle);

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private static void InstantiateProjectile(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in float3 worldPosition,
            in quaternion worldRotation,
            in ProjectileSettings settings)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, settings.Prefab);

            beginSimulationCommandBuffer.SetComponent(
                entityInQueryIndex, entity, new Translation { Value = worldPosition });

            beginSimulationCommandBuffer.SetComponent(
                entityInQueryIndex, entity, new Rotation { Value = worldRotation });

            beginSimulationCommandBuffer.AddComponent(
                entityInQueryIndex, entity, new ProjectileStartPosition { Value = worldPosition });

            beginSimulationCommandBuffer.AddComponent(
                entityInQueryIndex, entity, new ProjectileSpeed { Value = settings.Speed });

            beginSimulationCommandBuffer.AddComponent(
                entityInQueryIndex, entity, new ProjectileDestroyDistance { Value = settings.DestroyDistance });

            beginSimulationCommandBuffer.AddComponent(
                entityInQueryIndex, entity, new ProjectileDamage() { Value = settings.Damage });
        }
    }
}