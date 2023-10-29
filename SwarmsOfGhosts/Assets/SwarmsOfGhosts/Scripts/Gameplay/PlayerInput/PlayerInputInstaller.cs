using SwarmsOfGhosts.App;
using SwarmsOfGhosts.UI.Input;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.Gameplay.PlayerInput
{
    [CreateAssetMenu(fileName = "PlayerInputInstaller", menuName = "Installers/PlayerInputInstaller")]
    public class PlayerInputInstaller : ScriptableObjectInstaller<PlayerInputInstaller>
    {
        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var playerInputFacade = world.GetOrCreateSystem<PlayerInputFacadeSystem>();

            Container
                .Bind<PlayerInputFacadeSystem>()
                .FromInstance(playerInputFacade)
                .AsSingle()
                .OnInstantiated<PlayerInputFacadeSystem>((context, facade) =>
                {
                    var playerInput = context.Container.Resolve<IPlayerInput>();
                    facade.Construct(playerInput);
                })
                .NonLazy();
        }
    }
}