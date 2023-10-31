using Unity.Burst;
using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Pause
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class PauseSystem : SystemBase
    {
        public bool IsPaused { get; set; }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}