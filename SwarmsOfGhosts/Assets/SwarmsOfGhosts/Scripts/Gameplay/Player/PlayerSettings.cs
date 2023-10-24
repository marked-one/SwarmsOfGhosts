using Unity.Entities;

namespace SwarmsOfGhosts.Gameplay.Player
{
    [GenerateAuthoringComponent]
    public struct PlayerSettings : IComponentData
    {
        public float Speed;
        public float Health;
    }
}