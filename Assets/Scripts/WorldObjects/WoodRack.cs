using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;


public class WoodRack : MonoBehaviour, InteractInput.IInteractable, GameClock.ITickable, SaveData.ISaveable
{
    private class WoodRackSaveData
    {
        public int NumWetLogs;
        public int NumDryLogs;
        public List<float> LogTimers;
    }

    [SerializeField] private string _identifier = "WoodRack";
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Sprite[] _rackSprites;
    [SerializeField] private Inventory.Item _dryLog;
    [SerializeField] private Inventory.Item _wetLog;
    [SerializeField] private int _rackLogCapacity = 18;
    [SerializeField] private int _startingWetLogs = 0;
    [SerializeField] private int _startingDryLogs = 0;
    private Reactive<int> _numWetLogs = new Reactive<int>(0);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);

    private SpriteRenderer _spriteRenderer;
    private HeatSensitive _heatSensitive;

    private List<Action> _unsubscribeCBs = new();

    private const float _timeToDryGameMins = 2880f;
    [SerializeField] private List<float> _logDryingTimers = new List<float>();
    private float _temperatureMultiplier = 12f;

    private void OnEnable()
    {
        _heatSensitive = GetComponent<HeatSensitive>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _unsubscribeCBs.Add(_numWetLogs.OnChange(_ => UpdateRackSprite()));
        _unsubscribeCBs.Add(_numDryLogs.OnChange(_ => UpdateRackSprite()));
        _unsubscribeCBs.Add(_numWetLogs.OnChange((prev, curr) => PostAllLogsDryMessage(prev, curr)));
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        for (int i = 0; i < _startingWetLogs; i++)
            TryAddWetLog();
        for (int i = 0; i < _startingDryLogs; i++)
            TryAddDryLog();
        UpdateRackSprite();
    }

    private void OnDisable()
    {
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var _cb in _unsubscribeCBs)
            _cb();
        _unsubscribeCBs.Clear();
    }

    private void PostAllLogsDryMessage(int previousCount, int currentCount)
    {
        string _message = "All logs on the woodrack have dried out.";
        if (previousCount > 0 && currentCount == 0) 
        {
            if (_playerData.IsPlayerSleeping)
                Narrator.Instance.PostMessage(_message);
            else
                StartCoroutine(WaitToPostMessage(_message)); // waiting for narrator to load in
        }
    }

    private IEnumerator WaitToPostMessage(string message)
    { 
        yield return null;
        Narrator.Instance.PostMessage(message);
    }

    private void UpdateRackSprite()
    {
        _spriteRenderer.sprite = _rackSprites[_numDryLogs.Value + _numWetLogs.Value];
    }

    public void OnGameMinuteTick()
    {
        for (int i = 0; i < _logDryingTimers.Count; i++)
        {
            if (_heatSensitive.Temperature == Temperature.Hot || _heatSensitive.Temperature == Temperature.Warm)
                _logDryingTimers[i] += 1 * _temperatureMultiplier;
            else
                _logDryingTimers[i]++;
        }
        int _numOfExpiredTimers = _logDryingTimers.RemoveAll(timerCount => timerCount >= _timeToDryGameMins);
        _numWetLogs.Value -= _numOfExpiredTimers;
        _numDryLogs.Value += _numOfExpiredTimers;
    }

    public bool TryAddWetLog()
    {
        if (IsRackFull())
            return false;
        _numWetLogs.Value++;
        _logDryingTimers.Add(0);
        return true;
    }

    private bool IsRackFull()
    {
        if (_numWetLogs.Value + _numDryLogs.Value >= _rackLogCapacity)
        {
            Narrator.Instance.PostMessage("the rack is full.");
            return true;
        }
        else
            return false;
    }

    public bool TryAddDryLog()
    {
        if (IsRackFull())
            return false;
        _numDryLogs.Value++;
        return true;
    }

    public void RemoveDryLog()
    {
        // no logs on rack
        if (_numDryLogs.Value + _numWetLogs.Value == 0)
            return;

        // only wet logs on rack
        if (_numDryLogs.Value == 0 && _numWetLogs.Value > 0)
        {
            PlayerDialogue.Instance.PostMessage("this wood is wet");
            return;
        }

        if (_inventory.TryAddItem(_dryLog, 1))
            _numDryLogs.Value--;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        RemoveDryLog();
        return true;
    }

    public SaveData Save()
    {
        var _extendedData = new WoodRackSaveData()
        {
            NumWetLogs = _numWetLogs.Value,
            NumDryLogs = _numDryLogs.Value,
            LogTimers = _logDryingTimers
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(_identifier);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<WoodRackSaveData>(_extendedData);

        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<WoodRackSaveData>();
        _logDryingTimers = _extendedData.LogTimers;
        _numWetLogs.Value = _extendedData.NumWetLogs;
        _numDryLogs.Value = _extendedData.NumDryLogs;
    }

}
