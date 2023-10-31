using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayInstaller", menuName = "Installers/GameplayInstaller")]
    public class GameplayInstaller : ScriptableObjectInstaller<GameplayInstaller>
    {
        [SerializeField] private GameObject _cameraPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameplayFacade>()
                .AsSingle();

            Container
                .Bind<Camera>()
                .FromComponentInNewPrefab(_cameraPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<GameplayCamera>()
                .AsSingle();
        }
    }
}