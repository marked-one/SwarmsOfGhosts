using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.Projectile
{
    public struct ProjectileStartPosition : IComponentData
    {
        public float3 Value;
    }
}