using System.Collections;
using ReactiveUnity;
using UnityEngine;

public class Stump : MonoBehaviour, IInteractable, IWorldObject
{
    Animator _animator;
    private enum StumpStates{Default, AxeIn, LogOn, SplittingLog};
    private Reactive<StumpStates> _stumpState = new Reactive<StumpStates>(StumpStates.AxeIn);
    private Inventory _inventory;
    private const string IDENTIFIER = "Stump";

    public Collider2D ObjCollider {
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null) {
                return _collider;
            }
            else {
                Debug.LogError("Stump does not have a collider component");
                return null;
            }
        }
    }

    public string Identifier {
        get {
            return IDENTIFIER;
        }
    }

    public int State {
        get => (int) _stumpState.Value; 
        set => _stumpState.Value = (StumpStates) value;
    }
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _stumpState.OnChange((curr,prev) => OnStateChange());
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        OnStateChange();
    }

    void OnStateChange()
    {
        switch (_stumpState.Value)
        {
            case StumpStates.Default:
                _animator.Play("Default");
                break;
            case StumpStates.AxeIn:
                _animator.Play("AxeIn");
                break;
            case StumpStates.LogOn:
                _animator.Play("LogOn");
                break;
            case StumpStates.SplittingLog:
                _animator.Play("SplittingLog");
                StartCoroutine(WaitForAnimationToEnd());
                _inventory.TryAddItem("Firewood", 3);
                break;
        } 
    }

    public void SplitLog() {
        if (_stumpState.Value == StumpStates.LogOn) {
            _stumpState.Value = StumpStates.SplittingLog;
        }
    }

    public void LoadLog() {
        _inventory.TryRemoveItem("DryLog", 1);
        _stumpState.Value = StumpStates.LogOn;
    }

    private IEnumerator WaitForAnimationToEnd()
    {
        // lazy solution. 1.410s is the length of the splitting animation
        yield return new WaitForSeconds(1.410f);
        _animator.StopPlayback();
        _stumpState.Value = StumpStates.Default;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        switch (_stumpState.Value) { 
            case StumpStates.AxeIn:
                _inventory.TryAddItem("Axe", 1);
                _stumpState.Value = StumpStates.Default;
                return true;
            case StumpStates.LogOn:
                _inventory.TryAddItem("DryLog", 1);
                _stumpState.Value = StumpStates.Default;
                return true;
            default: 
                return false;
        }
    }
}
