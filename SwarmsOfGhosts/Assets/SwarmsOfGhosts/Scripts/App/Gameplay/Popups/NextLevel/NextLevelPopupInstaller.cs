using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.NextLevel
{
    [CreateAssetMenu(fileName = "NextLevelPopupInstaller", menuName = "Installers/NextLevelPopupInstaller")]
    public class NextLevelPopupInstaller : ScriptableObjectInstaller<NextLevelPopupInstaller>
    {
        [SerializeField] private NextLevelPopupView _nextLevelPopupViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<NextLevelPopupPopupViewModel>()
                .AsSingle();

            Container
                .Bind<NextLevelPopupView>()
                .FromComponentInNewPrefab(_nextLevelPopupViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<NextLevelPopupView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 100);
                })
                .NonLazy();
        }
    }
}