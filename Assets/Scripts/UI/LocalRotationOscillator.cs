using UnityEngine;

public class LocalRotationOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("Max rotation in degrees around Z axis")]
    [SerializeField] private float _amplitude = 5f;
    [Tooltip("Speed in cycles per second")]
    [SerializeField] private float _frequency = 1f;
    [Tooltip("Phase offset")]
    [SerializeField] private float _offset = 0f;
    [Tooltip("Use unscaled time (ignores Time.timeScale)")]
    [SerializeField] private bool _useUnscaledTime = true;
    [Tooltip("Start oscillating when enabled")]
    [SerializeField] private bool _playOnEnable = true;

    private Vector3 _startRotation;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        _startRotation = transform.localRotation.eulerAngles;
    }

    private void OnEnable()
    {
        if (_playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        float time = _useUnscaledTime ? Time.unscaledTime : Time.time;
        float z = _startRotation.z + _amplitude * Mathf.Sin((time * _frequency * 2f * Mathf.PI) + _offset);

        transform.localRotation = Quaternion.Euler(_startRotation.x, _startRotation.y, z);
    }

    public void Play()
    {
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Reset()
    {
        Stop();
        transform.localRotation = Quaternion.Euler(_startRotation);
    }

    public void SetAmplitude(float amplitude)
    {
        _amplitude = amplitude;
    }

    public void SetFrequency(float frequency)
    {
        _frequency = frequency;
    }
}
