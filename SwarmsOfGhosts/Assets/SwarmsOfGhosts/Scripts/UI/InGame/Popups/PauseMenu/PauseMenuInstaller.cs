using SwarmsOfGhosts.Gameplay.Pause;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.PauseMenu
{
    [CreateAssetMenu(fileName = "PauseMenuInstaller", menuName = "Installers/PauseMenuInstaller")]
    public class PauseMenuInstaller : ScriptableObjectInstaller<PauseMenuInstaller>
    {
        [SerializeField] private PauseMenuView _pauseMenuViewPrefab;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var pauseFacade = world.GetOrCreateSystem<PauseFacadeSystem>();

            Container
                .Bind<IPausable>()
                .FromInstance(pauseFacade)
                .AsSingle();

            Container
                .BindInterfacesTo<PauseMenuViewModel>()
                .AsSingle();

            Container
                .Bind<PauseMenuView>()
                .FromComponentInNewPrefab(_pauseMenuViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<PauseMenuView>((context, view) => view.transform.SetSiblingIndex(100))
                .NonLazy();
        }
    }
}