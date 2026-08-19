using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReactiveUnity;
using UnityEngine;

public class BeachedBoat : MonoBehaviour, ISceneSaveable
{
    private enum States { DamagedWithHammer, Damaged, Normal }
    private class BoatSaveData
    {
        public States BoatState;
    }
    [SerializeField] private string _identifier = "BeachedBoat";
    [SerializeField] private Sprite _boat;
    [SerializeField] private Sprite _damagedBoat;
    [SerializeField] private Sprite _damagedBoatWithHammer;
    private SpriteRenderer _renderer;
    private Reactive<States> _state = new Reactive<States>(States.DamagedWithHammer);
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _unsubscribeHooks.Add(_state.OnChange(curr => OnStateChange(curr)));
        OnStateChange(_state.Value);
    }

    private void OnDisable()
    {
        foreach(var hook in _unsubscribeHooks) 
            hook();        
        _unsubscribeHooks.Clear();
    }

    private void OnStateChange(States curr)
    {
        switch(curr) 
        {
            case States.Damaged:
                Transform child = transform.GetChild(0);
                if (child != null)
                    Destroy(child.gameObject);
                _renderer.sprite = _damagedBoat;
                break;
            case States.DamagedWithHammer:
                _renderer.sprite = _damagedBoatWithHammer;
                break;
            case States.Normal:
                _renderer.sprite = _boat;
                break;
            default:
                Debug.LogError("Unhandled state in BeachedBoat");
                break;
        }
    }

    public void RemoveHammer()
    {
        _state.Value = States.Damaged;
    }

    private string _persistentID;
    public string PrefabId => _identifier;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var _extendedData = new BoatSaveData()
        {
            BoatState = _state.Value
        };
        return JsonConvert.SerializeObject(_extendedData);
    }

    public void RestoreState(string json)
    {
        var _extendedData = JsonConvert.DeserializeObject<BoatSaveData>(json);
        _state.Value = _extendedData.BoatState;
    }

    public void ResetState() { }
}
