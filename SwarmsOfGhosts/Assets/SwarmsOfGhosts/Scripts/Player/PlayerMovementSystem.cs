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
                in PlayerMovement playerMovement,
                in PlayerMovementSpeed playerSpeed) => {
                translation.Value.xz += playerMovement.Value * playerSpeed.Value * deltaTime;

                if (math.lengthsq(playerMovement.Value) > float.Epsilon)
                {
                    var forward = new float3(playerMovement.Value.x, 0f, playerMovement.Value.y);
                    rotation.Value = quaternion.LookRotation(forward, math.up());
                }
            }).Schedule();
        }
    }
}