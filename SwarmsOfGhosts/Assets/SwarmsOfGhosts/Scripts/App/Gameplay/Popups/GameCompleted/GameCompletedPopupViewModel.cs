using System;
using Cysharp.Threading.Tasks;
using SceneManagement;
using SwarmsOfGhosts.App.Gameplay.Levels;
using SwarmsOfGhosts.App.Gameplay.MouseCursor;
using SwarmsOfGhosts.App.Loading;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.GameCompleted
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
        private readonly ISceneLoader<SceneName> _sceneLoader;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private GameCompletedPopupViewModel(
            ILevelSwitcher levelSwitcher, ICursor cursor, ISceneLoader<SceneName> sceneLoader)
        {
            _cursor = cursor;
            _sceneLoader = sceneLoader;

            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isVisible.Value = value == LevelState.GameCompleted)
                .AddTo(_subscriptions);

            IsVisible
                .Subscribe(cursor.SetVisibility)
                .AddTo(_subscriptions);
        }

        public void OpenMainMenuScene() => _sceneLoader.Load(SceneName.MainMenu).Forget(e => Debug.LogException(e));
        public void SetCursorVisible(bool isVisible) => _cursor.SetVisibility(isVisible);
        public void Dispose() => _subscriptions.Dispose();
    }
}