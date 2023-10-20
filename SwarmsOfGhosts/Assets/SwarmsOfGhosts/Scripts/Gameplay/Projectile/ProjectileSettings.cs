using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSettings : IComponentData
    {
        public Entity Prefab;
        public float SpawnInterval;
        public float Speed;
        public float DestroyDistance;
    }
}