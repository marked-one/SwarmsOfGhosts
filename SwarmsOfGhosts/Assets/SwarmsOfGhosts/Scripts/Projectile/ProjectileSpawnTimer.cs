using Unity.Entities;

namespace SwarmsOfGhosts.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSpawnTimer : IComponentData
    {
        public float Value;
    }
}