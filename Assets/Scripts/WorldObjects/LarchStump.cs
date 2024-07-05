using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class LarchStump : MonoBehaviour, IInteractable, IUseableWithAxe, ISaveable
{
    private const string IDENTIFIER = "LarchStump";
    private enum StumpStates { AxeIn, LogOn, Splitting, Idle };
    private class StumpSaveData
    {
        public StumpStates State;
    }

    [SerializeField] private Sprite _axeIn;
    [SerializeField] private Sprite _logOn;
    [SerializeField] private Sprite _idle;
    [SerializeField] private Reactive<StumpStates> _state = new Reactive<StumpStates>(StumpStates.Idle);
    [SerializeField] private AudioClip _fallingSplitWoodSFX;

    private Inventory _inventory;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private List<Action> _unsubscribeHooks = new();

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _animator.enabled = false;
        OnStateChange();
    }

    private void OnEnable()
    {
        _unsubscribeHooks.Add(_state.OnChange((curr, prev) => OnStateChange()));
    }

    private void OnDisable()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook();
    }

    void OnStateChange()
    {
        switch (_state.Value)
        {
            case StumpStates.AxeIn:
                _spriteRenderer.sprite = _axeIn;
                return;
            case StumpStates.LogOn:
                _spriteRenderer.sprite = _logOn;
                return;
            case StumpStates.Splitting:
                _animator.enabled = true;
                _animator.Play("Splitting");
                StartCoroutine(WaitForAnimationThenSpawnFirewood());
                return;
            case StumpStates.Idle:
                _spriteRenderer.sprite = _idle;
                return;
            default:
                Debug.LogError("LarchStump state machine defaulted.");
                break;
        }
    }

    private Vector3[] GetFirewoodSpawnPositions()
    {
        // Positions were determined via trial and error to match location with splitting animation
        Vector3[] _spawnPositions = {
                                     new Vector3(transform.position.x + 0.7f, transform.position.y + 0.7f, 0),
                                     new Vector3(transform.position.x - 1.1f, transform.position.y + 0.5f, 0),
                                     new Vector3(transform.position.x + 0.9f, transform.position.y - 0.3f, 0),
                                    };
        return _spawnPositions;
    }

    public void OnUseAxe()
    {
        if (_state.Value == StumpStates.LogOn)
            _state.Value = StumpStates.Splitting;
    }

    public void LoadLog()
    {
        if (_inventory.TryRemoveItem("DryLog", 1))
            _state.Value = StumpStates.LogOn;
    }

    private IEnumerator WaitForAnimationThenSpawnFirewood()
    {
        // Let animation play, and add split wood sound effext in the middle.
        float _animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(_animationLength - 0.2f);
        AudioManager.Instance.PlaySFX(_fallingSplitWoodSFX);
        yield return new WaitForSeconds(_animationLength - (_animationLength - 0.2f));

        // spawn firewood
        Vector3[] _spawnPositions = GetFirewoodSpawnPositions();
        SpawnItems.SpawnLooseItems("Firewood", _spawnPositions, false, 0, 0);

        // stop animator
        _animator.StopPlayback();
        _animator.enabled = false;
        _state.Value = StumpStates.Idle;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        switch (_state.Value)
        {
            case StumpStates.AxeIn:
                if (_inventory.TryAddItem("Axe", 1))
                {
                    _state.Value = StumpStates.Idle;
                    return true;
                }
                else
                    return false;
            case StumpStates.LogOn:
                if (_inventory.TryAddItem("DryLog", 1))
                {
                    _state.Value = StumpStates.Idle;
                    return true;
                }
                else
                    return false;
            default:
                return false;
        }
    }

    public SaveData Save()
    {
        var _extendedData = new StumpSaveData()
        {
            State = _state.Value,
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<StumpSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<StumpSaveData>();
        _state.Value = _extendedData.State;
    }
}