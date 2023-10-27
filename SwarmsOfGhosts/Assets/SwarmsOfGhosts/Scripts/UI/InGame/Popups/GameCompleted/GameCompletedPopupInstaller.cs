using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameCompleted
{
    [CreateAssetMenu(fileName = "GameCompletedPopupInstaller", menuName = "Installers/GameCompletedPopupInstaller")]
    public class GameCompletedPopupInstaller : ScriptableObjectInstaller<GameCompletedPopupInstaller>
    {
        [SerializeField] private GameCompletedPopupView _gameCompletedPopupViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameCompletedPopupViewModel>()
                .AsSingle();

            Container
                .Bind<GameCompletedPopupView>()
                .FromComponentInNewPrefab(_gameCompletedPopupViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<GameCompletedPopupView>((context, view) => view.transform.SetSiblingIndex(100))
                .NonLazy();
        }
    }
}