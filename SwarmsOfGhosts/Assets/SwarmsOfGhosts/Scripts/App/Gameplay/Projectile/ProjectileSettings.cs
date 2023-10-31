using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSettings : IComponentData
    {
        public float Speed;
        public float DestroyDistance;
        public float Damage;
    }
}