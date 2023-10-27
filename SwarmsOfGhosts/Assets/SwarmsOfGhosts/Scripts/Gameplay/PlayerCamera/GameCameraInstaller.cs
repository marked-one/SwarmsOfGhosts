using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.Gameplay.PlayerCamera
{
    [CreateAssetMenu(fileName = "GameCameraInstaller", menuName = "Installers/GameCameraInstaller")]
    public class GameCameraInstaller : ScriptableObjectInstaller<GameCameraInstaller>
    {
        [SerializeField] private GameObject _cameraPrefab;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var cameraMovementSystem = world.GetOrCreateSystem<CameraMovementSystem>();

            Container
                .Bind<Camera>()
                .FromComponentInNewPrefab(_cameraPrefab)
                .AsSingle()
                .OnInstantiated<Camera>((context, camera) => cameraMovementSystem.Construct(camera))
                .NonLazy();
        }
    }
}