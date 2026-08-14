using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ReactiveUnity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWheelBarrow : MonoBehaviour
{
    public Reactive<bool> IsHoldingWheelBarrow = new Reactive<bool>(false);

    [SerializeField] private WeightyObjectStack _wheelbarrowStack = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;
    [SerializeField] private GameObject _playerWheelBarrow;
    [SerializeField] private GameObject _staticWheelBarrowPrefab;
    [SerializeField] private SoundData _liftBarrowSound;
    [SerializeField] private SoundData _placeBarrowSound;
    List<Action> _unsubscribeHooks = new();
    private PlayerInput _playerInput;
    private Rigidbody2D _rb;

    public WeightyObjectStack WheelBarrowStack => _wheelbarrowStack;

    void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _unsubscribeHooks.Add(IsHoldingWheelBarrow.OnChange(curr => OnWheelBarrowingChange(curr)));
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.TryGetComponent(out _playerInput);
    }

    private void OnUseWheelBarrow()
    {
    }

    private void OnReleaseWheelBarrow()
    {
        IsHoldingWheelBarrow.Value = false;
    }

    private void OnWheelBarrowingChange(bool isWheelBarrowing)
    {
        if (isWheelBarrowing)
        {
            Debug.Log($"Player position change {Time.frameCount}");
            _playerInput?.SwitchCurrentActionMap("PlayerBarrowing");
            PlayerAudioManager.Instance.PlayOneShot(_liftBarrowSound);
        }
        else
        {
            _playerInput?.SwitchCurrentActionMap("Player");
            StaticWheelBarrowSelector newBarrow = InstantiateStaticWheelBarrow();
            newBarrow.SetFacingDirection(PlayerAnimatorController.Instance.AnimationDirection);
            TransferContentsToStaticWheelbarrow(newBarrow);
            PlayerAudioManager.Instance.PlayOneShot(_placeBarrowSound);
        }
    }

    public void PickUpStaticWheelbarrow(WeightyObjectStack sourceStack)
    {
        while (!sourceStack.IsEmpty())
        {
            _wheelbarrowStack.Push(sourceStack.Pop());
            if (_stackConfig != null && _stackConfig.InsertSound != null)
                PlayerAudioManager.Instance.PlayOneShot(_stackConfig.InsertSound);
        }
    }

    private void TransferContentsToStaticWheelbarrow(StaticWheelBarrowSelector staticBarrow)
    {
        var targetStack = staticBarrow.GetComponent<StaticWheelBarrow>()?.WeightyStack;
        if (targetStack == null) return;
        while (!_wheelbarrowStack.IsEmpty())
        {
            targetStack.Push(_wheelbarrowStack.Pop());
            if (_stackConfig != null && _stackConfig.InsertSound != null)
                PlayerAudioManager.Instance.PlayOneShot(_stackConfig.InsertSound);
        }
    }

    private StaticWheelBarrowSelector InstantiateStaticWheelBarrow()
    {
        if (_staticWheelBarrowPrefab != null)
        {
            Transform impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
            GameObject newStaticWheelBarrow = Instantiate(
                _staticWheelBarrowPrefab, 
                _playerWheelBarrow.transform.position,
                quaternion.identity, 
                impermanentContainer);
            StaticWheelBarrowSelector newBarrow = newStaticWheelBarrow.GetComponent<StaticWheelBarrowSelector>();
            if (_playerWheelBarrow == null)
                Debug.LogError("Static wheel barrow is missing its wheelbarrow component");
            return newBarrow;
        }
        else
        {
            Debug.LogError("StaticWheelBarrow prefab not set in inspector.");
            return null;
        }
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }
}
