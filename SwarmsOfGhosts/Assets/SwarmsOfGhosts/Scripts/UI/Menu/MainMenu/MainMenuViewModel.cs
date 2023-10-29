using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App;
using SwarmsOfGhosts.MetaGame.Saves;
using SwarmsOfGhosts.UI.Menu.SettingsMenu;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.MainMenu
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

        public IReadOnlyReactiveProperty<int> BestScore { get; }

        [Inject]
        private MainMenuViewModel(IApplication application, ISave save, ISettingsMenu settingsMenu)
        {
            _application = application;
            BestScore = save.Score;
            _settingsMenu = settingsMenu;
        }

        public void OpenGameplayScene()
        {
            // TODO:
        }

        public async UniTask OpenSettingsMenu() => await _settingsMenu.Open();
        public void QuitApplication() => _application.Quit();
    }
}