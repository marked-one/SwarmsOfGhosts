using SwarmsOfGhosts.Gameplay.Restart;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.MetaGame.Levels
{
    [CreateAssetMenu(fileName = "LevelInstaller", menuName = "Installers/LevelInstaller")]
    public class LevelsInstaller : ScriptableObjectInstaller<LevelsInstaller>
    {
        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var restartFacade = world.GetOrCreateSystem<RestartFacadeSystem>();

            Container
                .Bind<IRestartable>()
                .FromInstance(restartFacade)
                .AsSingle();

            Container
                .BindInterfacesTo<LevelSwitcher>()
                .AsSingle();
        }
    }
}