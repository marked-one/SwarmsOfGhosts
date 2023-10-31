using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    public struct EnemyHealth : IComponentData
    {
        public float Value;
        public float Max;
    }
}