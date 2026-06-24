using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStrength : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private WeightyObjectStackData _carryData;

    private int _currentLevel;
    private HashSet<string> _seenObjectIDs = new();

    private void Start()
    {
        Assert.IsNotNull(_playerData);
        Assert.IsNotNull(_carryData);

        _currentLevel = _playerData.StrengthData.GetLevel(_playerData.TotalPickupCount);
        ApplyCarryCapacity();
    }

    public void RegisterPickup(string objectId)
    {
        if (string.IsNullOrEmpty(objectId) || !_seenObjectIDs.Add(objectId))
            return;

        _playerData.TotalPickupCount++;
        int newLevel = _playerData.StrengthData.GetLevel(_playerData.TotalPickupCount);
        if (newLevel > _currentLevel)
        {
            _currentLevel = newLevel;
            string message = _playerData.StrengthData.GetLevelConfig(_currentLevel).LevelUpMessage;
            if (!string.IsNullOrEmpty(message))
                Narrator.Instance.PostMessage(message);
            ApplyCarryCapacity();
        }
    }

    private void ApplyCarryCapacity()
    {
        _carryData.WeightCapacity = _playerData.StrengthData.GetLevelConfig(_currentLevel).CarryCapacity;
    }
}
