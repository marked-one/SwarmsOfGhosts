using SwarmsOfGhosts.App;
using SwarmsOfGhosts.MetaGame.Saves;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.MainMenu
{
    public interface IMainMenuViewModel
    {
        public IReadOnlyReactiveProperty<int> BestScore { get; }
        public void OpenGameplayScene();
        public void OpenSettingsMenu();
        public void QuitApplication();
    }

    public class MainMenuViewModel : IMainMenuViewModel
    {
        private readonly IApplication _application;

        public IReadOnlyReactiveProperty<int> BestScore { get; }

        [Inject]
        private MainMenuViewModel(IApplication application, ISave save)
        {
            _application = application;
            BestScore = save.Score;
        }

        public void OpenGameplayScene()
        {
            // TODO:
        }

        public void OpenSettingsMenu()
        {
            // TODO:
        }

        public void QuitApplication() => _application.Quit();
    }
}