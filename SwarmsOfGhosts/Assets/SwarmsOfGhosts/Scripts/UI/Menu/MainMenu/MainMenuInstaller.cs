using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.MainMenu
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
                .NonLazy();
        }
    }
}