using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerStrength : MonoBehaviour, ISaveable
{
    public string SaveableId => "PlayerStrength";

    [SerializeField] private StrengthData _strengthData;
    public StrengthData StrengthData => _strengthData;
    public int TotalPickupCount = 0;

    private WeightyObjectStack _carryStack;
    private PlayerCarry _playerCarry;
    private int _currentLevel;
    private HashSet<string> _seenObjectIDs = new();

    [System.Serializable]
    private class State
    {
        public int TotalPickupCount;
    }

    private void Start()
    {
        _playerCarry = GetComponent<PlayerCarry>();
        _carryStack = _playerCarry.CarriedStack;

        _currentLevel = _strengthData.GetLevel(TotalPickupCount);
        ApplyCarryCapacity();
    }

    public void RegisterPickup(string objectId)
    {
        if (string.IsNullOrEmpty(objectId) || !_seenObjectIDs.Add(objectId))
            return;

        TotalPickupCount++;
        int newLevel = _strengthData.GetLevel(TotalPickupCount);
        if (newLevel > _currentLevel)
        {
            _currentLevel = newLevel;
            string message = _strengthData.GetLevelConfig(_currentLevel).LevelUpMessage;
            if (!string.IsNullOrEmpty(message))
                Narrator.Instance.PostMessage(message);
            ApplyCarryCapacity();
        }
    }

    private void ApplyCarryCapacity()
    {
        _carryStack.Capacity = _strengthData.GetLevelConfig(_currentLevel).CarryCapacity;
    }

    public string CaptureState()
    {
        var _state = new State { TotalPickupCount = TotalPickupCount };
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreState(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        TotalPickupCount = _state.TotalPickupCount;
    }

    public void ResetState()
    {
        TotalPickupCount = 0;
    }
}
