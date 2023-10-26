using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSpawnSettings : IComponentData
    {
        public Entity Prefab;
        public float Cooldown;
    }
}