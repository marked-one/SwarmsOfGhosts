using InputSystem;
using SwarmsOfGhosts.Gameplay.Pause;
using SwarmsOfGhosts.Gameplay.Restart;
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
        private PauseSystem _pauseSystem;

        private InputActions _inputActions;
        private Entity _player;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

            _inputActions = new InputActions();

            RequireSingletonForUpdate<IsPlayingTag>();
            RequireSingletonForUpdate<PlayerTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            _player = GetSingletonEntity<PlayerTag>();
            _inputActions.Enable();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var input = _inputActions.Keyboard.WASD.ReadValue<Vector2>();
            SetComponent(_player, new PlayerMovement { Value = input });
        }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            _inputActions.Disable();
            _player = Entity.Null;
        }
    }
}