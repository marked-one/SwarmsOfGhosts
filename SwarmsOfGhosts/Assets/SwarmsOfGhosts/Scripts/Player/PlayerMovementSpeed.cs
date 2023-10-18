using Unity.Entities;

namespace SwarmsOfGhosts.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerMovementSpeed : IComponentData
    {
        public float Value;
    }
}