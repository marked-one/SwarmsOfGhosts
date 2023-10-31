using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Environment
{
    [GenerateAuthoringComponent]
    public struct BattleGroundSettings : IComponentData
    {
        public float YScale;
        public Entity Prefab;
    }
}