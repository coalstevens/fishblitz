using System;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] AudioClip _walkingSFX;
    [SerializeField] float _walkingSFXVolume = 0.25f;
    private PlayerMovementController _playerMovementController;
    private Action _stopSoundCB;
    private Action _unsubscribeCB;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _unsubscribeCB = _playerMovementController.PlayerState.OnChange((prev,curr) => OnPlayerStateChange(prev,curr));
    }

    private void OnDisable() {
        if (_stopSoundCB != null) {
            _stopSoundCB();
            _stopSoundCB = null;
        }
        _unsubscribeCB();
    }

    private void OnPlayerStateChange(PlayerMovementController.PlayerStates previous, PlayerMovementController.PlayerStates current)
    {
        if (_stopSoundCB != null) {
            _stopSoundCB();
            _stopSoundCB = null;
        }

        switch (current) {
            case PlayerMovementController.PlayerStates.Walking:
                _stopSoundCB = AudioManager.Instance.PlayLoopingSFX(_walkingSFX, _walkingSFXVolume);
                break;
        }
    }
}
