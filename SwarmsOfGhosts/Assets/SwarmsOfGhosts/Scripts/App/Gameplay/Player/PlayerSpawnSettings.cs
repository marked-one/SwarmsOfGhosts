using Unity.Entities;

namespace SwarmsOfGhosts.App.Gameplay.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerSpawnSettings : IComponentData
    {
        public Entity Prefab;
    }
}