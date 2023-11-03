using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelsInstaller", menuName = "Installers/LevelsInstaller")]
    public class LevelsInstaller : ScriptableObjectInstaller<LevelsInstaller>
    {
        [SerializeField] private LevelsConfig _levelsConfig;

        public override void InstallBindings()
        {
            Container
                .Bind<ILevelsConfig>()
                .FromInstance(_levelsConfig)
                .AsSingle();

            Container
                .BindInterfacesTo<LevelSwitcher>()
                .AsSingle();
        }
    }
}