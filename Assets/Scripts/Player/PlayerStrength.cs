using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStrength : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;

    private WeightyObjectStack _carryStack;
    private int _currentLevel;
    private HashSet<string> _seenObjectIDs = new();

    private void Start()
    {
        Assert.IsNotNull(_playerData);
        _carryStack = GetComponent<WeightyObjectStack>();
        Assert.IsNotNull(_carryStack);

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
        _carryStack.SetWeightCapacity(_playerData.StrengthData.GetLevelConfig(_currentLevel).CarryCapacity);
    }
}
