using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.MetaGame.Saves
{
    [CreateAssetMenu(fileName = "SaveInstaller", menuName = "Installers/SaveInstaller")]
    public class SaveInstaller : ScriptableObjectInstaller<SaveInstaller>
    {
        public override void InstallBindings() =>
            Container
                .BindInterfacesTo<Save>()
                .AsSingle()
                .NonLazy();
    }
}