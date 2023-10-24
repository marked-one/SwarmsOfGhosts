using SwarmsOfGhosts.Gameplay.Player;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Utilities
{
    public interface IPlayerHealth
    {
        public IReadOnlyReactiveProperty<float> PlayerHealth { get; }
        public IReadOnlyReactiveProperty<float> MaxPlayerHealth { get; }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerHealthFacade : SystemBase, IPlayerHealth
    {
        private readonly ReactiveProperty<float> _playerHealth = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _maxPlayerHealth = new ReactiveProperty<float>();

        public IReadOnlyReactiveProperty<float> PlayerHealth => _playerHealth;
        public IReadOnlyReactiveProperty<float> MaxPlayerHealth => _maxPlayerHealth;

        [BurstCompile]
        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .ForEach((in PlayerHealth playerHealth) =>
                {
                    _maxPlayerHealth.Value = playerHealth.Max;
                    _playerHealth.Value = playerHealth.Value;
                })
                .Run();
        }
    }
}