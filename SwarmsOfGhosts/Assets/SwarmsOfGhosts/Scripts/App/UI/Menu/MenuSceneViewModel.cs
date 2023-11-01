using System;
using SwarmsOfGhosts.App.UI.LoadingScreen;
using Zenject;

namespace SwarmsOfGhosts.App.UI.Menu
{
    public class MenuSceneViewModel : IInitializable, IDisposable
    {
        private readonly ILoadingScreen _loadingScreen;

        [Inject]
        private MenuSceneViewModel(ILoadingScreen loadingScreen) => _loadingScreen = loadingScreen;

        public void Initialize() => _loadingScreen.Hide();
        public void Dispose() => _loadingScreen.Show();
    }
}