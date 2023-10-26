using InputSystem;
using SwarmsOfGhosts.Gameplay.Enemy;
using SwarmsOfGhosts.Gameplay.Player;
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

            var enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            RequireForUpdate(enemyQuery);

            RequireSingletonForUpdate<IsPlayingTag>();
            RequireSingletonForUpdate<PlayerTag>();
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