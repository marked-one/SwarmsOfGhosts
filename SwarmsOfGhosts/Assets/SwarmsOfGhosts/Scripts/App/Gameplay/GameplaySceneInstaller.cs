using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    [CreateAssetMenu(fileName = "GameplaySceneInstaller", menuName = "Installers/GameplaySceneInstaller")]
    public class GameplaySceneInstaller : ScriptableObjectInstaller<GameplaySceneInstaller>
    {
        [SerializeField] private Canvas _canvasPrefab;
        [SerializeField] private GameObject _cameraPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .Bind<IHierarchyOrganizer>()
                .To<HierarchyOrganizer>()
                .AsSingle();

            Container
                .BindInterfacesTo<GameplaySceneViewModel>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<GameplayFacade>()
                .AsSingle();

            Container
                .Bind<Camera>()
                .FromComponentInNewPrefab(_cameraPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<GameplayCamera>()
                .AsSingle();
        }
    }
}