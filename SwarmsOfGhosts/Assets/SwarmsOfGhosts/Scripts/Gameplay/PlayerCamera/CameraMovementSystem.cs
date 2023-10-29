using SwarmsOfGhosts.Gameplay.Pause;
using SwarmsOfGhosts.Gameplay.Player;
using SwarmsOfGhosts.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace SwarmsOfGhosts.Gameplay.PlayerCamera
{
    [BurstCompile]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class CameraMovementSystem : SystemBase
    {
        private PauseFacadeSystem _pauseSystem;
        private Entity _player;

        private Camera _camera;
        private Transform _cameraRoot;

        public void Construct(Camera camera) => _camera = camera;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseFacadeSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
            RequireSingletonForUpdate<PlayerTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            _player = GetSingletonEntity<PlayerTag>();
            _cameraRoot = _camera.transform.parent;
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var translation = GetComponent<Translation>(_player);
            var cameraPosition = _cameraRoot.position;
            cameraPosition = new Vector3(translation.Value.x, cameraPosition.y, translation.Value.z);
            _cameraRoot.position = cameraPosition;
        }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            _cameraRoot = null;
            _player = Entity.Null;
        }
    }
}