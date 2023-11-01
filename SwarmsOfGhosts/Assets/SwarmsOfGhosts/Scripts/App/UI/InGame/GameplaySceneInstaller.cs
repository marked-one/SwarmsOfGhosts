using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame
{
    [CreateAssetMenu(fileName = "GameplaySceneInstaller", menuName = "Installers/GameplaySceneInstaller")]
    public class GameplaySceneInstaller : ScriptableObjectInstaller<GameplaySceneInstaller>
    {
        [SerializeField] private Canvas _canvasPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<GameplaySceneViewModel>()
                .AsSingle()
                .NonLazy();
        }
    }
}