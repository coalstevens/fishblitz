using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public enum StoveStates {Dead, Ready, Hot, Embers};
public class WoodStoveSaveData : WorldObjectSaveData {
    public StoveStates State;
}
public class WoodStove : MonoBehaviour, IInteractable, IHeatSource, ITickable, ISaveable<WoodStoveSaveData>
{
    private const string IDENTIFIER = "WoodStove";
    private HeatSourceManager _heatSourceManager;
    private Animator _animator;
    private Inventory _inventory;
    private GameClock _gameClock;
    private Reactive<StoveStates> _stoveState = new Reactive<StoveStates>(StoveStates.Dead);
    private PulseLight _fireLight;
    public int _fireDurationCounterGameMinutes;

    [Header("Embers Settings")]
    [SerializeField] float _embersMinIntensity = 0.2f;
    [SerializeField] float _embersMaxIntensity = 1.0f;
    [SerializeField] private int _embersDurationGameMinutes = 60;

    [Header("Hot Fire Settings")]
    [SerializeField] float _fireMinIntensity = 1.3f;
    [SerializeField] float _fireMaxIntensity = 2f;
    [SerializeField] private int _hotFireDurationGameMinutes = 60;

    public Collider2D ObjCollider { 
        get {
            if (TryGetComponent<Collider2D>(out var _collider))
                return _collider;
            else {
                Debug.LogError("WoodStove does not have a collider component.");
                return null;
            }
        }
    }

    public HeatSourceManager HeatSource {
        get {
            return _heatSourceManager;
        }
    }

    public List<float> CountersGameMinutes { 
        get {
            List<float> _counters = new() {
                (float)_fireDurationCounterGameMinutes
            };
            return _counters;
        }
        set => _fireDurationCounterGameMinutes = (int) value[0];
    }

    private List<Action> _unsubscribeHooks = new();
    void Awake()
    {
        // References
        _heatSourceManager = GetComponent<HeatSourceManager>();
        _animator = GetComponent<Animator>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _gameClock = GameObject.FindGameObjectWithTag("GameClock").GetComponent<GameClock>();
        _fireLight = transform.GetComponentInChildren<PulseLight>();
        
        // Reactive
        _unsubscribeHooks.Add(_stoveState.OnChange((curr,prev) => OnStateChange()));
        _unsubscribeHooks.Add(_gameClock.GameMinute.OnChange((curr, prev) => OnGameMinuteTick()));
        EnterDead();
    }
    private void OnDisable() {
        foreach (var _hook in _unsubscribeHooks) 
            _hook();
    }

    public void OnGameMinuteTick() { 
        switch (_stoveState.Value) {
            case StoveStates.Hot:
                _fireDurationCounterGameMinutes++;
                if (_fireDurationCounterGameMinutes >= _hotFireDurationGameMinutes)
                    _stoveState.Value = StoveStates.Embers;
                break;
            case StoveStates.Embers:
                _fireDurationCounterGameMinutes++;
                if (_fireDurationCounterGameMinutes >= (_hotFireDurationGameMinutes + _embersDurationGameMinutes))
                    _stoveState.Value = StoveStates.Dead;
                break;
        } 
    }

    void OnStateChange() {
        switch (_stoveState.Value) {
            case StoveStates.Dead:
                EnterDead();
                break;
            case StoveStates.Ready:
                EnterReady();
                break;
            case StoveStates.Hot:
                EnterHot();
                break;
            case StoveStates.Embers:
                EnterEmbers();
                break;
        } 
    }

    private void EnterHot() {
        _fireDurationCounterGameMinutes = 0;
        _animator.speed = 1f;
        _animator.Play("HotFire");
        _heatSourceManager.LocalTemperature = Temperature.Hot;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_fireMinIntensity, _fireMaxIntensity);
    }

    private void EnterEmbers() {
        _animator.speed = 0.05f;
        _animator.Play("Embers");
        _heatSourceManager.LocalTemperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_embersMinIntensity, _embersMaxIntensity);
    }

    private void EnterDead() {
        _animator.speed = 1f;
        _animator.Play("Dead");
        _heatSourceManager.DisableHeatSource();
        _fireLight.gameObject.SetActive(false);
    }

    private void EnterReady() {
        _animator.speed = 1f;
        _animator.Play("Ready");
        _heatSourceManager.DisableHeatSource();
        _fireLight.gameObject.SetActive(false);
    }

    public bool CursorInteract(Vector3 cursorLocation) {
        switch (_stoveState.Value) {
            case StoveStates.Dead:
                // Add wood to ashes
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    _stoveState.Value = StoveStates.Ready;
                    return true;
                }
                return false;
            case StoveStates.Ready:
                // Start fire
                NarratorSpeechController.Instance.PostMessage("The room gets warm...");
                _stoveState.Value = StoveStates.Hot;
                return true;
            case StoveStates.Hot:
                // state internal transition, stoke fire
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    NarratorSpeechController.Instance.PostMessage("You stoke the fire...");
                    return true;
                }
                return false;   
            case StoveStates.Embers:
                // Stoke fire
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    _stoveState.Value = StoveStates.Hot;
                    NarratorSpeechController.Instance.PostMessage("You stoke the fire...");
                    return true;
                }   
                return false;
            default:
                Debug.LogError("WoodStove guard handler defaulted.");
                return false;
        } 
    }

    private void StokeFlame() {
        _inventory.TryRemoveItem("Firewood", 1);
        _fireDurationCounterGameMinutes = 0;
    }

    public WoodStoveSaveData Save()
    {
        return new WoodStoveSaveData {
            Identifier = IDENTIFIER,
            Position = new SimpleVector3(transform.position),
            State = _stoveState.Value
        };
    }

    public void Load(WoodStoveSaveData saveData)
    {
        _stoveState.Value = saveData.State;
    }
}
