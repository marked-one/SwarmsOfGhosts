using SwarmsOfGhosts.Gameplay.Enemy;
using SwarmsOfGhosts.Gameplay.Restart;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.MetaGame.Levels
{
    [CreateAssetMenu(fileName = "LevelsInstaller", menuName = "Installers/LevelsInstaller")]
    public class LevelsInstaller : ScriptableObjectInstaller<LevelsInstaller>
    {
        [SerializeField] private LevelsConfig _levelsConfig;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var restartFacade = world.GetOrCreateSystem<RestartFacadeSystem>();

            var enemySpawn = world.GetOrCreateSystem<EnemySpawnSystem>();

            Container
                .Bind<IRestartable>()
                .FromInstance(restartFacade)
                .AsSingle();

            Container
                .Bind<ILevelsConfig>()
                .FromInstance(_levelsConfig)
                .AsSingle();

            Container
                .Bind<IEnemySpawn>()
                .FromInstance(enemySpawn)
                .AsSingle();

            Container
                .BindInterfacesTo<LevelSwitcher>()
                .AsSingle();
        }
    }
}