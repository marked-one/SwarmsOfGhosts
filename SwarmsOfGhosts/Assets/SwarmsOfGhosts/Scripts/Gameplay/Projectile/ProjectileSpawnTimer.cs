using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Projectile
{
    [GenerateAuthoringComponent]
    public struct ProjectileSpawnTimer : IComponentData
    {
        public float Value;
    }
}