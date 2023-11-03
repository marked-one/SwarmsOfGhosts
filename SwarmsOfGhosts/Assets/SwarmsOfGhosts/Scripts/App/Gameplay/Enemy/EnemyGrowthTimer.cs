using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    public struct EnemyGrowthTimer : IComponentData
    {
        public float Value;
        public bool IsOver;
    }
}