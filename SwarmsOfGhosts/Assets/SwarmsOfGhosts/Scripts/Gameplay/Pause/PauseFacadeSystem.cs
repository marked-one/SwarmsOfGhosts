using SwarmsOfGhosts.Gameplay.Restart;
using UniRx;
using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Pause
{
    public interface IPausable
    {
        public IReadOnlyReactiveProperty<bool> IsPaused { get; }
        public void Unpause();
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(PauseSystem))]
    public partial class PauseFacadeSystem : SystemBase, IPausable
    {
        private PauseSystem _pauseSystem;

        private readonly ReactiveProperty<bool> _isPaused = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsPaused => _isPaused;

        public void Unpause() => _pauseSystem.IsPaused = false;

        [BurstCompile]
        protected override void OnCreate()
        {
            _pauseSystem = World.GetOrCreateSystem<PauseSystem>();
            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate() => _isPaused.Value = _pauseSystem.IsPaused;
    }
}