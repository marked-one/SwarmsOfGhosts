using SwarmsOfGhosts.UI;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Restart
{
    public interface IRestartable
    {
        public void Restart();
    }

    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [AlwaysUpdateSystem]
    public partial class RestartableSystem : SystemBase, IRestartable
    {
        private bool _isRestarting;
        private bool _shouldRestart;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public void Restart() => _shouldRestart = true;

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (!_shouldRestart)
                return;

            _shouldRestart = false;
            _isRestarting = !_isRestarting;
            if (_isRestarting)
                DestroyIsPlayingEntity();
            else
                CreateIsPlayingEntity();
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
        protected override void OnStopRunning()
        {
            DestroyIsPlayingEntity();
            _subscriptions.Dispose();
        }
    }
}