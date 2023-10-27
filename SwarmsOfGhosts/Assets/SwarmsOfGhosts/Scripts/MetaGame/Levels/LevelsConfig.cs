using UnityEngine;

namespace SwarmsOfGhosts.MetaGame.Levels
{
    public interface ILevelsConfig
    {
        public int GridStep { get; }
        public int MaxSteps { get; }
    }

    [CreateAssetMenu(fileName = "LevelsConfig", menuName = "Configs/LevelsConfig", order = 0)]
    public class LevelsConfig : ScriptableObject, ILevelsConfig
    {
        [SerializeField] private int _gridStep = 5;
        [SerializeField] private int _maxSteps = 30;

        public int GridStep => _gridStep;
        public int MaxSteps => _maxSteps;
    }
}