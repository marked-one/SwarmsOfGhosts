using InputSystem;
using Unity.Entities;
using UnityEngine;

namespace SwarmsOfGhosts.Player
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class PlayerInputSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerMovement>();
            _inputActions = new InputActions();
        }

        protected override void OnStartRunning()
        {
            _inputActions.Enable();
        }

        protected override void OnUpdate()
        {
            var input = _inputActions.Keyboard.WASD.ReadValue<Vector2>();
            SetSingleton(new PlayerMovement { Value = input });
        }

        protected override void OnStopRunning()
        {
            _inputActions.Disable();
        }
    }
}