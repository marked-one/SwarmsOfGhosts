using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace SwarmsOfGhosts.Player
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class CameraMovementSystem : SystemBase
    {
        private Transform _cameraRoot;
        private Entity _playerEntity;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerMovement>();
        }

        protected override void OnStartRunning()
        {
            _cameraRoot = Camera.main.transform.parent;
            _playerEntity = GetSingletonEntity<PlayerMovement>();
        }

        protected override void OnUpdate()
        {
            var translation = GetComponent<Translation>(_playerEntity);
            var cameraPosition = _cameraRoot.position;
            cameraPosition = new Vector3(translation.Value.x, cameraPosition.y, translation.Value.z);
            _cameraRoot.position = cameraPosition;
        }

        protected override void OnStopRunning()
        {
            _playerEntity = Entity.Null;
        }
    }
}