using System;
using SwarmsOfGhosts.App.Loading;
using Zenject;

namespace SwarmsOfGhosts.App.MainMenu
{
    public class MainMenuSceneViewModel : IInitializable, IDisposable
    {
        private readonly ILoadingScreen _loadingScreen;

        [Inject]
        private MainMenuSceneViewModel(ILoadingScreen loadingScreen) => _loadingScreen = loadingScreen;

        public void Initialize() => _loadingScreen.Hide();
        public void Dispose() => _loadingScreen.Show();
    }
}