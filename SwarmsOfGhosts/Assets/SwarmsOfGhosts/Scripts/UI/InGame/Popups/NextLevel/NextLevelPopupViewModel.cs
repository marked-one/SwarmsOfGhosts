using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.MetaGame.Levels;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.NextLevel
{
    public interface INextLevelPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsLevelCompleted { get; }
        public UniTask StartNextLevel();
        public void OpenMainMenuScene();
    }

    public class NextLevelPopupViewModel : INextLevelPopupViewModel, IDisposable
    {
        private readonly ILevelSwitcher _levelSwitcher;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isLevelCompleted = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsLevelCompleted => _isLevelCompleted;

        [Inject]
        private NextLevelPopupViewModel(ILevelSwitcher levelSwitcher)
        {
            _levelSwitcher = levelSwitcher;

            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isLevelCompleted.Value = value == LevelState.LevelCompleted)
                .AddTo(_subscriptions);
        }

        public async UniTask StartNextLevel() => await _levelSwitcher.StartNextLevel();

        public void OpenMainMenuScene()
        {
            // TODO:
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}