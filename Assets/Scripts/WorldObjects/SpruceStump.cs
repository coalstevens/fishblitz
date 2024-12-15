using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class SpruceStump : MonoBehaviour, SceneSaveLoadManager.ISaveable
{
    private const string IDENTIFIER = "SpruceStump";
    private enum StumpStates { Idle };
    private class StumpSaveData
    {
        public StumpStates State;
    }

    [SerializeField] private Sprite _idle;
    [SerializeField] private Reactive<StumpStates> _state = new Reactive<StumpStates>(StumpStates.Idle);
    private SpriteRenderer _spriteRenderer;
    private List<Action> _unsubscribeHooks = new();

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
            case StumpStates.Idle:
                _spriteRenderer.sprite = _idle;
                return;
            default:
                Debug.LogError("SpruceStump state machine defaulted.");
                break;
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
