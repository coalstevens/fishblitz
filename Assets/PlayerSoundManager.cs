using System;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] AudioClip _walkingSFX;
    private PlayerMovementController _playerMovementController;
    private Action _stopSoundCB;
    private Action _unsubscribeCB;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _unsubscribeCB = _playerMovementController.PlayerState.OnChange((prev,curr) => OnPlayerStateChange(prev,curr));
    }

    private void OnDisable() {
        _unsubscribeCB();
    }

    private void OnPlayerStateChange(PlayerStates previous, PlayerStates current)
    {
        if (_stopSoundCB != null) {
            _stopSoundCB();
            _stopSoundCB = null;
        }

        switch (current) {
            case PlayerStates.Walking:
                _stopSoundCB = AudioManager.Instance.PlayLoopingSFX(_walkingSFX, 0.5f);
                break;
        }
    }
}
