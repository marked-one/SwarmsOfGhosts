using UnityEngine;

namespace SwarmsOfGhosts.App.Gameplay.Levels
{
    public interface ILevelsConfig
    {
        public int EnemyGridStep { get; }
        public int EnemyGridMaxSteps { get; }
    }

    [CreateAssetMenu(fileName = "LevelsConfig", menuName = "Configs/LevelsConfig", order = 0)]
    public class LevelsConfig : ScriptableObject, ILevelsConfig
    {
        [SerializeField] private int _enemyGridStep = 5;
        [SerializeField] private int _enemyGridMaxSteps = 30;

        public int EnemyGridStep => _enemyGridStep;
        public int EnemyGridMaxSteps => _enemyGridMaxSteps;
    }
}