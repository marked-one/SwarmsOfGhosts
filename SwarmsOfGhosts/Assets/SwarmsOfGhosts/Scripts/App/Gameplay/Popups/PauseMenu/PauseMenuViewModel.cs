using System;
using Cysharp.Threading.Tasks;
using SceneManagement;
using SwarmsOfGhosts.App.Gameplay.MouseCursor;
using SwarmsOfGhosts.App.Gameplay.Popups.GameCompleted;
using SwarmsOfGhosts.App.Gameplay.Popups.GameOver;
using SwarmsOfGhosts.App.Gameplay.Popups.NextLevel;
using SwarmsOfGhosts.App.Input;
using SwarmsOfGhosts.App.Loading;
using SwarmsOfGhosts.App.SettingsMenu;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.PauseMenu
{
    public interface IPauseMenuViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public void Close();
        public UniTask OpenSettingsMenu();
        public void OpenMainMenuScene();
        public void SetCursorVisible(bool isVisible);
    }

    public class PauseMenuViewModel : IPauseMenuViewModel, IDisposable
    {
        private readonly IMenuInput _menuInput;
        private readonly IPausable _pausable;
        private readonly ISettingsMenu _settingsMenu;
        private readonly ICursor _cursor;
        private readonly INextLevelPopup _nextLevelPopup;
        private readonly IGameOverPopup _gameOverPopup;
        private readonly IGameCompletedPopup _gameCompletedPopup;
        private readonly ISceneLoader<SceneName> _sceneLoader;

        private IDisposable _menuInputSubscription;

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private PauseMenuViewModel(IMenuInput menuInput, IPausable pausable, ISettingsMenu settingsMenu, ICursor cursor,
            INextLevelPopup nextLevelPopup, IGameOverPopup gameOverPopup, IGameCompletedPopup gameCompletedPopup,
            ISceneLoader<SceneName> sceneLoader)
        {
            _menuInput = menuInput;
            _pausable = pausable;
            _settingsMenu = settingsMenu;
            _cursor = cursor;
            _nextLevelPopup = nextLevelPopup;
            _gameOverPopup = gameOverPopup;
            _gameCompletedPopup = gameCompletedPopup;
            _sceneLoader = sceneLoader;
            _menuInputSubscription = SubscribeToMenuInput();
        }

        private IDisposable SubscribeToMenuInput() =>
            _menuInput.Back
                .Subscribe(_ =>
                {
                    if (_nextLevelPopup.IsVisible.Value)
                        return;

                    if (_gameOverPopup.IsVisible.Value)
                        return;

                    if (_gameCompletedPopup.IsVisible.Value)
                        return;

                    SetVisibility(!_isVisible.Value);
                });

        public void Close() => SetVisibility(false);

        private void SetVisibility(bool isVisible)
        {
            _isVisible.Value = isVisible;
            _pausable.IsPaused = isVisible;
        }

        public async UniTask OpenSettingsMenu()
        {
            _menuInputSubscription.Dispose();
            await _settingsMenu.Open();
            _menuInputSubscription = SubscribeToMenuInput();
        }

        public void OpenMainMenuScene() => _sceneLoader.Load(SceneName.MainMenu).Forget(e => Debug.LogException(e));
        public void SetCursorVisible(bool isVisible) => _cursor.SetVisibility(isVisible);
        public void Dispose() => _menuInputSubscription?.Dispose();
    }
}