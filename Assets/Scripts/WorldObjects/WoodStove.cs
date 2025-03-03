using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public enum FireStates {Dead, Ready, Hot, Embers};
public class WoodStove : MonoBehaviour, PlayerInteractionManager.IInteractable, GameClock.ITickable, SceneSaveLoadManager.ISaveable
{
    private const string IDENTIFIER = "WoodStove";
    private class WoodStoveSaveData {
        public FireStates State;
        public int FireDurationCounterGameMinutes;
    }
    private Animator _animator;
    private GameClock _gameClock;
    private LocalHeatSource _localHeatSource;
    private Reactive<FireStates> _stoveState = new Reactive<FireStates>(FireStates.Dead);
    private PulseLight _fireLight;
    public int _fireDurationCounterGameMinutes;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Inventory.ItemType _firewood;
    [Header("Embers Settings")]
    [SerializeField] float _embersMinIntensity = 0.2f;
    [SerializeField] float _embersMaxIntensity = 1.0f;
    [SerializeField] private int _embersDurationGameMinutes = 60;

    [Header("Hot Fire Settings")]
    [SerializeField] float _fireMinIntensity = 1.3f;
    [SerializeField] float _fireMaxIntensity = 2f;
    [SerializeField] private int _hotFireDurationGameMinutes = 60;
    private List<Action> _unsubscribeHooks = new();
    
    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        _localHeatSource = GetComponent<LocalHeatSource>();
        _fireLight = transform.GetComponentInChildren<PulseLight>();
        
        _unsubscribeHooks.Add(_stoveState.OnChange((curr,prev) => OnStateChange()));
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;

        EnterDead();
    }

    private void OnDisable() {
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var _hook in _unsubscribeHooks) 
            _hook();
    }

    public void OnGameMinuteTick() { 
        switch (_stoveState.Value) {
            case FireStates.Hot:
                _fireDurationCounterGameMinutes++;
                if (_fireDurationCounterGameMinutes >= _hotFireDurationGameMinutes)
                    _stoveState.Value = FireStates.Embers;
                break;
            case FireStates.Embers:
                _fireDurationCounterGameMinutes++;
                if (_fireDurationCounterGameMinutes >= (_hotFireDurationGameMinutes + _embersDurationGameMinutes))
                    _stoveState.Value = FireStates.Dead;
                break;
        } 
    }

    void OnStateChange() {
        switch (_stoveState.Value) {
            case FireStates.Dead:
                EnterDead();
                break;
            case FireStates.Ready:
                EnterReady();
                break;
            case FireStates.Hot:
                EnterHot();
                break;
            case FireStates.Embers:
                EnterEmbers();
                break;
        } 
    }

    private void EnterHot() {
        _fireDurationCounterGameMinutes = 0;
        _animator.speed = 1f;
        _animator.Play("HotFire");
        _localHeatSource.enabled = true;
        _localHeatSource.Temperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_fireMinIntensity, _fireMaxIntensity);
    }

    private void EnterEmbers() {
        _animator.speed = 0.05f;
        _animator.Play("Embers");
        _localHeatSource.enabled = true;
        _localHeatSource.Temperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_embersMinIntensity, _embersMaxIntensity);
    }

    private void EnterDead() {
        _animator.speed = 1f;
        _animator.Play("Dead");
        _localHeatSource.enabled = false;
        _fireLight.gameObject.SetActive(false);
    }

    private void EnterReady() {
        _animator.speed = 1f;
        _animator.Play("Ready");
        _localHeatSource.enabled = false;
        _fireLight.gameObject.SetActive(false);
    }

    public bool CursorInteract(Vector3 cursorLocation) {
        switch (_stoveState.Value) {
            case FireStates.Dead:
                // Add wood to ashes
                if (_inventory.IsPlayerHoldingItem(_firewood)) {
                    StokeFlame();
                    _stoveState.Value = FireStates.Ready;
                    return true;
                }
                return false;
            case FireStates.Ready:
                // Start fire
                NarratorSpeechController.Instance.PostMessage("The room gets warm...");
                _stoveState.Value = FireStates.Hot;
                return true;
            case FireStates.Hot:
                // state internal transition, stoke fire
                if (_inventory.IsPlayerHoldingItem(_firewood)) {
                    StokeFlame();
                    NarratorSpeechController.Instance.PostMessage("You stoke the fire...");
                    return true;
                }
                return false;   
            case FireStates.Embers:
                // Stoke fire
                if (_inventory.IsPlayerHoldingItem(_firewood)) {
                    StokeFlame();
                    _stoveState.Value = FireStates.Hot;
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
        _inventory.TryRemoveItem(_firewood, 1);
        _fireDurationCounterGameMinutes = 0;
    }

    public SaveData Save()
    {
        var _extendedData = new WoodStoveSaveData {
            State = _stoveState.Value,
            FireDurationCounterGameMinutes = _fireDurationCounterGameMinutes
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<WoodStoveSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<WoodStoveSaveData>();
        _stoveState.Value = _extendedData.State;
        _fireDurationCounterGameMinutes = _extendedData.FireDurationCounterGameMinutes;
    }
}
