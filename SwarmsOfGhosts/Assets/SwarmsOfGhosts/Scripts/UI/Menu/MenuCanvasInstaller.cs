using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu
{
    [CreateAssetMenu(fileName = "MenuCanvasInstaller", menuName = "Installers/MenuCanvasInstaller")]
    public class MenuCanvasInstaller : ScriptableObjectInstaller<MenuCanvasInstaller>
    {
        [SerializeField] private Canvas _canvasPrefab;

        public override void InstallBindings() =>
            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .OnInstantiated<Canvas>((context, canvas) => canvas.worldCamera = context.Container.Resolve<Camera>())
                .NonLazy();
    }
}