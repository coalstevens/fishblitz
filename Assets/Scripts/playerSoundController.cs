using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _caughtSoundEffect;
    [SerializeField] private AudioClip _missedSoundEffect;

    public void PlaySound(string soundEffect) {
        switch (soundEffect) {
            case "Caught":
                    _audioSource.clip = _caughtSoundEffect;
                break;
            case "Missed":
                    _audioSource.clip = _missedSoundEffect;
                break;
        }
        _audioSource.Play();
    }
}
