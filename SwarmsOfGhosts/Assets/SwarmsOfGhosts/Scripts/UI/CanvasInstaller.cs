using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI
{
    [CreateAssetMenu(fileName = "CanvasInstaller", menuName = "Installers/CanvasInstaller")]
    public class CanvasInstaller : ScriptableObjectInstaller<CanvasInstaller>
    {
        [SerializeField] private Canvas _canvasPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<Canvas>()
                .FromComponentInNewPrefab(_canvasPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}