using SwarmsOfGhosts.App;
using SwarmsOfGhosts.Gameplay.Pause;
using SwarmsOfGhosts.Gameplay.Player;
using SwarmsOfGhosts.Gameplay.Restart;
using SwarmsOfGhosts.UI.Input;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.PlayerInput
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerInputFacadeSystem : SystemBase
    {
        private PauseFacadeSystem _pauseSystem;
        private Entity _player;

        private IPlayerInput _playerInput;

        public void Construct(IPlayerInput playerInput) => _playerInput = playerInput;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseFacadeSystem>();

            RequireSingletonForUpdate<IsPlayingTag>();
            RequireSingletonForUpdate<PlayerTag>();
        }

        [BurstCompile]
        protected override void OnStartRunning() => _player = GetSingletonEntity<PlayerTag>();

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            SetComponent(_player, new PlayerMovement { Value = _playerInput.Movement });
        }

        [BurstCompile]
        protected override void OnStopRunning() => _player = Entity.Null;
    }
}