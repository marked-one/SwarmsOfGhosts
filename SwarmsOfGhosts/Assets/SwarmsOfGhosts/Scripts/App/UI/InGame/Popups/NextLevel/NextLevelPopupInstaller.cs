using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.Popups.NextLevel
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
                .OnInstantiated<NextLevelPopupView>((context, view) => view.transform.SetSiblingIndex(100))
                .NonLazy();
        }
    }
}