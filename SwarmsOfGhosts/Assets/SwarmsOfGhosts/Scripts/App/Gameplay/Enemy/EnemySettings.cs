using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySettings : IComponentData
    {
        public float2 SpeedRange;
        public float2 TransparencyRange;
        public float Scale;
        public int GrowthStep;
        public float GrowthLimit;
        public float Health;
        public float Damage;
        public float DamageCooldown;
    }
}