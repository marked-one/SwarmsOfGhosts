using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.MetaGame.Levels;
using SwarmsOfGhosts.UI.InGame.GameplayCursor;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.NextLevel
{
    public interface INextLevelPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public UniTask StartNextLevel();
        public void OpenMainMenuScene();
        public void SetCursorVisible(bool isVisible);
    }

    public interface INextLevelPopup
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public class NextLevelPopupPopupViewModel : INextLevelPopupViewModel, INextLevelPopup, IDisposable
    {
        private readonly ILevelSwitcher _levelSwitcher;
        private readonly ICursor _cursor;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private NextLevelPopupPopupViewModel(ILevelSwitcher levelSwitcher, ICursor cursor)
        {
            _levelSwitcher = levelSwitcher;
            _cursor = cursor;

            LevelScore = _levelSwitcher.LevelScore;

            _levelSwitcher.LevelState
                .Subscribe(value => _isVisible.Value = value == LevelState.LevelCompleted)
                .AddTo(_subscriptions);

            IsVisible
                .Subscribe(cursor.SetVisibility)
                .AddTo(_subscriptions);
        }

        public async UniTask StartNextLevel() => await _levelSwitcher.StartNextLevel();

        public void OpenMainMenuScene()
        {
            // TODO:
        }

        public void SetCursorVisible(bool isVisible) => _cursor.SetVisibility(isVisible);

        public void Dispose() => _subscriptions.Dispose();
    }
}