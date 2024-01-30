using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class WoodRack : MonoBehaviour, IHeatSensitive, IInteractable, IWorldObject, IItemStorage, ITimeSensitive
{
    private SpriteRenderer _spriteRenderer;
    private GameClock _gameClock;
    private Inventory _inventory;
    private Reactive<int> _numWetLogs = new Reactive<int>(0);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);
    private const int _capacity = 18;
    private List<float> _logTimers = new List<float>();
    private float _timeToDryGameMins = 120f;
    private float _fireOnMultiplier = 1.5f;
    private HeatSensitiveManager _heatSensitiveManager;
    [SerializeField] private Sprite[] _rackSprites; 
    
    private const string IDENTIFIER = "WoodRack";

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

    public string Identifier {
        get => IDENTIFIER;
    }
    
    // the state in WoodRack is defined by _numWetlogs and _numDrylogs
    // itemQuantityPair is used to load/unload this object
    public int State { 
        get => 0;
        set {
            //do nothing
        }
    }

    public Dictionary<string, int> ItemQuantities { 
        get {
            var _items = new Dictionary<string, int> {
                {"WetLog", _numWetLogs.Value},
                {"DryLog", _numDryLogs.Value}
            };
            return _items;
        }
        set {
            _numDryLogs.Value = value["DryLog"];
            _numWetLogs.Value = value["WetLog"];
        }
    }

    HeatSensitiveManager IHeatSensitive.HeatSensitive {
        get => _heatSensitiveManager;
    }
    public List<float> CountersGameMinutes { 
        get => _logTimers;
        set => _logTimers = value; 
    }

    private List<Action> _unsubscribeHooks = new();
    
    void Awake()
    {   
        // References
        _heatSensitiveManager = GetComponent<HeatSensitiveManager>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _gameClock = GameObject.FindGameObjectWithTag("GameClock").GetComponent<GameClock>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();

        // Reactive
        _unsubscribeHooks.Add(_numWetLogs.OnChange((prev,curr) => UpdateRackSprite()));
        _unsubscribeHooks.Add(_numDryLogs.OnChange((prev,curr) => UpdateRackSprite()));
        _unsubscribeHooks.Add(_gameClock.GameMinute.OnChange((prev,curr) => OnGameMinuteTick()));
    }

    private void Start() {
        UpdateRackSprite();
    }
    private void OnDisable() {
        foreach(var _hook in _unsubscribeHooks)
            _hook();
    }

    void UpdateRackSprite() {
        _spriteRenderer.sprite = _rackSprites[_numDryLogs.Value+_numWetLogs.Value];
    }

    public void OnGameMinuteTick() {
        for (int i = 0; i < _logTimers.Count; i++) {
            if (_heatSensitiveManager.LocalTemperature == Temperature.Hot || _heatSensitiveManager.LocalTemperature == Temperature.Warm) {
                _logTimers[i] +=  1 * _fireOnMultiplier;
            }
            else {
                _logTimers[i]++;
            }
        }
        int _numOfExpiredTimers = _logTimers.RemoveAll(timerCount => timerCount >= _timeToDryGameMins);
        _numWetLogs.Value -= _numOfExpiredTimers;
        _numDryLogs.Value += _numOfExpiredTimers;
    }

    public void AddWetLog() {
        _inventory.TryRemoveItem("WetLog", 1);
        _numWetLogs.Value++;
        _logTimers.Add(0);
    }

    private bool IsRackFull() {
        if (_numWetLogs.Value + _numDryLogs.Value >= _capacity) {
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
}
