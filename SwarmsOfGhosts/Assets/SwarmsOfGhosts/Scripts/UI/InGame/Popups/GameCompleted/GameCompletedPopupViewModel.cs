using System;
using SwarmsOfGhosts.MetaGame.Levels;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameCompleted
{
    public interface IGameCompletedPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsGameCompleted { get; }
        public void OpenMainMenuScene();
    }

    public class GameCompletedPopupViewModel : IGameCompletedPopupViewModel, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isGameCompleted = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsGameCompleted => _isGameCompleted;

        [Inject]
        private GameCompletedPopupViewModel(ILevelSwitcher levelSwitcher)
        {
            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isGameCompleted.Value = value == LevelState.GameCompleted)
                .AddTo(_subscriptions);
        }

        public void OpenMainMenuScene()
        {
            // TODO:
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}