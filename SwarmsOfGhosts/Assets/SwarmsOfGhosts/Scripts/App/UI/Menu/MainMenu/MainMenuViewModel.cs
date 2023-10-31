using Cysharp.Threading.Tasks;
using SceneManagement;
using SwarmsOfGhosts.App.MetaGame.Saves;
using SwarmsOfGhosts.App.UI.Menu.SettingsMenu;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.Menu.MainMenu
{
    public interface IMainMenuViewModel
    {
        public IReadOnlyReactiveProperty<int> BestScore { get; }
        public void OpenGameplayScene();
        public UniTask OpenSettingsMenu();
        public void QuitApplication();
    }

    public class MainMenuViewModel : IMainMenuViewModel
    {
        private readonly IApplication _application;
        private readonly ISettingsMenu _settingsMenu;
        private readonly ISceneLoader<SceneName> _sceneLoader;

        public IReadOnlyReactiveProperty<int> BestScore { get; }

        [Inject]
        private MainMenuViewModel(
            IApplication application, ISave save, ISettingsMenu settingsMenu, ISceneLoader<SceneName> sceneLoader)
        {
            _application = application;
            BestScore = save.Score;
            _settingsMenu = settingsMenu;
            _sceneLoader = sceneLoader;
        }

        public void OpenGameplayScene() =>
            _sceneLoader
                .Load(SceneName.Gameplay)
                .Forget(e => { Debug.LogException(e); });

        public async UniTask OpenSettingsMenu() => await _settingsMenu.Open();
        public void QuitApplication() => _application.Quit();
    }
}