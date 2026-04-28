using System;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField] AudioClip _walkingSFX;
    [SerializeField] float _walkingSFXVolume = 0.25f;
    [Header("Variation Settings")]
    [SerializeField] private float _pitchVariation = 0.005f;
    [SerializeField] private float _volumeVariation = 0.02f;
    [SerializeField] private float _startTimeVariation = 0f;
    [SerializeField] private float _loopSpacing = 0f;
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
                _stopSoundCB = AudioManager.Instance.PlayLoopingSFXWithVariation(
                    _walkingSFX, 
                    _walkingSFXVolume, 
                    pitchVariation: _pitchVariation, 
                    volumeVariation: _volumeVariation,
                    startTimeVariation: _startTimeVariation,
                    loopSpacing: _loopSpacing
                );
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
