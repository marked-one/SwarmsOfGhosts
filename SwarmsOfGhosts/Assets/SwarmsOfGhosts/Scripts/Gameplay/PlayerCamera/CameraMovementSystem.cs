using SwarmsOfGhosts.Gameplay.Player;
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
        private Transform _cameraRoot;
        private Entity _player;

        private Camera _camera;

        public void Construct(Camera camera) => _camera = camera;

        [BurstCompile]
        protected override void OnCreate() => RequireSingletonForUpdate<PlayerTag>();

        [BurstCompile]
        protected override void OnStartRunning()
        {
            _cameraRoot = _camera.transform.parent;
            _player = GetSingletonEntity<PlayerTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
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