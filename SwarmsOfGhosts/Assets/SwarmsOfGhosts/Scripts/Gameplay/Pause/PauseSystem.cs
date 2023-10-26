using InputSystem;
using SwarmsOfGhosts.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Pause
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class PauseSystem : SystemBase
    {
        private InputActions _inputActions;

        public bool IsPaused { get; set; }

        [BurstCompile]
        protected override void OnCreate()
        {
            _inputActions = new InputActions();
            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning() => _inputActions.Enable();

        [BurstCompile]
        protected override void OnUpdate()
        {
            var isBackPressed = _inputActions.Keyboard.Back.WasPressedThisFrame();
            if (!isBackPressed)
                return;

            IsPaused = !IsPaused;
        }

        [BurstCompile]
        protected override void OnStopRunning() => _inputActions.Disable();
    }
}