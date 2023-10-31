using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    public struct PlayerHealth : IComponentData
    {
        public float Max;
        public float Value;
    }
}