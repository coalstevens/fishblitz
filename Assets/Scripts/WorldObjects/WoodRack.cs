using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class WoodRackSaveData : WorldObjectSaveData {
    public int NumWetLogs;
    public int NumDryLogs;
    public List <float> LogTimers;
}

public class WoodRack : MonoBehaviour, IHeatSensitive, IInteractable, ITickable, ISaveable<WoodRackSaveData>
{
    // References
    private SpriteRenderer _spriteRenderer;
    private Inventory _inventory;
    private HeatSensitiveManager _heatSensitiveManager;

    // Reactive
    private Reactive<int> _numWetLogs = new Reactive<int>(0);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);
    private List<Action> _unsubscribeHooks = new();
    
    // Basic Fields
    private const string IDENTIFIER = "WoodRack";
    private const int _rackLogCapacity = 18;
    private const float _timeToDryGameMins = 120f;
    private List<float> _logDryingTimers = new List<float>();
    private float _temperatureMultiplier = 1.5f;

    // Inspector    
    [SerializeField] private Sprite[] _rackSprites; 

    public Collider2D ObjCollider {
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null) {
                return _collider;
            }
            else {
                Debug.LogError("Woodrack does not have a collider component");
                return null;
            }
        }
    }
    HeatSensitiveManager IHeatSensitive.HeatSensitive {
        get => _heatSensitiveManager;
    }
    
    private void Start()
    {   
        // References
        _heatSensitiveManager = GetComponent<HeatSensitiveManager>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        UpdateRackSprite();
    }

    private void OnEnable() {
        _unsubscribeHooks.Add(_numWetLogs.OnChange((prev,curr) => UpdateRackSprite()));
        _unsubscribeHooks.Add(_numDryLogs.OnChange((prev,curr) => UpdateRackSprite()));
        _unsubscribeHooks.Add(GameClock.Instance.GameMinute.OnChange((prev,curr) => OnGameMinuteTick()));
    }

    private void OnDisable() {
        foreach(var _hook in _unsubscribeHooks)
            _hook();
    }

    void UpdateRackSprite() {
        _spriteRenderer.sprite = _rackSprites[_numDryLogs.Value+_numWetLogs.Value];
    }

    public void OnGameMinuteTick() {
        for (int i = 0; i < _logDryingTimers.Count; i++) {
            if (_heatSensitiveManager.LocalTemperature == Temperature.Hot || _heatSensitiveManager.LocalTemperature == Temperature.Warm) {
                _logDryingTimers[i] +=  1 * _temperatureMultiplier;
            }
            else {
                _logDryingTimers[i]++;
            }
        }
        int _numOfExpiredTimers = _logDryingTimers.RemoveAll(timerCount => timerCount >= _timeToDryGameMins);
        _numWetLogs.Value -= _numOfExpiredTimers;
        _numDryLogs.Value += _numOfExpiredTimers;
    }

    public void AddWetLog() {
        _inventory.TryRemoveItem("WetLog", 1);
        _numWetLogs.Value++;
        _logDryingTimers.Add(0);
    }

    private bool IsRackFull() {
        if (_numWetLogs.Value + _numDryLogs.Value >= _rackLogCapacity) {
            PlayerDialogueController.Instance.PostMessage("I can't fit anymore...");
            return true;
        } 
        else {
            return false;
        }
    }

    public void AddDryLog() {
        _inventory.TryRemoveItem("DryLog", 1);
        _numDryLogs.Value++;
    }

    public void RemoveDryLog() {
        // no logs on rack
        if (_numDryLogs.Value + _numWetLogs.Value == 0)
            return;
        
        // only wet logs on rack
        if (_numDryLogs.Value == 0 && _numWetLogs.Value > 0) {
            PlayerDialogueController.Instance.PostMessage("These are all still wet...");
            return;
        }

        // Add a dry log to inventory
        if (_inventory.TryAddItem("DryLog", 1)) {
            _numDryLogs.Value--;
        }
        else {
            PlayerDialogueController.Instance.PostMessage("I'm all full up...");
        }
    }
    
    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_inventory.TryGetActiveItem(out var _activeItem)) {
            switch (_activeItem.ItemName) {
                case "DryLog":
                    if (!IsRackFull()) {
                        AddDryLog();
                    }
                    break;
                case "WetLog":
                    if (!IsRackFull()) {
                        AddWetLog();
                    }
                    break;
                default:
                    RemoveDryLog();
                    break;
            }
        }
        else {
            RemoveDryLog();
        }
        return true;
    }

    public WoodRackSaveData Save() {
        return new WoodRackSaveData() {
            Identifier = IDENTIFIER,
            Position = new SimpleVector3(transform.position),
            NumWetLogs = _numWetLogs.Value,
            NumDryLogs = _numDryLogs.Value,
            LogTimers = _logDryingTimers
        };
    }

    public void Load(WoodRackSaveData saveData)
    {
        _logDryingTimers = saveData.LogTimers;
        _numWetLogs.Value = saveData.NumWetLogs;
        _numDryLogs.Value = saveData.NumDryLogs;
    }

}
