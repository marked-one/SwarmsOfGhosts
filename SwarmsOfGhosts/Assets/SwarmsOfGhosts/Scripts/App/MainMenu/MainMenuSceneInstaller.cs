using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuSceneInstaller", menuName = "Installers/MainMenuSceneInstaller")]
    public class MainMenuSceneInstaller : ScriptableObjectInstaller<MainMenuSceneInstaller>
    {
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private Light _lightPrefab;
        [SerializeField] private Canvas _canvasPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<Camera>()
                .FromComponentInNewPrefab(_cameraPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .Bind<Light>()
                .FromComponentInNewPrefab(_lightPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .OnInstantiated<Canvas>((context, canvas) => canvas.worldCamera = context.Container.Resolve<Camera>())
                .NonLazy();

            Container
                .Bind<IHierarchyOrganizer>()
                .To<HierarchyOrganizer>()
                .AsSingle();

            Container
                .BindInterfacesTo<MainMenuSceneViewModel>()
                .AsSingle()
                .NonLazy();
        }
    }
}