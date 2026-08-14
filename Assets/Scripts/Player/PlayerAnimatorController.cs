using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private static PlayerAnimatorController _instance;
    public static PlayerAnimatorController Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("This object does not exist");
            return _instance;
        }
    }
    private Animator _defaultAnimator;
    private Animator _barrowAnimator;
    private Animator _carryAnimator;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private GameObject _DefaultSprite;
    [SerializeField] private GameObject _BarrowingSprite;
    [SerializeField] private GameObject _CarryingSprite;
    private PlayerMovement _playerMovementController;
    private PlayerWheelBarrow _wheelBarrow;
    private PlayerCarry _playerCarry;
    private WeightyObjectStack _carriedObjects;
    private List<Action> _unsubscribeHooks = new();
    public CompassDirection AnimationDirection = CompassDirection.SouthEast;

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        _defaultAnimator = _DefaultSprite.GetComponent<Animator>();
        _barrowAnimator = _BarrowingSprite.GetComponent<Animator>();
        _carryAnimator = _CarryingSprite.GetComponent<Animator>();
        _playerMovementController = GetComponentInParent<PlayerMovement>();
        _wheelBarrow = GetComponentInParent<PlayerWheelBarrow>();
        _playerCarry = GetComponentInParent<PlayerCarry>();
        _carriedObjects = _playerCarry.CarriedStack;

        _unsubscribeHooks.Add(_playerMovementController.PlayerState.OnChange(curr => OnStateChange(curr)));
        _unsubscribeHooks.Add(_carriedObjects.StoredObjects.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_playerMovementController.Direction.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_wheelBarrow.IsHoldingWheelBarrow.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_playerCarry.IsCarrying.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_inventory.ActiveItemSlot.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    public GameObject ActiveSprite =>
        _BarrowingSprite.activeSelf ? _BarrowingSprite :
        _CarryingSprite.activeSelf ? _CarryingSprite :
        _DefaultSprite;

    private Animator CurrentAnimator =>
        _BarrowingSprite.activeSelf ? _barrowAnimator :
        _CarryingSprite.activeSelf ? _carryAnimator :
        _defaultAnimator;

    private void ShowDefaultSprite()
    {
        _DefaultSprite.SetActive(true);
        _BarrowingSprite.SetActive(false);
        _CarryingSprite.SetActive(false);
    }

    private void ShowBarrowingSprite()
    {
        _DefaultSprite.SetActive(false);
        _BarrowingSprite.SetActive(true);
        _CarryingSprite.SetActive(false);
    }

    private void ShowCarryingSprite()
    {
        _DefaultSprite.SetActive(false);
        _BarrowingSprite.SetActive(false);
        _CarryingSprite.SetActive(true);
    }

    private void OnStateChange(PlayerMovement.PlayerStates curr)
    {
        CompassDirection playerDirection = _playerMovementController.Direction.Value;
        CompassDirection[] faceSouthEast = {CompassDirection.East, CompassDirection.NorthEast, CompassDirection.SouthEast};
        CompassDirection[] faceSouthWest = {CompassDirection.West, CompassDirection.NorthWest, CompassDirection.SouthWest};

        if (faceSouthEast.Contains(playerDirection))
        {
            AnimationDirection = CompassDirection.SouthEast;
            transform.localScale = new Vector2(1, 1);
            _BarrowingSprite.transform.localScale = Vector2.one;
            _CarryingSprite.transform.localScale = Vector2.one;
        }
        else if (faceSouthWest.Contains(playerDirection))
        {
            AnimationDirection = CompassDirection.SouthWest;
            transform.localScale = new Vector2(-1, 1);
        }

        switch (curr)
        {
            case PlayerMovement.PlayerStates.Idle:
                HandleIdle();
                break;
            case PlayerMovement.PlayerStates.Running:
                HandleRunning();
                break;
            case PlayerMovement.PlayerStates.Birding:
                PlayIdleBinos();
                break;
            case PlayerMovement.PlayerStates.BirdingRunning:
                PlayRunningBinos();
                break;
            case PlayerMovement.PlayerStates.Fishing:
                HandleFishing();
                break;
            case PlayerMovement.PlayerStates.Catching:
                HandleCatching();
                break;
            case PlayerMovement.PlayerStates.Axing:
                HandleChopping();
                break;
            case PlayerMovement.PlayerStates.Celebrating:
                HandleCelebrating();
                break;
            case PlayerMovement.PlayerStates.PickingUp:
                HandlePickingUp();
                break;
            case PlayerMovement.PlayerStates.Crouched:
                HandleCrouched();
                break;
            case PlayerMovement.PlayerStates.BowCharging:
                PlayIdleBow();
                break;
            case PlayerMovement.PlayerStates.BowChargingRunning:
                PlayRunningBow();
                break;
        }
    }

    private void HandleRunning()
    {
        if (_wheelBarrow.IsHoldingWheelBarrow.Value)
        {
            ShowBarrowingSprite();
            PlayRunningBarrow();
            return;
        }

        if (_playerCarry.IsCarrying.Value)
        {
            ShowCarryingSprite();
            PlayRunningCarry();
            return;
        }

        ShowDefaultSprite();

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            PlayRunning();
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                PlayRunningAxe();
                break;
            default:
                PlayRunning();
                break;
        }
    }

    private void PlayRunning()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Running");
                break;
        }
    }

    private void HandleIdle()
    {
        if (_wheelBarrow.IsHoldingWheelBarrow.Value)
        {
            ShowBarrowingSprite();
            HandleBarrowIdle();
            return;
        }

        if (_playerCarry.IsCarrying.Value)
        {
            ShowCarryingSprite();
            HandleCarryIdle();
            return;
        }

        ShowDefaultSprite();

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            PlayIdle();
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                PlayIdleAxe();
                break;
            default:
                PlayIdle();
                break;
        }
    }

    private void PlayIdle()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Idle");
                break;
        }
    }

    private void PlayIdleAxe()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Idle_Axe");
                break;
        }
    }

    private void PlayRunningAxe()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Running_Axe");
                break;
        }
    }

    private void PlayRunningBinos()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Running_Binos");
                break;
        }
    }

    private void PlayIdleBinos()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Idle_Binos");
                break;
        }
    }

    private void PlayRunningBarrow()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("SE_Running_Barrow");
                break;
        }
    }

    private void HandleCarryIdle()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthWest:
            case CompassDirection.SouthEast:
                CurrentAnimator.Play("SE_Idle_Carry");
                break;
        }
    }

    private void PlayRunningCarry()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthWest:
            case CompassDirection.SouthEast:
                CurrentAnimator.Play("SE_Running_Carry");
                break;
        }
    }

    private void SetPlayerIdle()
    {
        _playerMovementController.PlayerState.Value = PlayerMovement.PlayerStates.Idle;
    }

    private void HandleBarrowIdle()
    {
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("SE_Idle_Barrow");
                break;
        }
    }

    private void HandleCatching()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Catch");
                break;
        }
    }
    
    private void HandleCrouched()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Crouch");
                break;
        }
    }

    private void HandleCelebrating()
    {
        ShowDefaultSprite();
        CurrentAnimator.Play("Caught");
        Invoke(nameof(SetPlayerIdle), 1.5f);
    }

    private void HandlePickingUp()
    {
        ShowCarryingSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("SE_Pickup");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.06f*8); // duration is half the pickup animation
    }

    private void HandleFishing()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Fishing");
                break;
        }
    }

    private void HandleChopping()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("S_Chopping");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.610f);
    }

    private void PlayIdleBow()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("SE_Idle_Bow_Charging");
                break;
        }
    }

    private void PlayRunningBow()
    {
        ShowDefaultSprite();
        switch (AnimationDirection)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                CurrentAnimator.Play("SE_Running_Bow_Charging");
                break;
        }
    }
}
