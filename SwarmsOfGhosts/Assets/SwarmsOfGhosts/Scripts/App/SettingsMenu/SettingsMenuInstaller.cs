using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.SettingsMenu
{
    [CreateAssetMenu(fileName = "SettingsMenuInstaller", menuName = "Installers/SettingsMenuInstaller")]
    public class SettingsMenuInstaller : ScriptableObjectInstaller<SettingsMenuInstaller>
    {
        [SerializeField] private SettingsMenuView _settingsMenuViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<SettingsMenuViewModel>()
                .AsSingle();

            Container
                .Bind<SettingsMenuView>()
                .FromComponentInNewPrefab(_settingsMenuViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<SettingsMenuView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 200);
                })
                .NonLazy();
        }
    }
}