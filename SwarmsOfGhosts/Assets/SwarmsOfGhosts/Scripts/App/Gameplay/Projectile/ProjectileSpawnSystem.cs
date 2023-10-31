using SwarmsOfGhosts.App.Gameplay.Enemy;
using SwarmsOfGhosts.App.Gameplay.Environment;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.App.Gameplay.Projectile
{
    [BurstCompile]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class ProjectileSpawnSystem : SystemBase
    {
        private PauseSystem _pauseSystem;

        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        private EntityQuery _battleGroundQuery;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _battleGroundQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<BattleGroundScale>());

            var enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            RequireForUpdate(enemyQuery);

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in ProjectileSpawnTag _) =>
            {
                endSimulationCommandBuffer.AddComponent<ProjectileSpawnTimer>(entityInQueryIndex, entity);
                endSimulationCommandBuffer.AddComponent<ProjectileSpawnCounter>(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

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
                    ref ProjectileSpawnCounter counter,
                    in Translation translation,
                    in LocalToWorld localToWorld,
                    in ProjectileSpawnSettings spawnSettings,
                    in ProjectileSettings settings,
                    in ProjectileSpawnTag _) =>
                {
                    var waitTime = spawnSettings.Cooldown;
                    var time = timer.Value;
                    time += deltaTime;
                    if (time < waitTime)
                        timer.Value = time;
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
                                ref counter, worldPosition, worldRotation, spawnSettings, settings);
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
            ref ProjectileSpawnCounter counter,
            in float3 worldPosition,
            in quaternion worldRotation,
            in ProjectileSpawnSettings spawnSettings,
            in ProjectileSettings settings)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, spawnSettings.Prefab);
            beginSimulationCommandBuffer.SetName(entityInQueryIndex, entity, $"Projectile {counter.Value}");

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

            counter.Value++;
        }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in ProjectileTag _) =>
            {
                endSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in ProjectileSpawnTag _) =>
            {
                endSimulationCommandBuffer.RemoveComponent<ProjectileSpawnTimer>(entityInQueryIndex, entity);
                endSimulationCommandBuffer.RemoveComponent<ProjectileSpawnCounter>(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}