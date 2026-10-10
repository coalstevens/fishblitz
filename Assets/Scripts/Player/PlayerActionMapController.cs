using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput), typeof(PlayerWheelBarrow))]
public class PlayerActionMapController : MonoBehaviour
{
    // Overlay names intentionally equal their action-map names in fish blitz.inputactions.
    public enum Overlay { Combat, Birding, Menu }

    private const string PlayerMap = "Player";
    private const string BarrowMap = "PlayerBarrowing";

    private static PlayerActionMapController _instance;
    public static PlayerActionMapController Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("PlayerActionMapController does not exist");
            return _instance;
        }
    }

    private PlayerInput _playerInput;
    private PlayerWheelBarrow _wheelBarrow;
    private readonly List<Overlay> _overlays = new();
    private Action _unsubscribeHolding;

    private string BaseMap => _wheelBarrow.IsHoldingWheelBarrow.Value ? BarrowMap : PlayerMap;
    private string DesiredMap => _overlays.Count > 0 ? _overlays[_overlays.Count - 1].ToString() : BaseMap;

    private void Awake()
    {
        _instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _wheelBarrow = GetComponent<PlayerWheelBarrow>();
        Assert.IsNotNull(_playerInput);
        Assert.IsNotNull(_wheelBarrow);
    }

    private void OnEnable()
    {
        _unsubscribeHolding = _wheelBarrow.IsHoldingWheelBarrow.OnChange(_ => Refresh());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        _unsubscribeHolding?.Invoke();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PushOverlay(Overlay overlay)
    {
        if (_overlays.Contains(overlay)) return;
        _overlays.Add(overlay);
        Refresh();
    }

    public void PopOverlay(Overlay overlay)
    {
        if (!_overlays.Remove(overlay)) return;
        Refresh();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _overlays.Clear();
        Refresh();
    }

    private void Refresh()
    {
        string map = DesiredMap;
        if (_playerInput.currentActionMap?.name != map)
            _playerInput.SwitchCurrentActionMap(map);
    }
}
