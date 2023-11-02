using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.GameOver
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
                .OnInstantiated<GameOverPopupView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 100);
                })
                .NonLazy();
        }
    }
}