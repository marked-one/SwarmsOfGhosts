﻿using Unity.Entities;
using Unity.Mathematics;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [GenerateAuthoringComponent]
    public struct EnemySettings : IComponentData
    {
        public float2 SpeedRange;
        public float2 TransparencyRange;
        public float Scale;
        public int GrowthStep;
        public float GrowthLimit;
    }
}