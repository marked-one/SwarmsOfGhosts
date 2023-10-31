using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSpawnSettings : IComponentData
    {
        public Entity Prefab;
        public float Cooldown;
    }
}