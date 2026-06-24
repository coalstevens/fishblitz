using UnityEngine;

[CreateAssetMenu(fileName = "NewStrengthData", menuName = "Player/StrengthData")]
public class StrengthData : ScriptableObject
{
    [System.Serializable]
    public struct LevelConfig
    {
        public int PickupsRequired;
        public int CarryCapacity;
        public string LevelUpMessage;
    }

    [SerializeField] private LevelConfig[] _levels;

    public int LevelCount => _levels.Length;

    public int GetLevel(int totalPickups)
    {
        for (int i = _levels.Length - 1; i >= 0; i--)
        {
            if (totalPickups >= _levels[i].PickupsRequired)
                return i;
        }
        return 0;
    }

    public LevelConfig GetLevelConfig(int level)
    {
        if (_levels == null || _levels.Length == 0)
            return new LevelConfig { PickupsRequired = 0, CarryCapacity = 0 };
        return _levels[Mathf.Clamp(level, 0, _levels.Length - 1)];
    }
}
