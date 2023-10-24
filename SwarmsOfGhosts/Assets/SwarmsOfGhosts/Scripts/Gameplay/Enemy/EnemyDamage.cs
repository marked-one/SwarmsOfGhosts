using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    public struct EnemyDamage : IComponentData
    {
        public float Value;
        public float Cooldown;
    }
}