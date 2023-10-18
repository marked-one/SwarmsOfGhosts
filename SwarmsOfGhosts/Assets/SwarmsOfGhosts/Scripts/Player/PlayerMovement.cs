using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerMovement : IComponentData
    {
        public float2 Value;
    }
}