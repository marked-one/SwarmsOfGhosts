using Cysharp.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Restart
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [AlwaysUpdateSystem]
    public partial class RestartSystem : SystemBase
    {
        private UniTaskCompletionSource _restartCompletionSource;

        private const int _framesForRestart = 1;
        private int _framesCounter;

        private bool _isAlive;

        [BurstCompile]
        protected override void OnCreate() => _isAlive = true;

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_restartCompletionSource == null)
                return;

            switch (_framesCounter)
            {
                case 0:
                    DestroyIsPlayingEntity();
                    break;

                // We need at least a 1 frame a delay before starting
                // again, to allow systems free their resources.
                case _framesForRestart:
                    CreateIsPlayingEntity();
                    _framesCounter = 0;
                    _restartCompletionSource.TrySetResult();
                    return;
            }

            _framesCounter++;
        }

        // In theory, the Entity with IsPlayingTag should be instantiated when scene or system is loaded.
        // But this makes behaviour unpredictable as the same code works differently between the Editor
        // with open subscene, the Editor with closed subscene and the built app. So that Entity exists
        // in the subscene right away. Subsequent destructions and creations of the Entity work fine.

        private void CreateIsPlayingEntity()
        {
            if (!_isAlive)
                return;

            var doesIsPlayingEntityExist = TryGetSingletonEntity<IsPlayingTag>(out var isPlayingEntity);
            if (doesIsPlayingEntityExist)
                return;

            isPlayingEntity = EntityManager.CreateEntity(typeof(IsPlayingTag));
            EntityManager.SetName(isPlayingEntity, "IsPlayingEntity");
        }

        public async UniTask Restart()
        {
            _restartCompletionSource = new UniTaskCompletionSource();
            await _restartCompletionSource.Task;
            _restartCompletionSource = null;
        }

        public void DestroyIsPlayingEntity()
        {
            if (!_isAlive)
                return;

            var doesIsPlayingEntityExist = TryGetSingletonEntity<IsPlayingTag>(out var isPlayingEntity);
            if (doesIsPlayingEntityExist)
                EntityManager.DestroyEntity(isPlayingEntity);
        }

        [BurstCompile]
        protected override void OnStopRunning() => _restartCompletionSource?.TrySetResult();

        [BurstCompile]
        protected override void OnDestroy() => _isAlive = false;
    }
}