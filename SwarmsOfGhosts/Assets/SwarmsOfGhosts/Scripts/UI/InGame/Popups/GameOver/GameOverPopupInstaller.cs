using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameOver
{
    [CreateAssetMenu(fileName = "GameOverPopupInstaller", menuName = "Installers/GameOverPopupInstaller")]
    public class GameOverPopupInstaller : ScriptableObjectInstaller<GameOverPopupInstaller>
    {
        [SerializeField] private GameOverPopupView _gameOverPopupViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameOverPopupViewModel>()
                .AsSingle();

            Container
                .Bind<GameOverPopupView>()
                .FromComponentInNewPrefab(_gameOverPopupViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<GameOverPopupView>((context, view) => view.transform.SetSiblingIndex(100))
                .NonLazy();
        }
    }
}