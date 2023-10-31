using System;
using Cysharp.Threading.Tasks;
using SceneManagement;
using SwarmsOfGhosts.App.MetaGame.Levels;
using SwarmsOfGhosts.App.UI.InGame.GameplayCursor;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.Popups.GameOver
{
    public interface IGameOverPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public void OpenMainMenuScene();
        public void SetCursorVisible(bool isVisible);
    }

    public interface IGameOverPopup
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public class GameOverPopupViewModel : IGameOverPopupViewModel, IGameOverPopup, IDisposable
    {
        private readonly ICursor _cursor;
        private readonly ISceneLoader<SceneName> _sceneLoader;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<int> LevelScore { get; }

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private GameOverPopupViewModel(
            ILevelSwitcher levelSwitcher, ICursor cursor, ISceneLoader<SceneName> sceneLoader)
        {
            _cursor = cursor;
            _sceneLoader = sceneLoader;

            LevelScore = levelSwitcher.LevelScore;

            levelSwitcher.LevelState
                .Subscribe(value => _isVisible.Value = value == LevelState.GameOver)
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