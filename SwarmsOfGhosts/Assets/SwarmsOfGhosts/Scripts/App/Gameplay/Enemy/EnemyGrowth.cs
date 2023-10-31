using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    public struct EnemyGrowth : IComponentData
    {
        public int Step;
        public float Limit;
    }
}