using SwarmsOfGhosts.Gameplay.Enemy;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Facades
{
    public interface IInfestation
    {
        public IReadOnlyReactiveProperty<float> Current { get; }
        public IReadOnlyReactiveProperty<float> Max { get; }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EnemySpawnSystem))]
    public partial class EnemyHealthFacade : SystemBase, IInfestation
    {
        private readonly ReactiveProperty<float> _current = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _max = new ReactiveProperty<float>();

        public IReadOnlyReactiveProperty<float> Current => _current;
        public IReadOnlyReactiveProperty<float> Max => _max;

        protected override void OnStartRunning()
        {
            var max = 0f;
            var current = 0f;

            Entities
                .WithoutBurst()
                .ForEach((in EnemyHealth playerHealth) =>
                {
                    max += playerHealth.Max;
                    current += playerHealth.Value;
                })
                .Run();

            _max.Value = max;
            _current.Value = current;
        }

        protected override void OnUpdate()
        {
            var current = 0f;

            Entities
                .WithoutBurst()
                .ForEach((in EnemyHealth playerHealth) =>
                {
                    current += playerHealth.Value;
                })
                .Run();

            _current.Value = current;
        }
    }
}