using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSounds : MonoBehaviour
{
    [System.Serializable]
    private class BirdSound
    {
        public AudioClip Sound;
        public float Volume;
    }

    [SerializeField] private List<BirdSound> _birdSounds = new();
    [SerializeField] private Vector2 _soundPlayInterval = new(5f, 15f);
    [SerializeField] private float _silenceRadius = 20f;
    [SerializeField] private float _fullVolumeRadius = 5f;

    private float _timeSinceLastSound;
    private float _nextSoundDelay;
    private Transform _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetNextSoundDelay();
        if (_birdSounds.Count == 0)
            this.enabled = false;
    }

    private void Update()
    {
        if (_player == null || _birdSounds.Count == 0) return;

        _timeSinceLastSound += Time.deltaTime;
        if (_timeSinceLastSound >= _nextSoundDelay)
        {
            _timeSinceLastSound = 0;
            SetNextSoundDelay();
            PlayRandomBirdSound();
        }
    }

    private void SetNextSoundDelay()
    {
        _nextSoundDelay = Random.Range(_soundPlayInterval.x, _soundPlayInterval.y);
    }

    private void PlayRandomBirdSound()
    {
        if (_birdSounds.Count == 0) return;

        float _distance = Vector3.Distance(transform.position, _player.position);
        if (_distance >= _silenceRadius) return;

        BirdSound _selectedSound = _birdSounds[Random.Range(0, _birdSounds.Count)];
        float _volume = _selectedSound.Volume;

        if (_distance > _fullVolumeRadius)
        {
            float t = (_distance - _fullVolumeRadius) / (_silenceRadius - _fullVolumeRadius);
            _volume = Mathf.Lerp(_selectedSound.Volume, 0f, Mathf.Pow(t, 0.5f)); // Quadratic distance falloff
        }

        AudioManager.Instance.PlaySFX(_selectedSound.Sound, _volume);
        // StartCoroutine(EnlargeTemporarily());
    }

    // FOR DEBUGGING
    private IEnumerator EnlargeTemporarily()
    {
        transform.localScale *= 2f; // Double the size
        yield return new WaitForSeconds(3f);
        transform.localScale /= 2f; // Reset to original size
    }
}