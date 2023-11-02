using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Fps
{
    [CreateAssetMenu(fileName = "FpsCounterInstaller", menuName = "Installers/FpsCounterInstaller")]
    public class FpsCounterInstaller : ScriptableObjectInstaller<FpsCounterInstaller>
    {
        [SerializeField] private FpsCounterView _fpsCounterViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<FpsCounterViewModel>()
                .AsSingle();

            Container
                .Bind<FpsCounterView>()
                .FromComponentInNewPrefab(_fpsCounterViewPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}