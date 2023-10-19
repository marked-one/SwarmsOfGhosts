using Unity.Entities;

namespace SwarmsOfGhosts.Environment
{
    [GenerateAuthoringComponent]
    public struct BattleGroundSettings : IComponentData
    {
        public float YScale;
        public Entity Prefab;
    }
}