using System;
using UnityEngine;

[RequireComponent(typeof(PlayerWheelBarrow))]
public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] private SoundData _walkingSound;
    [SerializeField] private SoundData _wheelbarrowRollingSound;
    private PlayerWheelBarrow _wheelBarrow;
    private Action _stopFootstepSoundCB;
    private Action _stopWheelbarrowSoundCB;
    private Action _unsubscribeStateCB;
    private Action _unsubscribeRunningCB;
    private Action _unsubscribeWheelbarrowCB;

    private void OnEnable()
    {
        _wheelBarrow = GetComponent<PlayerWheelBarrow>();
        _unsubscribeStateCB = PlayerStateController.Instance.PlayerState.OnChange(_ => RefreshSounds());
        _unsubscribeRunningCB = PlayerStateController.Instance.IsRunning.OnChange(_ => RefreshSounds());
        _unsubscribeWheelbarrowCB = _wheelBarrow.IsHoldingWheelBarrow.OnChange(_ => RefreshSounds());
    }

    private void OnDisable()
    {
        StopFootstepSound();
        StopWheelbarrowSound();
        _unsubscribeStateCB();
        _unsubscribeRunningCB();
        _unsubscribeWheelbarrowCB();
    }

    private void RefreshSounds()
    {
        bool isRunning = PlayerStateController.Instance.IsRunning.Value
                      && PlayerStateController.Instance.PlayerState.Value.CanMove();
        bool shouldRoll = PlayerStateController.Instance.PlayerState.Value == PlayerStates.Barrowing
                       && PlayerStateController.Instance.IsRunning.Value
                       && _wheelBarrow.IsHoldingWheelBarrow.Value;

        if (isRunning && _stopFootstepSoundCB == null)
            _stopFootstepSoundCB = PlayerAudioManager.Instance.PlayLooping(_walkingSound);
        else if (!isRunning)
            StopFootstepSound();

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
