using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WoodRack : MonoBehaviour, IHeatSensitive, IInteractable, IWorldObject, IItemStorage
{
    private SpriteRenderer _spriteRenderer;
    private GameClock _gameClock;
    private Inventory _inventory;
    private Reactive<int> _numWetLogs = new Reactive<int>(3);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);
    private const int _capacity = 18;
    private List<float> _logTimers = new List<float>();
    private float _timeToDryGameMins = 120f;
    private float _fireOnMultiplier = 1.5f;

    [SerializeField] Sprite _empty;
    [SerializeField] Sprite _oneLog;
    [SerializeField] Sprite _twoLog;
    [SerializeField] Sprite _threeLog;
    [SerializeField] Sprite _fourLog;
    [SerializeField] Sprite _fiveLog;
    [SerializeField] Sprite _sixLog;
    [SerializeField] Sprite _sevenLog;
    [SerializeField] Sprite _eightLog;
    [SerializeField] Sprite _nineLog;
    [SerializeField] Sprite _tenLog;
    [SerializeField] Sprite _elevenLog;
    [SerializeField] Sprite _twelveLog;
    [SerializeField] Sprite _thirteenLog;
    [SerializeField] Sprite _fourteenLog;
    [SerializeField] Sprite _fifteenLog;
    [SerializeField] Sprite _sixteenLog;
    [SerializeField] Sprite _seventeenLog;
    [SerializeField] Sprite _eighteenLog;
    
    private Temperature _localTemperature;
    public Temperature LocalTemperature { 
        get => _localTemperature;
        set => _localTemperature = value;
    }
    private const string IDENTIFIER = "WoodRack";
    private Dictionary<string, int> _savedItems = new Dictionary<string, int>();

    public Collider2D InteractCollider {
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
            _savedItems["WetLog"] = _numWetLogs.Value;
            _savedItems["DryLog"] = _numDryLogs.Value;
            return _savedItems;
        }
        set {
            _numDryLogs.Value = value["DryLog"];
            _numWetLogs.Value = value["WetLog"];
        }
    }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _gameClock = GameObject.FindGameObjectWithTag("GameClock").GetComponent<GameClock>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _numWetLogs.OnChange((prev,curr) => UpdateRackSprite());
        _numDryLogs.OnChange((prev,curr) => UpdateRackSprite());
        _gameClock.GameMinute.OnChange((prev,curr) => OnGameMinuteChange());
        _savedItems.Add("WetLog", _numWetLogs.Value);
        _savedItems.Add("DryLog", _numDryLogs.Value);
        UpdateRackSprite();
    }

    void UpdateRackSprite() {
        switch (_numDryLogs.Value + _numWetLogs.Value) {
            case 0:
                _spriteRenderer.sprite = _empty;
                break;
            case 1:
                _spriteRenderer.sprite = _oneLog;
                break;
            case 2:
                _spriteRenderer.sprite = _twoLog;
                break;
            case 3:
                _spriteRenderer.sprite = _threeLog;
                break;
            case 4:
                _spriteRenderer.sprite = _fourLog;
                break;
            case 5:
                _spriteRenderer.sprite = _fiveLog;
                break;
            case 6:
                _spriteRenderer.sprite = _sixLog;
                break;
            case 7:
                _spriteRenderer.sprite = _sevenLog;
                break;
            case 8:
                _spriteRenderer.sprite = _eightLog;
                break;
            case 9:
                _spriteRenderer.sprite = _nineLog;
                break;
            case 10:
                _spriteRenderer.sprite = _tenLog;
                break;
            case 11:
                _spriteRenderer.sprite = _elevenLog;
                break;
            case 12:
                _spriteRenderer.sprite = _twelveLog;
                break;
            case 13:
                _spriteRenderer.sprite = _thirteenLog;
                break;
            case 14:
                _spriteRenderer.sprite = _fourteenLog;
                break;
            case 15:
                _spriteRenderer.sprite = _fifteenLog;
                break;
            case 16:
                _spriteRenderer.sprite = _sixteenLog;
                break;
            case 17:
                _spriteRenderer.sprite = _seventeenLog;
                break;
            case 18:
                _spriteRenderer.sprite = _eighteenLog;
                break;
            default:
                break;
        }
    }
    void OnGameMinuteChange() {
        for (int i = 0; i < _logTimers.Count; i++) {
            if (_localTemperature == Temperature.Hot || _localTemperature == Temperature.Warm) {
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
        _inventory.RemoveItem("WetLog", 1);
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
        _inventory.RemoveItem("DryLog", 1);
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
        if (_inventory.AddItem("DryLog", 1)) {
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
