using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySpawnSettings : IComponentData
    {
        public int GridSize;
        public float Spread;
        public float HeadroomInCenter;
        public Entity Prefab;
    }
}