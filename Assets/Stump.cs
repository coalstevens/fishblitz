using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Stump : MonoBehaviour, IInteractableWorldObject
{
    Animator _animator;
    private enum StumpStates{Default, AxeIn, LogOn, SplittingLog};
    private Reactive<StumpStates> _stumpState = new Reactive<StumpStates>(StumpStates.AxeIn);
    private Inventory _inventory;
    private Coroutine _waitForAnimationCoroutine;
    
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
                _inventory.AddItem("Firewood", 3);
                break;
        } 
    }
    public bool CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        switch (_stumpState.Value) { 
            case StumpStates.AxeIn:
                _inventory.AddItem("Axe", 1);
                _stumpState.Value = StumpStates.Default;
                return true;
            case StumpStates.LogOn:
                _inventory.AddItem("Log", 1);
                _stumpState.Value = StumpStates.Default;
                return true;
            default: 
                return false;
        }
    }

    public void SplitLog() {
        if (_stumpState.Value == StumpStates.LogOn) {
            _stumpState.Value = StumpStates.SplittingLog;
        }
    }

    public void LoadLog() {
        _inventory.RemoveItem("Log", 1);
        _stumpState.Value = StumpStates.LogOn;
    }

    private IEnumerator WaitForAnimationToEnd()
    {
        // lazy solution. 1.410s is the length of the splitting animation
        yield return new WaitForSeconds(1.410f);
        _animator.StopPlayback();
        _stumpState.Value = StumpStates.Default;
    }
}
