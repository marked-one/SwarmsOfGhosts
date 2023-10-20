﻿using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySpawnSettings : IComponentData
    {
        public int GridDimensionSize;
        public float Spread;
        public float HeadroomInCenter;
        public Entity Prefab;
    }
}