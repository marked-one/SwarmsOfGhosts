using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySpawnSettings : IComponentData
    {
        public float Spread;
        public float HeadroomInCenter;
        public Entity Prefab;
    }
}