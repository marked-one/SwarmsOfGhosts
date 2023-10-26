using SwarmsOfGhosts.Gameplay.Player;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.HUD.Player
{
    [CreateAssetMenu(fileName = "PlayerHealthHUDInstaller", menuName = "Installers/PlayerHealthHUDInstaller")]
    public class PlayerHealthHUDInstaller : ScriptableObjectInstaller<PlayerHealthHUDInstaller>
    {
        [SerializeField] private PlayerHealthView _playerHealthViewPrefab;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var playerHealthFacade = world.GetOrCreateSystem<PlayerHealthFacadeSystem>();

            Container
                .Bind<IPlayerHealth>()
                .FromInstance(playerHealthFacade)
                .AsSingle();

            Container
                .BindInterfacesTo<PlayerHealthViewModel>()
                .AsSingle();

            Container
                .Bind<PlayerHealthView>()
                .FromComponentInNewPrefab(_playerHealthViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .NonLazy();
        }
    }
}