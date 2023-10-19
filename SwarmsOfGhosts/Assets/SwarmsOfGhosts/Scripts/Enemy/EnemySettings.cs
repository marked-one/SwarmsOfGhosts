using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySettings : IComponentData
    {
        public float2 SpeedRange;
    }
}