using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.Gameplay.Player
{
    public struct PlayerMovement : IComponentData
    {
        public float2 Value;
    }
}