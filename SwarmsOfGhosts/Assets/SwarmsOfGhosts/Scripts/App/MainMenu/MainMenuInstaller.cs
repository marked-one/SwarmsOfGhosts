using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuInstaller", menuName = "Installers/MainMenuInstaller")]
    public class MainMenuInstaller : ScriptableObjectInstaller<MainMenuInstaller>
    {
        [SerializeField] private MainMenuView _mainMenuViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<MainMenuViewModel>()
                .AsSingle();

            Container
                .Bind<MainMenuView>()
                .FromComponentInNewPrefab(_mainMenuViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<MainMenuView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 0);
                })
                .NonLazy();
        }
    }
}