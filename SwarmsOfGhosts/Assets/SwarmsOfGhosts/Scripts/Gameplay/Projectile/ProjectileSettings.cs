using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSettings : IComponentData
    {
        public float Speed;
        public float DestroyDistance;
        public float Damage;
    }
}