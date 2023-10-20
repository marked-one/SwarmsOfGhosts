using System;
using SwarmsOfGhosts.Gameplay.Environment;
using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(RandomSystem))]
    public partial class EnemySpawnSystem : SystemBase
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
            var randomArray = _randomSystem.RandomArray;

            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((
                    int nativeThreadIndex,
                    int entityInQueryIndex,
                    in Translation enemySpawnTranslation,
                    in EnemySpawnSettings enemySpawnSettings,
                    in EnemySettings enemySettings,
                    in BattleGroundSettings battleGroundSettings,
                    in EnemySpawnTag enemySpawnTag) =>
                {
                    var enemyGridDimensionSize = enemySpawnSettings.GridDimensionSize;
                    var enemySpread = enemySpawnSettings.Spread;

                    var random = randomArray[nativeThreadIndex];

                    InstantiateBattleGround(entityInQueryIndex, ref beginSimulationCommandBuffer, battleGroundSettings,
                        enemySpawnTranslation, enemyGridDimensionSize, enemySpread, ref random);

                    InstantiateEnemies(entityInQueryIndex, ref beginSimulationCommandBuffer, enemySpawnTranslation,
                        enemySpawnSettings, enemySettings, enemyGridDimensionSize, enemySpread, ref random);
                })
                .ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private static void InstantiateBattleGround(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in BattleGroundSettings battleGroundSettings,
            in Translation enemySpawnTranslation,
            int enemyGridDimensionSize, float enemySpread, ref Random random)
        {
            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, battleGroundSettings.Prefab);

            var battleGroundPosition = new float3(enemySpawnTranslation.Value.x, 0f, enemySpawnTranslation.Value.z);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = battleGroundPosition });

            var battleGroundScale = (enemyGridDimensionSize - 1) * enemySpread * 1.5f;
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity, new NonUniformScale
                { Value = new float3(battleGroundScale, battleGroundSettings.YScale, battleGroundScale) });

            beginSimulationCommandBuffer.AddComponent<BattleGroundScale>(entityInQueryIndex, entity);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new BattleGroundScale { Value = battleGroundScale });

            beginSimulationCommandBuffer.AddComponent<URPMaterialPropertyBaseColor>(entityInQueryIndex, entity);
            var color = new float4(random.NextFloat3(), 1f);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = color });
        }

        [BurstCompile]
        private static void InstantiateEnemies(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation spawnTranslation,
            in EnemySpawnSettings spawnSettings,
            in EnemySettings enemySettings,
            int enemyGridDimensionSize, float enemySpread, ref Random random)
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
                        spawnTranslation, spawnSettings, enemySettings, xOffset, zOffset, ref random);
                }
            }
        }

        [BurstCompile]
        private static void InstantiateEnemy(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation spawnTranslation,
            in EnemySpawnSettings spawnSettings,
            in EnemySettings enemySettings,
            float xOffset, float zOffset, ref Random random)
        {
            var enemyPosition = new float3(
                spawnTranslation.Value.x + xOffset,
                spawnTranslation.Value.y,
                spawnTranslation.Value.z + zOffset);

            var entity = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, spawnSettings.Prefab);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new Translation { Value = enemyPosition });

            beginSimulationCommandBuffer.AddComponent<EnemyMovementSpeed>(entityInQueryIndex, entity);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity, new EnemyMovementSpeed
                { Value = random.NextFloat(enemySettings.SpeedRange.x, enemySettings.SpeedRange.y) });

            beginSimulationCommandBuffer.AddComponent<URPMaterialPropertyBaseColor>(entityInQueryIndex, entity);
            var alpha = random.NextFloat(enemySettings.TransparencyRange.x, enemySettings.TransparencyRange.y);
            var color = new float4(random.NextFloat3(), alpha);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, entity,
                new URPMaterialPropertyBaseColor { Value = color });
        }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}