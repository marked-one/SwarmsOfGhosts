using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    public struct PlayerMovement : IComponentData
    {
        public float2 Value;
    }
}