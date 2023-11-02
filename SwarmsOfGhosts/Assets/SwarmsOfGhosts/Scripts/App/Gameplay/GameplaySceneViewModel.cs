using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App.Loading;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    public class GameplaySceneViewModel : IInitializable, IDisposable
    {
        private readonly ILoadingScreen _loadingScreen;
        private readonly ISubSceneLoader _subSceneLoader;

        [Inject]
        private GameplaySceneViewModel(ILoadingScreen loadingScreen, ISubSceneLoader subSceneLoader)
        {
            _loadingScreen = loadingScreen;
            _subSceneLoader = subSceneLoader;
        }

        public void Initialize() =>
            _subSceneLoader
                .LoadSubScene()
                .ContinueWith(_loadingScreen.Hide)
                .Forget(e => { Debug.LogException(e); });

        public void Dispose() => _loadingScreen.Show();
    }
}