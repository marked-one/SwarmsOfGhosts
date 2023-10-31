using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.Popups.PauseMenu
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
                .OnInstantiated<PauseMenuView>((context, view) => view.transform.SetSiblingIndex(100))
                .NonLazy();
        }
    }
}