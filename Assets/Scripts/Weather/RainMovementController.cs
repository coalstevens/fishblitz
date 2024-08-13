using UnityEngine;

public class RainMovementController : MonoBehaviour
{
    Transform _playerCamera;
    ParticleSystem _rain;
    private const float EMITTER_Y_OFFSET = 6f;
    private void Awake()
    {
        _playerCamera = GameObject.FindGameObjectWithTag("Player").transform;
        _rain = GetComponent<ParticleSystem>();

        if (RainManager.Instance.RainState == RainStates.NotRaining)
        {
            _rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        SetEmitterPosition(PlayerData.Instance.SceneSpawnPosition, EMITTER_Y_OFFSET);
        _rain.Clear();

        // this is a manual prewarm!
        // its beautiful, stunning, just the best
        _rain.Simulate(_rain.main.duration);
        _rain.Play();
    }

    private void OnEnable()
    {
        RainManager.Instance.RainStateChange += OnRainStateChange;
    }

    private void OnDisable()
    {
        if (RainManager.Instance != null)
            RainManager.Instance.RainStateChange -= OnRainStateChange;
    }

    private void Update()
    {
        SetEmitterPosition(_playerCamera.position, EMITTER_Y_OFFSET);
    }

    void SetEmitterPosition(Vector3 newPosition, float yOffset)
    {
        Vector3 _offsetPosition = newPosition;
        _offsetPosition.y += EMITTER_Y_OFFSET;
        transform.position = _offsetPosition;
    }

    void OnRainStateChange(RainStates newState)
    {
        switch (newState)
        {
            case RainStates.Raining:
                _rain.Play();
                break;
            case RainStates.NotRaining:
                _rain.Stop();
                break;
        }
    }
}
