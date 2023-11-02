using System;
using SwarmsOfGhosts.App.Input;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Input
{
    public class GameplayInputViewModel : IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        [Inject]
        private GameplayInputViewModel(IGameplayInput gameplayInput, IMovable movable) =>
            gameplayInput.Movement
                .Subscribe(value => movable.Movement = value)
                .AddTo(_subscriptions);

        public void Dispose() => _subscriptions.Dispose();
    }
}