using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class Campfire : MonoBehaviour, InteractInput.IInteractable, GameClock.ITickable, SaveData.ISaveable
{
    private enum FireStates {Dead, Ready, Hot, Embers};
    private const string IDENTIFIER = "Campfire";
    private class CampfireSaveData
    {
        public FireStates State;
        public int FireDurationCounterGameMinutes;
    }

    private Animator _animator;
    private LocalHeatSource _localHeatSource;
    private Reactive<FireStates> _stoveState = new Reactive<FireStates>(FireStates.Dead);
    private PulseLight _fireLight;
    public int _fireDurationCounterGameMinutes;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Inventory.Item _firewood;
    [SerializeField] private Inventory.Item _dryWood;
    [SerializeField] private Inventory.Item _wetWood;

    [Header("Embers Settings")]
    [SerializeField] float _embersMinIntensity = 0.2f;
    [SerializeField] float _embersMaxIntensity = 1.0f;
    [SerializeField] private int _embersDurationGameMinutes = 30;

    [Header("Hot Fire Settings")]
    [SerializeField] float _fireMinIntensity = 1.3f;
    [SerializeField] float _fireMaxIntensity = 2f;
    [SerializeField] private int _hotFireDurationGameMinutes = 30;
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        _localHeatSource = GetComponent<LocalHeatSource>();
        _fireLight = transform.GetComponentInChildren<PulseLight>();

        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        _unsubscribeHooks.Add(_stoveState.OnChange((curr, prev) => OnStateChange()));
        EnterDead();
    }

    private void OnDisable()
    {
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var _hook in _unsubscribeHooks)
            _hook();
    }

    public void OnGameMinuteTick()
    {
        switch (_stoveState.Value)
        {
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

    void OnStateChange()
    {
        switch (_stoveState.Value)
        {
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

    private void EnterHot()
    {
        _fireDurationCounterGameMinutes = 0;
        _animator.speed = 1f;
        _animator.Play("HotFire");
        _localHeatSource.enabled = true;
        _localHeatSource.Temperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_fireMinIntensity, _fireMaxIntensity);
    }

    private void EnterEmbers()
    {
        _animator.speed = 0.05f;
        _animator.Play("Embers");
        _localHeatSource.enabled = true;
        _localHeatSource.Temperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_embersMinIntensity, _embersMaxIntensity);
    }

    private void EnterDead()
    {
        _animator.speed = 1f;
        _animator.Play(""); // this little reset fixes some jank when the fire dies in a different scene
        _animator.Play("Dead");
        _localHeatSource.enabled = false;
        _fireLight.gameObject.SetActive(false);
    }

    private void EnterReady()
    {
        _animator.speed = 1f;
        _animator.Play("Ready");
        _localHeatSource.enabled = false;
        _fireLight.gameObject.SetActive(false);
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        switch (_stoveState.Value)
        {
            case FireStates.Dead:
                return true;
            case FireStates.Ready: // Start fire
                _stoveState.Value = FireStates.Hot;
                Narrator.Instance.PostMessage("the room grows warm.");
                return true;
            default:
                return false;
        }
    }

    public bool AddFirewood() {
        if (_stoveState.Value == FireStates.Ready)
            return false;

        _fireDurationCounterGameMinutes = 0;

        if (_stoveState.Value == FireStates.Dead)
        {
            _stoveState.Value = FireStates.Ready;
            return true;
        }
        if (_stoveState.Value == FireStates.Hot)
        {
            Narrator.Instance.PostMessage("you stoke the flames.");
            return true;
        }
        if (_stoveState.Value == FireStates.Embers)
        {
            _stoveState.Value = FireStates.Hot;
            Narrator.Instance.PostMessage("you stoke the flames.");
            return true;
        }

        Debug.LogError("State not handled for adding firewood.");
        return false;
    }

    public SaveData Save()
    {
        var _extendedData = new CampfireSaveData
        {
            State = _stoveState.Value,
            FireDurationCounterGameMinutes = _fireDurationCounterGameMinutes
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<CampfireSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<CampfireSaveData>();
        _stoveState.Value = _extendedData.State;
        _fireDurationCounterGameMinutes = _extendedData.FireDurationCounterGameMinutes;
    }
}
