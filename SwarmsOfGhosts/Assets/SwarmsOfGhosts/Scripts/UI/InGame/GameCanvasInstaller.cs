using SwarmsOfGhosts.UI.Menu.MainMenu;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame
{
    [CreateAssetMenu(fileName = "GameCanvasInstaller", menuName = "Installers/GameCanvasInstaller")]
    public class GameCanvasInstaller : ScriptableObjectInstaller<GameCanvasInstaller>
    {
        [SerializeField] private Canvas _canvasPrefab;

        public override void InstallBindings() =>
            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .NonLazy();
    }
}