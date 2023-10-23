using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    public struct EnemyGrowth : IComponentData
    {
        public int Step;
        public float Limit;
    }
}