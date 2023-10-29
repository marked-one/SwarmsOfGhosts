using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.SettingsMenu
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
                .OnInstantiated<SettingsMenuView>((context, view) => view.transform.SetSiblingIndex(200))
                .NonLazy();
        }
    }
}