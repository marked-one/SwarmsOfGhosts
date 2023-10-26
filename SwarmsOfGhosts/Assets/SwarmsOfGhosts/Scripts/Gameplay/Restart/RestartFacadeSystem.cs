using Cysharp.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Restart
{
    public interface IRestartable
    {
        public UniTask Restart();
    }

    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [AlwaysUpdateSystem]
    public partial class RestartFacadeSystem : SystemBase, IRestartable
    {
        private UniTaskCompletionSource _restartCompletionSource;

        private const int _framesForRestart = 1;
        private int _framesCounter;

        public async UniTask Restart()
        {
            _restartCompletionSource = new UniTaskCompletionSource();
            await _restartCompletionSource.Task;
            _restartCompletionSource = null;
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_restartCompletionSource == null)
                return;

            DestroyIsPlayingEntity();

            if (_framesCounter == _framesForRestart)
            {
                CreateIsPlayingEntity();
                _framesCounter = 0;
                _restartCompletionSource.TrySetResult();
                return;
            }

            _framesCounter++;
        }

        private void CreateIsPlayingEntity()
        {
            var doesIsPlayingEntityExist = TryGetSingletonEntity<IsPlayingTag>(out var isPlayingEntity);
            if (doesIsPlayingEntityExist)
                return;

            isPlayingEntity = EntityManager.CreateEntity(typeof(IsPlayingTag));
            EntityManager.SetName(isPlayingEntity, "IsPlayingEntity");
        }

        private void DestroyIsPlayingEntity()
        {
            var doesIsPlayingEntityExist = TryGetSingletonEntity<IsPlayingTag>(out var isPlayingEntity);
            if (doesIsPlayingEntityExist)
                EntityManager.DestroyEntity(isPlayingEntity);
        }

        [BurstCompile]
        protected override void OnStopRunning() => _restartCompletionSource?.TrySetResult();
    }
}