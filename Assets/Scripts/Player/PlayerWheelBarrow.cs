using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using ReactiveUnity;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWheelBarrow : MonoBehaviour, ISaveable
{
    public string SaveableId => "PlayerWheelBarrow";

    public Reactive<bool> IsHoldingWheelBarrow = new Reactive<bool>(false);

    [SerializeField] private WeightyObjectStack _wheelbarrowStack = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;
    [SerializeField] private GameObject _playerWheelBarrow;
    [SerializeField] private GameObject _staticWheelBarrowPrefab;
    [SerializeField] private SoundData _liftBarrowSound;
    [SerializeField] private SoundData _placeBarrowSound;
    List<Action> _unsubscribeHooks = new();
    private Rigidbody2D _rb;
    private bool _suppressLiftSound;

    public WeightyObjectStack WheelBarrowStack => _wheelbarrowStack;

    void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _unsubscribeHooks.Add(IsHoldingWheelBarrow.OnChange(curr => OnWheelBarrowingChange(curr)));
    }

    public void PutDownWheelBarrow()
    {
        if (PlayerStateController.Instance.PlayerState.Value != PlayerStates.Barrowing)
            return;

        IsHoldingWheelBarrow.Value = false;
        PlayerStateController.Instance.TransitionTo(PlayerStates.Default);
    }

    private void OnWheelBarrowingChange(bool isWheelBarrowing)
    {
        if (isWheelBarrowing)
        {
            if (!_suppressLiftSound)
                PlayerAudioManager.Instance.PlayOneShot(_liftBarrowSound);
        }
        else
        {
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

    public string CaptureState()
    {
        var _data = new StackContentsSaveData
        {
            Holding = IsHoldingWheelBarrow.Value,
            Items = StoredWeightyObjectSaveData.CaptureAll(_wheelbarrowStack.StoredObjects)
        };
        return JsonConvert.SerializeObject(_data);
    }

    public void RestoreState(string json)
    {
        var _data = JsonConvert.DeserializeObject<StackContentsSaveData>(json);
        if (_data?.Items == null) return;

        _wheelbarrowStack.Clear();
        foreach (var _saveData in _data.Items)
        {
            var _storedObject = _saveData.Restore();
            if (_storedObject != null)
                _wheelbarrowStack.Push(_storedObject);
        }

        _suppressLiftSound = true;
        IsHoldingWheelBarrow.Value = _data.Holding;
        PlayerStateController.Instance.TransitionTo(_data.Holding ? PlayerStates.Barrowing : PlayerStates.Default);
        _suppressLiftSound = false;
    }

    public void ResetState()
    {
        _wheelbarrowStack.Clear();
        IsHoldingWheelBarrow.Value = false;
        PlayerStateController.Instance.TransitionTo(PlayerStates.Default);
    }
}
