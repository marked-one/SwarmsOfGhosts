using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Player;
using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmsOfGhosts.App.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial class EnemyMovementSystem : SystemBase
    {
        private PauseSystem _pauseSystem;
        private Entity _player;

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

            var deltaTime = Time.DeltaTime;
            var playerTranslation = GetComponent<Translation>(_player);

            Entities.ForEach((
                ref Translation translation, ref Rotation rotation,
                in EnemyMovementSpeed speed, in EnemyTag _) =>
            {
                if (math.all(playerTranslation.Value == translation.Value))
                    return;

                var path = playerTranslation.Value - translation.Value;
                rotation.Value = quaternion.LookRotation(path, math.up());

                var movement = math.normalize(path) * speed.Value * deltaTime;
                if (math.length(movement) > math.length(path))
                    translation.Value = playerTranslation.Value;
                else
                    translation.Value += movement;
            }).ScheduleParallel();
        }

        [BurstCompile]
        protected override void OnStopRunning() => _player = Entity.Null;
    }
}