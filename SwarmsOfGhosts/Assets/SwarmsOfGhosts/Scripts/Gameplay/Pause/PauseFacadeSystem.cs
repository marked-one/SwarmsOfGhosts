using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Pause
{
    public interface IPausable
    {
        public void SetPause(bool paused);
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class PauseFacadeSystem : SystemBase, IPausable
    {
        public bool IsPaused { get; private set; }

        public void SetPause(bool paused) => IsPaused = paused;

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}