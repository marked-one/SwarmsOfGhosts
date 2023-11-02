﻿using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Restart;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    public partial class EnemyHealthSystem : SystemBase
    {
        private PauseSystem _pauseSystem;

        private readonly ReactiveProperty<float> _current = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<float> Current => _current;

        private readonly ReactiveProperty<float> _max = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<float> Max => _max;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();
            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            var max = 0f;
            var current = 0f;

            Entities
                .WithoutBurst()
                .ForEach((in EnemyHealth health) =>
                {
                    max += health.Max;
                    current += health.Value;
                })
                .Run();

            _max.Value = max;
            _current.Value = current;
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var current = 0f;

            Entities
                .WithoutBurst()
                .ForEach((in EnemyHealth health) => current += health.Value)
                .Run();

            _current.Value = current;
        }
    }
}