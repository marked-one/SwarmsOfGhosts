using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;

namespace SwarmsOfGhosts.Gameplay.Enemy
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StepPhysicsWorld))]
    public partial class ProjectileHitSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            
        }
    }
}