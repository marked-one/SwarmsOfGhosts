using SwarmsOfGhosts.Gameplay.Player;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Facades
{
    public interface IPlayerHealth
    {
        public IReadOnlyReactiveProperty<float> Current { get; }
        public IReadOnlyReactiveProperty<float> Max { get; }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerHealthFacade : SystemBase, IPlayerHealth
    {
        private readonly ReactiveProperty<float> _current = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _max = new ReactiveProperty<float>();

        public IReadOnlyReactiveProperty<float> Current => _current;
        public IReadOnlyReactiveProperty<float> Max => _max;

        protected override void OnStartRunning()
        {
            Entities
                .WithoutBurst()
                .ForEach((in PlayerHealth playerHealth) =>
                {
                    _max.Value += playerHealth.Max;
                    _current.Value += playerHealth.Value;
                })
                .Run();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .ForEach((in PlayerHealth playerHealth) =>
                {
                    //_max.Value = playerHealth.Max;
                    _current.Value = playerHealth.Value;
                })
                .Run();
        }
    }
}