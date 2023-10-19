using SwarmsOfGhosts.Environment;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class EnemySpawnSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;

        [BurstCompile]
        protected override void OnCreate()
        {
            _beginSimulationEntityCommandBufferSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            var beginSimulationCommandBuffer =
                _beginSimulationEntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

            Entities.ForEach((
                int entityInQueryIndex,
                in Translation enemySpawnTranslation,
                in EnemySpawnSettings enemySpawnSettings,
                in EnemySettings enemySettings,
                in BattleGroundSettings battleGroundSettings,
                in EnemySpawnTag enemySpawnTag) =>
            {
                var enemyGridDimensionSize = enemySpawnSettings.GridDimensionSize;
                var enemySpread = enemySpawnSettings.Spread;

                InstantiateBattleGround(entityInQueryIndex, ref beginSimulationCommandBuffer,
                    battleGroundSettings, enemySpawnTranslation, enemyGridDimensionSize, enemySpread);

                InstantiateEnemies(entityInQueryIndex, ref beginSimulationCommandBuffer,
                    enemySpawnTranslation, enemySpawnSettings, enemySettings, enemyGridDimensionSize, enemySpread);
            }).ScheduleParallel();

            _beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        private static void InstantiateBattleGround(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in BattleGroundSettings battleGroundSettings,
            in Translation enemySpawnTranslation,
            int enemyGridDimensionSize, float enemySpread)
        {
            var ground = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, battleGroundSettings.Prefab);

            var battleGroundPosition = new float3(enemySpawnTranslation.Value.x, 0f, enemySpawnTranslation.Value.z);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, ground,
                new Translation { Value = battleGroundPosition });

            var battleGroundScale = (enemyGridDimensionSize - 1) * enemySpread * 1.5f;
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, ground, new NonUniformScale
                { Value = new float3(battleGroundScale, battleGroundSettings.YScale, battleGroundScale) });

            beginSimulationCommandBuffer.AddComponent<BattleGroundScale>(entityInQueryIndex, ground);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, ground,
                new BattleGroundScale { Value = battleGroundScale });
        }

        [BurstCompile]
        private static void InstantiateEnemies(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation enemySpawnTranslation,
            in EnemySpawnSettings enemySpawnSettings,
            in EnemySettings enemySettings,
            int enemyGridDimensionSize, float enemySpread)
        {
            var random = Random.CreateFromIndex((uint)entityInQueryIndex);
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
                    if (distanceToCenter < enemySpawnSettings.HeadroomInCenter)
                        continue;

                    InstantiateEnemy(entityInQueryIndex, ref beginSimulationCommandBuffer,
                        enemySpawnTranslation, enemySpawnSettings, enemySettings, xOffset, zOffset, ref random);
                }
            }
        }

        [BurstCompile]
        private static void InstantiateEnemy(
            int entityInQueryIndex,
            ref EntityCommandBuffer.ParallelWriter beginSimulationCommandBuffer,
            in Translation enemySpawnTranslation,
            in EnemySpawnSettings enemySpawnSettings,
            in EnemySettings enemySettings,
            float xOffset, float zOffset, ref Random random)
        {
            var enemyPosition =
                new float3(enemySpawnTranslation.Value.x + xOffset, 0f, enemySpawnTranslation.Value.z + zOffset);

            var enemy = beginSimulationCommandBuffer.Instantiate(entityInQueryIndex, enemySpawnSettings.Prefab);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, enemy,
                new Translation { Value = enemyPosition });

            beginSimulationCommandBuffer.AddComponent<EnemyMovementSpeed>(entityInQueryIndex, enemy);
            beginSimulationCommandBuffer.SetComponent(entityInQueryIndex, enemy, new EnemyMovementSpeed
                { Value = random.NextFloat(enemySettings.SpeedRange.x, enemySettings.SpeedRange.y) });
        }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}