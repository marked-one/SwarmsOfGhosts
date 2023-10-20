using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerSpawnSettings : IComponentData
    {
        public Entity Prefab;
    }
}