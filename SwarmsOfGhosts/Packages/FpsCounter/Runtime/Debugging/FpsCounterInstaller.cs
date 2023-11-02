using UnityEngine;
using Zenject;

namespace Debugging
{
    [CreateAssetMenu(fileName = "FpsCounterInstaller", menuName = "Installers/VladimirKlubkov/FpsCounterInstaller")]
    public class FpsCounterInstaller : ScriptableObjectInstaller<FpsCounterInstaller>
    {
        [SerializeField] private float _measurePeriodSeconds = 0.15f;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<FpsCounter>()
                .AsSingle()
                .WithArguments(_measurePeriodSeconds);

            Container
                .BindInterfacesTo<UnityTimeAdapter>()
                .AsSingle();
        }
    }
}