using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Loading
{
    [CreateAssetMenu(fileName = "LoadingScreenInstaller", menuName = "Installers/LoadingScreenInstaller")]
    public class LoadingScreenInstaller : ScriptableObjectInstaller<LoadingScreenInstaller>
    {
        [SerializeField] private LoadingScreenView _loadingScreenViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<LoadingScreenViewModel>()
                .AsSingle();

            Container
                .Bind<LoadingScreenView>()
                .FromComponentInNewPrefab(_loadingScreenViewPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}