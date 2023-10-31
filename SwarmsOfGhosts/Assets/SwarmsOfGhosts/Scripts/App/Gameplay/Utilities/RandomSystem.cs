using SwarmsOfGhosts.App.Gameplay.Restart;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SwarmsOfGhosts.App.Gameplay.Utilities
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class RandomSystem : SystemBase
    {
        public NativeArray<Random> RandomArray { get; private set; }
        public Random MainThreadRandom { get; set; }

        [BurstCompile]
        protected override void OnCreate()
        {
            const int maxJobThreadCount = JobsUtility.MaxJobThreadCount;
            var randomArray = new Random[maxJobThreadCount];
            var seed = new System.Random();
            MainThreadRandom = new Random((uint)seed.Next(1, int.MaxValue));
            for (var i = 0; i < maxJobThreadCount; i++)
                randomArray[i] = new Random((uint)seed.Next(1, int.MaxValue));

            RandomArray = new NativeArray<Random>(randomArray, Allocator.Persistent);

            RequireSingletonForUpdate<IsPlayingTag>();
        }

        [BurstCompile]
        protected override void OnUpdate() { }

        [BurstCompile]
        protected override void OnDestroy() => RandomArray.Dispose();
    }
}