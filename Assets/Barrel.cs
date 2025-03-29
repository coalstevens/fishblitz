using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class Barrel : MonoBehaviour, SaveData.ISaveable
{
    private enum States { Normal, WithBinos }
    private class BarrelSaveData
    {
        public States BarrelState;
    }
    [SerializeField] private string _identifier = "Barrel";
    [SerializeField] private Sprite _barrel;
    [SerializeField] private Sprite _barrelWithBinos;
    private SpriteRenderer _renderer;
    private Reactive<States> _state = new Reactive<States>(States.WithBinos);
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
            case States.WithBinos:
                _renderer.sprite = _barrelWithBinos;
                break;
            case States.Normal:
                Transform child = transform.GetChild(0);
                if (child != null)
                    Destroy(child.gameObject);
                _renderer.sprite = _barrel;
                break;
            default:
                Debug.LogError("Unhandled state in Barrel");
                break;
        }
    }

    public void RemoveBinoculars()
    {
        _state.Value = States.Normal;
    }

    public SaveData Save()
    {
        var _extendedData = new BarrelSaveData()
        {
            BarrelState = _state.Value
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(_identifier);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<BarrelSaveData>(_extendedData);

        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<BarrelSaveData>();
        _state.Value = _extendedData.BarrelState;
    }
}
