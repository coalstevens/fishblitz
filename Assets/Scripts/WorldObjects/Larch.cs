using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
public enum ExtendedStates{AxeIn = 4, LogOn, Splitting, BaseState};

public class LarchSaveData : WorldObjectSaveData {
    public ExtendedStates ExtendedState;
    public TreeStates TreeState;
}

// Note: If you switch from basestate.stump to an extendedstate, then back to basestate.stump
// you can't trigger the transition by setting basestate as OnChange won't be called
// as basestate was always basestate.stump
public class Larch : TreePlant, IInteractable, IUseableWithAxe, ISaveable<LarchSaveData>
{
    private const string IDENTIFIER = "Larch";
    
    [Header("Extended State")]
    [SerializeField] private Sprite _axeIn;
    [SerializeField] private Sprite _logOn;
    [SerializeField] private Reactive<ExtendedStates> _extendedState = new Reactive<ExtendedStates>(ExtendedStates.BaseState);
    private Inventory _inventory;
    Animator _animator;
    List<Action> _unsubscribeHooks = new();

    protected override void Start() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _animator.enabled = false;
        OnExtendedStateChange();
    }

    protected override void OnEnable() {
        _unsubscribeHooks.Add(_treeState.OnChange((curr,prev) => _extendedState.Value = ExtendedStates.BaseState));
        _unsubscribeHooks.Add(_treeState.OnChange((curr,prev) => OnExtendedStateChange()));
        _unsubscribeHooks.Add(_extendedState.OnChange((curr,prev) => OnExtendedStateChange()));
    }

    protected override void OnDisable() {
        foreach (var _hook in _unsubscribeHooks)
            _hook();
    }

    void OnExtendedStateChange() {
        switch (_extendedState.Value) {
            case ExtendedStates.AxeIn:
                _spriteRenderer.material = _standardLit;
                _spriteRenderer.sprite = _axeIn;
                return;
            case ExtendedStates.LogOn:
                _spriteRenderer.sprite = _logOn;
                return;
            case ExtendedStates.Splitting:
                _animator.enabled = true;
                _animator.Play("Splitting");
                StartCoroutine(WaitForAnimationToEnd());
                _inventory.TryAddItem("Firewood", 3);
                return;
            default:
                // Extended case null. Check base states
                OnStateChange();
            break;
        }
    }

    public void OnUseAxe() {
        if (_extendedState.Value == ExtendedStates.LogOn)
            _extendedState.Value = ExtendedStates.Splitting;
    }

    public void LoadLog() {
        if (_inventory.TryRemoveItem("DryLog", 1))
            _extendedState.Value = ExtendedStates.LogOn;
    }

    private IEnumerator WaitForAnimationToEnd()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _animator.StopPlayback();
        _animator.enabled = false;
        _extendedState.Value = ExtendedStates.BaseState;
    }

    public bool CursorInteract(Vector3 cursorLocation) {
        switch (_extendedState.Value) { 
            case ExtendedStates.AxeIn:
                if (_inventory.TryAddItem("Axe", 1)) {
                    _treeState.Value = TreeStates.Stump;
                    return true;
                }
                else
                    return false;
            case ExtendedStates.LogOn:
                if (_inventory.TryAddItem("DryLog", 1)) {
                    _treeState.Value = TreeStates.Stump;
                    return true;
                }
                else
                    return false;
            default: 
                return false;
        }
    }

    public LarchSaveData Save()
    {
        return new LarchSaveData() {
            Identifier = IDENTIFIER,
            Position = new SimpleVector3(transform.position),
            ExtendedState = _extendedState.Value,
            TreeState = _treeState.Value,
        };
    }

    public void Load(LarchSaveData saveData)
    {
        OnDisable();
        _treeState.Value = saveData.TreeState;
        _extendedState.Value = saveData.ExtendedState;
        OnEnable();
        OnExtendedStateChange();
    }
}
