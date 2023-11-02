using SwarmsOfGhosts.App.Gameplay.Destruction;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Randomize;
using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using Random = Unity.Mathematics.Random;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    public partial class EnemyGrowthSystem : SystemBase
    {
        private PauseSystem _pauseSystem;
        private EnemySpawnSystem _spawnSystem;
        private RandomSystem _randomSystem;
        private StepPhysicsWorld _stepPhysicsWorld;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();
            _spawnSystem = World.GetOrCreateSystem<EnemySpawnSystem>();
            _randomSystem = World.GetOrCreateSystem<RandomSystem>();
            _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var collidersCacheSizes = _spawnSystem.ColliderCacheSizes;
            var collidersCache = _spawnSystem.ColliderCaches;
            if (!collidersCacheSizes.IsCreated || !collidersCache.IsCreated)
                return;

            var random = _randomSystem.MainThreadRandom;
            var endSimulationCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            Dependency = new TriggerPhysicsEventsJob
            {
                Seed = random.NextInt(1, int.MaxValue),
                EnemyGroup = GetComponentDataFromEntity<EnemyTag>(true),
                SpawnIdGroup = GetComponentDataFromEntity<EnemySpawnId>(true),
                GrowthGroup = GetComponentDataFromEntity<EnemyGrowth>(true),
                DestroyGroup = GetComponentDataFromEntity<DestroyTag>(true),
                SpeedGroup = GetComponentDataFromEntity<EnemyMovementSpeed>(),
                HealthGroup = GetComponentDataFromEntity<EnemyHealth>(),
                DamageGroup = GetComponentDataFromEntity<EnemyDamage>(),
                ScaleGroup = GetComponentDataFromEntity<Scale>(),
                ColliderGroup = GetComponentDataFromEntity<PhysicsCollider>(),
                ColorGroup = GetComponentDataFromEntity<URPMaterialPropertyBaseColor>(),
                CollidersCacheSizes = collidersCacheSizes,
                CollidersCache = collidersCache,
                EndSimulationCommandBuffer = endSimulationCommandBuffer
            }.Schedule(_stepPhysicsWorld.Simulation, Dependency);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            _randomSystem.MainThreadRandom = random;
        }

        [BurstCompile]
        private struct TriggerPhysicsEventsJob : ITriggerEventsJob
        {
            public int Seed;

            [ReadOnly] public ComponentDataFromEntity<EnemyTag> EnemyGroup;
            [ReadOnly] public ComponentDataFromEntity<EnemySpawnId> SpawnIdGroup;
            [ReadOnly] public ComponentDataFromEntity<EnemyGrowth> GrowthGroup;
            [ReadOnly] public ComponentDataFromEntity<DestroyTag> DestroyGroup;

            public ComponentDataFromEntity<EnemyMovementSpeed> SpeedGroup;
            public ComponentDataFromEntity<EnemyHealth> HealthGroup;
            public ComponentDataFromEntity<EnemyDamage> DamageGroup;
            public ComponentDataFromEntity<Scale> ScaleGroup;
            public ComponentDataFromEntity<PhysicsCollider> ColliderGroup;
            public ComponentDataFromEntity<URPMaterialPropertyBaseColor> ColorGroup;

            public NativeArray<int> CollidersCacheSizes;
            public NativeArray<BlobAssetReference<Collider>> CollidersCache;

            public EntityCommandBuffer EndSimulationCommandBuffer;

            [BurstCompile]
            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var isEntityAEnemy = EnemyGroup.HasComponent(entityA);

                var entityB = triggerEvent.EntityB;
                var isEntityBEnemy = EnemyGroup.HasComponent(entityB);

                if (!isEntityAEnemy || !isEntityBEnemy)
                    return;

                var scaleA = ScaleGroup[entityA];
                var scaleB = ScaleGroup[entityB];
                var speedA = SpeedGroup[entityA];
                var speedB = SpeedGroup[entityB];
                var (entityToGrow, scale, speed, entityToDestroy)
                    = scaleA.Value >= scaleB.Value
                        ? (entityA, scaleA, speedA, entityB)
                        : (entityB, scaleB, speedB, entityA);

                speed.Value = speedA.Value >= speedB.Value ? speedA.Value : speedB.Value;
                SpeedGroup[entityToGrow] = speed;

                var growth = GrowthGroup[entityToGrow];
                if (math.abs(math.min(scale.Value, growth.Limit) - growth.Limit) < math.EPSILON)
                    return;

                var growthStep = growth.Step;
                var newScale = Rescale(ref scale, growthStep, growth.Limit);
                ScaleGroup[entityToGrow] = scale;

                unsafe
                {
                    var spawnId = SpawnIdGroup[entityToGrow].Value;
                    var physicsCollider = ColliderGroup[entityToGrow];

                    ReplaceCollider(ref physicsCollider,
                        (int*)CollidersCacheSizes.GetUnsafeReadOnlyPtr(), spawnId, newScale, growthStep,
                        (BlobAssetReference<Collider>*)CollidersCache.GetUnsafePtr());

                    ColliderGroup[entityToGrow] = physicsCollider;
                }

                var healthOfGrowingEntity = HealthGroup[entityToGrow];
                var healthOfDestroyedEntity = HealthGroup[entityToDestroy];
                healthOfGrowingEntity.Value += healthOfDestroyedEntity.Value;
                HealthGroup[entityToGrow] = healthOfGrowingEntity;

                var damageOfGrowingEntity = DamageGroup[entityToGrow];
                var damageOfDestroyedEntity = DamageGroup[entityToDestroy];
                damageOfGrowingEntity.Value += damageOfDestroyedEntity.Value;
                DamageGroup[entityToGrow] = damageOfGrowingEntity;

                var random = CreateRandom(Seed, triggerEvent.BodyIndexA, triggerEvent.BodyIndexB);
                var color = ColorGroup[entityToGrow];
                ChangeMaterialColor(ref color, ref random);
                ColorGroup[entityToGrow] = color;

                if (!DestroyGroup.HasComponent(entityToDestroy))
                    EndSimulationCommandBuffer.AddComponent<DestroyTag>(entityToDestroy);
            }

            [BurstCompile]
            private static float Rescale(ref Scale scale, int growthStep, float growthLimit)
            {
                var growth = growthStep / 100f;
                var newScale = scale.Value + growth;
                if (newScale > growthLimit)
                    newScale = growthLimit;

                scale.Value = newScale;
                return newScale;
            }

            [BurstCompile]
            private static unsafe void ReplaceCollider(ref PhysicsCollider physicsCollider,
                int* collidersCacheSizesPtr, int spawnId, float newScale, int growthStep,
                BlobAssetReference<Collider>* collidersCachePtr)
            {
                var colliderIndex = CalculateColliderIndex(collidersCacheSizesPtr, spawnId, newScale, growthStep);
                var cachedCollider = collidersCachePtr[colliderIndex];
                if (cachedCollider.IsCreated)
                    physicsCollider.Value = cachedCollider;
            }

            [BurstCompile]
            private static unsafe int CalculateColliderIndex(
                int* collidersCacheSizes, int spawnId, float newScale, int growthStep)
            {
                var collidersCacheOffset = 0;
                for (var i = 0; i < spawnId; i++)
                    collidersCacheOffset += collidersCacheSizes[i];

                return collidersCacheOffset + (int)math.round((newScale - 1f) * (100f / growthStep));
            }

            [BurstCompile]
            private static Random CreateRandom(int seed, int bodyIndexA, int bodyIndexB)
            {
                // nativeThreadIndex is not available to ITriggerEventsJob in 0.51,
                // so try our best here to make our seed value unique enough.
                var finalSeed = seed;
                finalSeed += bodyIndexA * bodyIndexB;
                finalSeed = finalSeed == 0 ? 1 : finalSeed;
                return new Random((uint)finalSeed);
            }

            [BurstCompile]
            private static void ChangeMaterialColor(ref URPMaterialPropertyBaseColor color, ref Random random) =>
                color.Value = new float4(random.NextFloat3(new float3(1f, 1f, 1f)), color.Value.w);
        }
    }
}