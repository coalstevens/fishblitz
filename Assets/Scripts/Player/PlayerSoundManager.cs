using System;
using UnityEngine;

[RequireComponent(typeof(PlayerWheelBarrow), typeof(PlayerMovement))]
public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] private SoundData _walkingSound;
    [SerializeField] private SoundData _wheelbarrowRollingSound;
    private PlayerWheelBarrow _wheelBarrow;
    private PlayerMovement _playerMovementController;
    private Action _stopFootstepSoundCB;
    private Action _stopWheelbarrowSoundCB;
    private Action _unsubscribeStateCB;
    private Action _unsubscribeWheelbarrowCB;

    private void OnEnable()
    {
        _wheelBarrow = GetComponent<PlayerWheelBarrow>();
        _playerMovementController = GetComponent<PlayerMovement>();
        _unsubscribeStateCB = _playerMovementController.PlayerState.OnChange((prev, curr) => OnPlayerStateChange(prev, curr));
        _unsubscribeWheelbarrowCB = _wheelBarrow.IsHoldingWheelBarrow.OnChange(_ => OnWheelbarrowStateChange());
    }

    private void OnDisable()
    {
        StopFootstepSound();
        StopWheelbarrowSound();
        _unsubscribeStateCB();
        _unsubscribeWheelbarrowCB();
    }

    private void OnPlayerStateChange(PlayerMovement.PlayerStates previous, PlayerMovement.PlayerStates current)
    {
        switch (current)
        {
            case PlayerMovement.PlayerStates.Running:
                _stopFootstepSoundCB = PlayerAudioManager.Instance.PlayLooping(_walkingSound);
                break;
            default:
                StopFootstepSound();
                break;
        }
        OnWheelbarrowStateChange();
    }

    private void OnWheelbarrowStateChange()
    {
        bool shouldRoll = _playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.Running
                       && _wheelBarrow.IsHoldingWheelBarrow.Value;

        if (shouldRoll && _stopWheelbarrowSoundCB == null)
            _stopWheelbarrowSoundCB = PlayerAudioManager.Instance.PlayLooping(_wheelbarrowRollingSound);
        else if (!shouldRoll)
            StopWheelbarrowSound();
    }

    private void StopFootstepSound()
    {
        if (_stopFootstepSoundCB != null)
        {
            _stopFootstepSoundCB();
            _stopFootstepSoundCB = null;
        }
    }

    private void StopWheelbarrowSound()
    {
        if (_stopWheelbarrowSoundCB != null)
        {
            _stopWheelbarrowSoundCB();
            _stopWheelbarrowSoundCB = null;
        }
    }
}
