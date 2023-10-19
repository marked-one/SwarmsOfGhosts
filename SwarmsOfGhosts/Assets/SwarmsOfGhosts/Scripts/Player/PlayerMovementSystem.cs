using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.Player
{
    [BurstCompile]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class PlayerMovementSystem : SystemBase
    {
        [BurstCompile]
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((
                ref Translation translation,
                ref Rotation rotation,
                in PlayerMovement movement,
                in PlayerMovementSpeed speed) =>
            {
                translation.Value.xz += movement.Value * speed.Value * deltaTime;

                if (math.lengthsq(movement.Value) > float.Epsilon)
                {
                    var forward = new float3(movement.Value.x, 0f, movement.Value.y);
                    rotation.Value = quaternion.LookRotation(forward, math.up());
                }
            }).ScheduleParallel();
        }
    }
}