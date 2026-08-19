using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReactiveUnity;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FirewoodPile : MonoBehaviour, InteractInput.IInteractable, ISceneSaveable
{
    private class LogPileSaveData
    {
        public int NumLogs;
    }

    private const string IDENTIFIER = "FirewoodPile";
    [SerializeField] private Reactive<int> _numLogs = new Reactive<int>(4);
    [SerializeField] private Inventory _inventory;
    [SerializeField] private List<Sprite> _sprites = new();
    [SerializeField] private Inventory.Item _firewood;
    private SpriteRenderer _spriteRenderer;
    private List<Action> _unsubscribeCBs = new();

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateRackSprite();
    }

    private void OnEnable()
    {
        _unsubscribeCBs.Add(_numLogs.OnChange((prev, curr) => UpdateRackSprite()));
    }

    private void OnDisable()
    {
        foreach (var _cb in _unsubscribeCBs)
            _cb();
    }

    private void UpdateRackSprite()
    {
        if (_numLogs.Value == 0)
            return;
        _spriteRenderer.sprite = _sprites[_numLogs.Value - 1];
    }

    public void RemoveLog()
    {
        if (_numLogs.Value == 0)
            return;

        if (_inventory.TryAddItem(_firewood, 1))
        {
            _numLogs.Value--;
            if (_numLogs.Value == 0)
                Destroy(gameObject);
        }
        else
        {
            PlayerDialogue.Instance.PostMessage("I'm all full up");
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        RemoveLog();
        return true;
    }

    private string _persistentID;
    public string PrefabId => IDENTIFIER;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var _extendedData = new LogPileSaveData()
        {
            NumLogs = _numLogs.Value,
        };
        return JsonConvert.SerializeObject(_extendedData);
    }

    public void RestoreState(string json)
    {
        var _extendedData = JsonConvert.DeserializeObject<LogPileSaveData>(json);
        _numLogs.Value = _extendedData.NumLogs;
    }

    public void ResetState() { }
}
