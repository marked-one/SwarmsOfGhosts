using SwarmsOfGhosts.Gameplay.Environment;
using SwarmsOfGhosts.Gameplay.Restart;
using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using Material = Unity.Physics.Material;
using Random = Unity.Mathematics.Random;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(RandomSystem))]
    public partial class EnemySpawnSystem : SystemBase
    {
        public struct ColliderDefaults
        {
            public float CapsuleDefaultRadius;
            public float3 CapsuleDefaultVertex0;
            public float3 CapsuleDefaultVertex1;
            public float DefaultScale;
            public int ScaleStep;
        }

        private RandomSystem _randomSystem;
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        private EntityQuery _spawnsQuery;
        public NativeArray<int> ColliderCacheSizes { get; private set; }
        public NativeArray<BlobAssetReference<Collider>> ColliderCaches { get; private set; }

        [BurstCompile]
        protected override void OnCreate()
        {
            _randomSystem = World.GetOrCreateSystem<RandomSystem>();

            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _endSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _spawnsQuery = GetEntityQuery(
                ComponentType.ReadOnly<EnemySettings>(),
                ComponentType.ReadOnly<EnemySpawnTag>());

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            var spawnEntities = _spawnsQuery.ToEntityArrayAsync(Allocator.TempJob, out var spawnEntitiesJob);
            var colliderDefaults = new NativeArray<ColliderDefaults>(spawnEntities.Length, Allocator.Persistent);
            var collidersCacheSizes = new NativeArray<int>(spawnEntities.Length, Allocator.Persistent);
            spawnEntities.Dispose();

            var jobs = new NativeArray<JobHandle>(3, Allocator.Temp);
            jobs[0] = Dependency;
            jobs[1] = spawnEntitiesJob;
            var combinedJobs = JobHandle.CombineDependencies(jobs);

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            var randomArray = _randomSystem.RandomArray;
            Dependency = Entities
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((
                    int nativeThreadIndex,
                    int entityInQueryIndex,
                    in Translation enemySpawnTranslation,
                    in EnemySpawnSettings enemySpawnSettings,
                    in EnemySettings enemySettings,
                    in BattleGroundSettings battleGroundSettings,
                    in EnemySpawnTag _) =>
                {
                    var enemyGridDimensionSize = enemySpawnSettings.GridDimensionSize;
                    var enemySpread = enemySpawnSettings.Spread;

                    var random = randomArray[nativeThreadIndex];

                    InstantiateBattleGround(entityInQueryIndex, ref beginSimulationCommandBuffer, battleGroundSettings,
                        enemySpawnTranslation, enemyGridDimensionSize, enemySpread, ref random);

                    InstantiateEnemies(entityInQueryIndex, ref beginSimulationCommandBuffer, enemySpawnTranslation,
                        enemySpawnSettings, enemySettings, enemyGridDimensionSize, enemySpread, ref random);

                    var prefabPhysicsCollider = GetComponent<PhysicsCollider>(enemySpawnSettings.Prefab);
                    CreateColliderDefaults(out var enemyColliderSettings, prefabPhysicsCollider, enemySettings);
                    colliderDefaults[entityInQueryIndex] = enemyColliderSettings;
                    collidersCacheSizes[entityInQueryIndex] = CalculateCollidersCacheSize(enemySettings);

                    randomArray[nativeThreadIndex] = random;
                })
                .ScheduleParallel(combinedJobs);

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency.Complete();

            unsafe
            {
                var colliderCachesSize = CalculateColliderCachesSize(
                    (int*)collidersCacheSizes.GetUnsafeReadOnlyPtr(), collidersCacheSizes.Length);

                var colliderCaches =
                    new NativeArray<BlobAssetReference<Collider>>(colliderCachesSize, Allocator.Persistent);

                FillColliderCaches(
                    (int*)collidersCacheSizes.GetUnsafeReadOnlyPtr(), collidersCacheSizes.Length,
                    (ColliderDefaults*)colliderDefaults.GetUnsafeReadOnlyPtr(),
                    (BlobAssetReference<Collider>*)colliderCaches.GetUnsafeReadOnlyPtr());

                ColliderCaches = colliderCaches;
            }

            ColliderCacheSizes = collidersCacheSizes;

            colliderDefaults.Dispose();
        }

        [BurstCompile]
        private static unsafe int CalculateColliderCachesSize(int* collidersCacheSizes, int spawnsCount)
        {
            var colliderCachesSize = 0;
            for (var i = 0; i < spawnsCount; i++)
                colliderCachesSize += collidersCacheSizes[i];

            return colliderCachesSize;
        }

        [BurstCompile]
        private static unsafe void FillColliderCaches(
            int* collidersCacheSizes, int spawnsCount,
            ColliderDefaults* colliderDefaults,
            BlobAssetReference<Collider>* colliderCaches)
        {
            var collidersCacheOffset = 0;
            for (var i = 0; i < spawnsCount; i++)
            {
                var defaults = colliderDefaults[i];

                var size = collidersCacheSizes[i];
                var scale = defaults.DefaultScale;
                var step = defaults.ScaleStep / 100f;
                for (var j = collidersCacheOffset; j < collidersCacheOffset + size; j++)
                {
                    var capsuleGeometry = new CapsuleGeometry
                    {
                        Radius = defaults.CapsuleDefaultRadius * scale,
                        Vertex0 = defaults.CapsuleDefaultVertex0 * scale,
                        Vertex1 = defaults.CapsuleDefaultVertex1 * scale
                    };

                    var material = new Material { CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents };
                    colliderCaches[j] = CapsuleCollider.Create(capsuleGeometry, CollisionFilter.Default, material);

                    scale += step;
                }

                collidersCacheOffset += size;
            }
        }

        [BurstCompile]
        private static void InstantiateBattleGround(int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in BattleGroundSettings battleGroundSettings, in Translation enemySpawnTranslation,
            int enemyGridDimensionSize, float enemySpread, ref Random random)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, battleGroundSettings.Prefab);
            beginSimulationCommandBuffer.SetName(entityInQueryIndex, entity, $"BattleGround {entityInQueryIndex}");

            var battleGroundPosition = new float3(enemySpawnTranslation.Value.x, 0f, enemySpawnTranslation.Value.z);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = battleGroundPosition });

            var battleGroundScale = (enemyGridDimensionSize - 1) * enemySpread * 1.5f;
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity, new NonUniformScale
                { Value = new float3(battleGroundScale, battleGroundSettings.YScale, battleGroundScale) });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new BattleGroundScale { Value = battleGroundScale });

            var color = new float4(random.NextFloat3(), 1f);
            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = color });
        }

        [BurstCompile]
        private static void InstantiateEnemies(int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation spawnTranslation, in EnemySpawnSettings spawnSettings,
            in EnemySettings enemySettings, int enemyGridDimensionSize, float enemySpread,
            ref Random random)
        {
            var halfEnemyGridDimensionSize = enemyGridDimensionSize / 2;
            var lowerBound = -halfEnemyGridDimensionSize;
            var isEven = enemyGridDimensionSize % 2 == 0;
            var offset = isEven ? 0.5f : 0f;
            var upperBound = isEven ? halfEnemyGridDimensionSize : halfEnemyGridDimensionSize + 1;
            for (var i = lowerBound; i < upperBound; i++)
            {
                for (var j = lowerBound; j < upperBound; j++)
                {
                    var xOffset = (i + offset) * enemySpread;
                    var zOffset = (j + offset) * enemySpread;
                    var toCenter = float2.zero - new float2(xOffset, zOffset);
                    var distanceToCenter = math.length(toCenter);
                    if (distanceToCenter < spawnSettings.HeadroomInCenter)
                        continue;

                    InstantiateEnemy(entityInQueryIndex, ref beginSimulationCommandBuffer,
                        spawnTranslation, spawnSettings, enemySettings, xOffset, zOffset, i, j, ref random);
                }
            }
        }

        [BurstCompile]
        private static void InstantiateEnemy(int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation spawnTranslation, in EnemySpawnSettings spawnSettings,
            in EnemySettings enemySettings, float xOffset, float zOffset,
            int i, int j, ref Random random)
        {
            var enemyPosition = new float3(
                spawnTranslation.Value.x + xOffset,
                spawnTranslation.Value.y,
                spawnTranslation.Value.z + zOffset);

            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, spawnSettings.Prefab);
            beginSimulationCommandBuffer.SetName(entityInQueryIndex, entity, $"Enemy {i} {j}");

            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = enemyPosition });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity, new EnemyMovementSpeed
            {
                Value = random.NextFloat(enemySettings.SpeedRange.x, enemySettings.SpeedRange.y),
            });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new Scale { Value = enemySettings.Scale });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new EnemySpawnId { Value = entityInQueryIndex });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new EnemyGrowth { Step = enemySettings.GrowthStep, Limit = enemySettings.GrowthLimit });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new EnemyHealth { Value = enemySettings.Health, Max = enemySettings.Health });

            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new EnemyDamage { Value = enemySettings.Damage, Cooldown = enemySettings.DamageCooldown });

            var alpha = random.NextFloat(enemySettings.TransparencyRange.x, enemySettings.TransparencyRange.y);
            var color = new float4(random.NextFloat3(), alpha);
            beginSimulationCommandBuffer.AddComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = color });
        }

        [BurstCompile]
        private static unsafe void CreateColliderDefaults(
            out ColliderDefaults colliderDefaults,
            in PhysicsCollider prefabCollider,
            in EnemySettings enemySettings)
        {
            var colliderPtr = (CapsuleCollider*)prefabCollider.ColliderPtr;
            colliderDefaults = new ColliderDefaults
            {
                CapsuleDefaultRadius = colliderPtr->Radius,
                CapsuleDefaultVertex0 = colliderPtr->Vertex0,
                CapsuleDefaultVertex1 = colliderPtr->Vertex1,
                DefaultScale = enemySettings.Scale,
                ScaleStep = enemySettings.GrowthStep
            };
        }

        [BurstCompile]
        private static int CalculateCollidersCacheSize(in EnemySettings enemySettings)
        {
            var min = enemySettings.Scale;
            var max = enemySettings.GrowthLimit;
            var step = enemySettings.GrowthStep / 100f;
            return (int)math.round((max - min) / step) + 1;
        }

        [BurstCompile]
        protected override void OnUpdate() { }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            var endSimulationCommandBuffer =
                _endSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in EnemyTag _) =>
            {
                endSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();


            Entities.ForEach((Entity entity, int entityInQueryIndex, in BattleGroundTag _) =>
            {
                endSimulationCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency.Complete();

            if (ColliderCacheSizes.IsCreated)
                ColliderCacheSizes.Dispose();

            if (ColliderCaches.IsCreated)
            {
                for (var i = 0; i < ColliderCaches.Length; i++)
                {
                    if(ColliderCaches[i].IsCreated)
                        ColliderCaches[i].Dispose();
                }

                ColliderCaches.Dispose();
            }
        }
    }
}