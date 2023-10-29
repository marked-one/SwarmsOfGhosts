using System;
using SwarmsOfGhosts.MetaGame.Levels;
using SwarmsOfGhosts.UI.InGame.GameplayCursor;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameCompleted
{
    public interface IGameCompletedPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public void OpenMainMenuScene();
        public void SetCursorVisible(bool isVisible);
    }

    public interface IGameCompletedPopup
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public class GameCompletedPopupViewModel : IGameCompletedPopupViewModel, IGameCompletedPopup, IDisposable
    {
        private readonly ICursor _cursor;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private GameCompletedPopupViewModel(ILevelSwitcher levelSwitcher, ICursor cursor)
        {
            _cursor = cursor;

            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isVisible.Value = value == LevelState.GameCompleted)
                .AddTo(_subscriptions);

            IsVisible
                .Subscribe(cursor.SetVisibility)
                .AddTo(_subscriptions);
        }

        public void OpenMainMenuScene()
        {
            // TODO:
        }

        public void SetCursorVisible(bool isVisible) => _cursor.SetVisibility(isVisible);

        public void Dispose() => _subscriptions.Dispose();
    }
}