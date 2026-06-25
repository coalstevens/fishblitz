using System;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] private SoundData _walkingSound;
    [SerializeField] private SoundData _wheelbarrowRollingSound;
    [SerializeField] private PlayerData _playerData;
    private PlayerMovementController _playerMovementController;
    private Action _stopFootstepSoundCB;
    private Action _stopWheelbarrowSoundCB;
    private Action _unsubscribeStateCB;
    private Action _unsubscribeWheelbarrowCB;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _unsubscribeStateCB = _playerMovementController.PlayerState.OnChange((prev, curr) => OnPlayerStateChange(prev, curr));
        _unsubscribeWheelbarrowCB = _playerData.IsHoldingWheelBarrow.OnChange(_ => OnWheelbarrowStateChange());
    }

    private void OnDisable()
    {
        StopFootstepSound();
        StopWheelbarrowSound();
        _unsubscribeStateCB();
        _unsubscribeWheelbarrowCB();
    }

    private void OnPlayerStateChange(PlayerMovementController.PlayerStates previous, PlayerMovementController.PlayerStates current)
    {
        switch (current)
        {
            case PlayerMovementController.PlayerStates.Running:
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
        bool shouldRoll = _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Running
                       && _playerData.IsHoldingWheelBarrow.Value;

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
