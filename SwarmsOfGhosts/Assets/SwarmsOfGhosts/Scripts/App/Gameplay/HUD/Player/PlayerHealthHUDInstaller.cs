using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.HUD.Player
{
    [CreateAssetMenu(fileName = "PlayerHealthHUDInstaller", menuName = "Installers/PlayerHealthHUDInstaller")]
    public class PlayerHealthHUDInstaller : ScriptableObjectInstaller<PlayerHealthHUDInstaller>
    {
        [SerializeField] private PlayerHealthView _playerHealthViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<PlayerHealthViewModel>()
                .AsSingle();

            Container
                .Bind<PlayerHealthView>()
                .FromComponentInNewPrefab(_playerHealthViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<PlayerHealthView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 0);
                })
                .NonLazy();
        }
    }
}