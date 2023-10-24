using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Player
{
    public struct PlayerHealth : IComponentData
    {
        public float Max;
        public float Value;
    }
}