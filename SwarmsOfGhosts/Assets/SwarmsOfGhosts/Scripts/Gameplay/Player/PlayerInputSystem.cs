using InputSystem;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerInputSystem : SystemBase
    {
        private InputActions _inputActions;
        private Entity _player;

        [BurstCompile]
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerTag>();
            _inputActions = new InputActions();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            _player = GetSingletonEntity<PlayerTag>();

            var entityManager = World.EntityManager;
            entityManager.AddComponent<PlayerMovement>(_player);

            _inputActions.Enable();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var input = _inputActions.Keyboard.WASD.ReadValue<Vector2>();
            SetComponent(_player, new PlayerMovement { Value = input });
        }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            _player = Entity.Null;
            _inputActions.Disable();
        }
    }
}