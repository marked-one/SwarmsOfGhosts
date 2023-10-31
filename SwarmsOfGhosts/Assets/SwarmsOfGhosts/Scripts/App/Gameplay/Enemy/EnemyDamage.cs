using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    public struct EnemyDamage : IComponentData
    {
        public float Value;
        public float Cooldown;
    }
}