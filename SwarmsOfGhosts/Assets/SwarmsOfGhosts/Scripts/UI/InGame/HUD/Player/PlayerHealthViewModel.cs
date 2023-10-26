using SwarmsOfGhosts.Gameplay.Player;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.HUD.Player
{
    public interface IPlayerHealthViewModel
    {
        public IReadOnlyReactiveProperty<float> PlayerHealth { get; }
    }

    public class PlayerHealthViewModel : IPlayerHealthViewModel
    {
        private readonly ReactiveProperty<float> _playerHealth = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<float> PlayerHealth => _playerHealth;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        [Inject]
        private PlayerHealthViewModel(IPlayerHealth playerHealth)
        {
            var maxPlayerHealth = 0f;
            playerHealth.Max
                .Subscribe(value => maxPlayerHealth = value)
                .AddTo(_subscriptions);

            playerHealth.Current
                .Subscribe(value =>
                {
                    _playerHealth.Value = Mathf.Approximately(maxPlayerHealth, 0f) ? 1f : value / maxPlayerHealth;
                })
                .AddTo(_subscriptions);
        }
    }
}