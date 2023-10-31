using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PlayerSpawnSystem))]
    public partial class PlayerInputSystem : SystemBase
    {
        private PauseSystem _pauseSystem;
        private Entity _player;

        public Vector2 Movement { private get; set; }

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

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

            SetComponent(_player, new PlayerMovement { Value = Movement });
        }

        [BurstCompile]
        protected override void OnStopRunning() => _player = Entity.Null;
    }
}