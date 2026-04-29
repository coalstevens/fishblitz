using System;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] private SoundData _walkingSound;
    [SerializeField] private AudioSource _audioSource;
    private PlayerMovementController _playerMovementController;
    private Action _stopSoundCB;
    private Action _unsubscribeCB;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _unsubscribeCB = _playerMovementController.PlayerState.OnChange((prev, curr) => OnPlayerStateChange(prev, curr));
    }

    private void OnDisable()
    {
        StopSound();
        _unsubscribeCB();
    }

    private void OnPlayerStateChange(PlayerMovementController.PlayerStates previous, PlayerMovementController.PlayerStates current)
    {
        switch (current)
        {
            case PlayerMovementController.PlayerStates.Running:
                _stopSoundCB = AudioManager.PlayLoopingSFX(_audioSource, _walkingSound);
                break;
            default:
                StopSound();
                break;
        }
    }

    private void StopSound()
    {
        if (_stopSoundCB != null)
        {
            _stopSoundCB();
            _stopSoundCB = null;
        }
    }
}
