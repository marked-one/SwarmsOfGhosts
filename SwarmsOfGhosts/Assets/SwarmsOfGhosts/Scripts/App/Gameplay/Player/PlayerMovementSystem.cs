using System;
using SwarmsOfGhosts.App.Gameplay.Enemy;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Restart;
using UniRx;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial class PlayerMovementSystem : SystemBase
    {
        private PauseSystem _pauseSystem;

        private readonly Subject<Vector3> _position = new Subject<Vector3>();
        public IObservable<Vector3> Position => _position;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();

            var enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            RequireForUpdate(enemyQuery);

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_pauseSystem.IsPaused)
                return;

            var deltaTime = Time.DeltaTime;

            var position = float3.zero;

            Entities.ForEach((ref Translation translation, ref Rotation rotation,
                in PlayerMovement movement, in PlayerMovementSpeed speed, in PlayerTag _) =>
            {
                translation.Value.xz += movement.Value * speed.Value * deltaTime;
                if (math.length(movement.Value) > math.EPSILON)
                {
                    var forward = new float3(movement.Value.x, 0f, movement.Value.y);
                    rotation.Value = quaternion.LookRotation(forward, math.up());
                }

                position = translation.Value;
            }).Run();

            _position.OnNext(position);
        }
    }
}