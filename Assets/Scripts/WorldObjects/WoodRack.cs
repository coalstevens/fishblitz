using System;
using System.Collections.Generic;
using ReactiveUnity;
using Unity.Collections;
using UnityEngine;


public class WoodRack : MonoBehaviour, PlayerInteractionManager.IInteractable, GameClock.ITickable, SceneSaveLoadManager.ISaveable
{
    private class WoodRackSaveData
    {
        public int NumWetLogs;
        public int NumDryLogs;
        public List<float> LogTimers;
    }

    [SerializeField] private Inventory _inventory;
    [SerializeField] private Sprite[] _rackSprites;
    [SerializeField] private Inventory.ItemType _dryLog;
    [SerializeField] private Inventory.ItemType _wetLog;

    private SpriteRenderer _spriteRenderer;
    private HeatSensitive _heatSensitive;

    private Reactive<int> _numWetLogs = new Reactive<int>(0);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);
    private List<Action> _unsubscribeCBs = new();

    private const string IDENTIFIER = "WoodRack";
    private const int _rackLogCapacity = 18;
    private const float _timeToDryGameMins = 120f;
    private List<float> _logDryingTimers = new List<float>();
    private float _temperatureMultiplier = 1.5f;



    public Collider2D ObjCollider
    {
        get
        {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null)
            {
                return _collider;
            }
            else
            {
                Debug.LogError("Woodrack does not have a collider component");
                return null;
            }
        }
    }

    private void Awake()
    {
        // References
        _heatSensitive = GetComponent<HeatSensitive>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateRackSprite();
    }

    private void OnEnable()
    {
        _unsubscribeCBs.Add(_numWetLogs.OnChange((prev, curr) => UpdateRackSprite()));
        _unsubscribeCBs.Add(_numDryLogs.OnChange((prev, curr) => UpdateRackSprite()));
        _unsubscribeCBs.Add(_numWetLogs.OnChange((prev, curr) => AllLogsDry(prev, curr)));
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
    }

    private void OnDisable()
    {
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var _cb in _unsubscribeCBs)
            _cb();
    }

    private void AllLogsDry(int previousCount, int currentCount)
    {
        if (previousCount > 0 && currentCount == 0 && _numDryLogs.Value > 0)
            Narrator.Instance.PostMessage("All logs on the woodrack have dried out.");
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
            {
                _logDryingTimers[i] += 1 * _temperatureMultiplier;
            }
            else
            {
                _logDryingTimers[i]++;
            }
        }
        int _numOfExpiredTimers = _logDryingTimers.RemoveAll(timerCount => timerCount >= _timeToDryGameMins);
        _numWetLogs.Value -= _numOfExpiredTimers;
        _numDryLogs.Value += _numOfExpiredTimers;
    }

    public void AddWetLog()
    {
        _inventory.TryRemoveItem(_wetLog, 1);
        _numWetLogs.Value++;
        _logDryingTimers.Add(0);
    }

    private bool IsRackFull()
    {
        if (_numWetLogs.Value + _numDryLogs.Value >= _rackLogCapacity)
        {
            PlayerDialogue.Instance.PostMessage("I can't fit anymore...");
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddDryLog()
    {
        _inventory.TryRemoveItem(_dryLog, 1);
        _numDryLogs.Value++;
    }

    public void RemoveDryLog()
    {
        // no logs on rack
        if (_numDryLogs.Value + _numWetLogs.Value == 0)
            return;

        // only wet logs on rack
        if (_numDryLogs.Value == 0 && _numWetLogs.Value > 0)
        {
            PlayerDialogue.Instance.PostMessage("These are all still wet...");
            return;
        }

        // Add a dry log to inventory
        if (_inventory.TryAddItem(_dryLog, 1))
        {
            _numDryLogs.Value--;
        }
        else
        {
            PlayerDialogue.Instance.PostMessage("I'm all full up...");
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_inventory.TryGetActiveItemType(out var _activeItem))
        {
            switch (_activeItem.ItemLabel)
            {
                case "DryLog":
                    if (!IsRackFull())
                    {
                        AddDryLog();
                    }
                    break;
                case "WetLog":
                    if (!IsRackFull())
                    {
                        AddWetLog();
                    }
                    break;
                default:
                    RemoveDryLog();
                    break;
            }
        }
        else
        {
            RemoveDryLog();
        }
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
        _saveData.AddIdentifier(IDENTIFIER);
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
