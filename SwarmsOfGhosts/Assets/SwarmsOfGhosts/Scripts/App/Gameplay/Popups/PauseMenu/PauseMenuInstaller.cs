using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.PauseMenu
{
    [CreateAssetMenu(fileName = "PauseMenuInstaller", menuName = "Installers/PauseMenuInstaller")]
    public class PauseMenuInstaller : ScriptableObjectInstaller<PauseMenuInstaller>
    {
        [SerializeField] private PauseMenuView _pauseMenuViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<PauseMenuViewModel>()
                .AsSingle();

            Container
                .Bind<PauseMenuView>()
                .FromComponentInNewPrefab(_pauseMenuViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<PauseMenuView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 100);
                })
                .NonLazy();
        }
    }
}