using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReactiveUnity;
using UnityEngine;

public class SpruceStump : MonoBehaviour, ISceneSaveable
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

    private string _persistentID;
    public string PrefabId => IDENTIFIER;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var _extendedData = new StumpSaveData()
        {
            State = _state.Value,
        };
        return JsonConvert.SerializeObject(_extendedData);
    }

    public void RestoreState(string json)
    {
        var _extendedData = JsonConvert.DeserializeObject<StumpSaveData>(json);
        _state.Value = _extendedData.State;
    }

    public void ResetState() { }
}
