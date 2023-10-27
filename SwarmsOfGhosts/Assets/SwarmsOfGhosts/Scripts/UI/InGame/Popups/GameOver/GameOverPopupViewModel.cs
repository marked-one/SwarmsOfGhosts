using System;
using SwarmsOfGhosts.MetaGame.Levels;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameOver
{
    public interface IGameOverPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsGameOver { get; }
        public void OpenMainMenuScene();
    }

    public class GameOverPopupViewModel : IGameOverPopupViewModel, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isGameOver = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsGameOver => _isGameOver;

        [Inject]
        private GameOverPopupViewModel(ILevelSwitcher levelSwitcher)
        {
            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isGameOver.Value = value == LevelState.GameOver)
                .AddTo(_subscriptions);
        }

        public void OpenMainMenuScene()
        {
            // TODO:
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}