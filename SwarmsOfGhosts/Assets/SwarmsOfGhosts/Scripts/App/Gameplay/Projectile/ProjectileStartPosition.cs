using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.App.Gameplay.Projectile
{
    public struct ProjectileStartPosition : IComponentData
    {
        public float3 Value;
    }
}