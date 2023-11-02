using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Popups.GameCompleted
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
                .OnInstantiated<GameCompletedPopupView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 100);
                })
                .NonLazy();
        }
    }
}