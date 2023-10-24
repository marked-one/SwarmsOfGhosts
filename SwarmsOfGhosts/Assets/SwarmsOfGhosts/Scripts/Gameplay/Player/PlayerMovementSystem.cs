using SwarmsOfGhosts.Gameplay.Enemy;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial class PlayerMovementSystem : SystemBase
    {
        [BurstCompile]
        protected override void OnCreate()
        {
            var enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            RequireForUpdate(enemyQuery);
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((ref Translation translation, ref Rotation rotation,
                in PlayerMovement movement, in PlayerMovementSpeed speed) =>
            {
                translation.Value.xz += movement.Value * speed.Value * deltaTime;
                if (math.length(movement.Value) > math.EPSILON)
                {
                    var forward = new float3(movement.Value.x, 0f, movement.Value.y);
                    rotation.Value = quaternion.LookRotation(forward, math.up());
                }
            }).ScheduleParallel();
        }
    }
}