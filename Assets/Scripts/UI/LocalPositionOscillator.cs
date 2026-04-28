using UnityEngine;
using UnityEngine.UI;

public class LocalPositionOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("Distance to oscillate on each axis")]
    [SerializeField] private Vector2 _amplitude = new Vector2(0f, 5f);
    [Tooltip("Speed in cycles per second")]
    [SerializeField] private float _frequency = 1f;
    [Tooltip("Phase offset for each axis")]
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [Tooltip("Use unscaled time (ignores Time.timeScale)")]
    [SerializeField] private bool _useUnscaledTime = true;
    [Tooltip("Start oscillating when enabled")]
    [SerializeField] private bool _playOnEnable = true;

    private Vector3 _startPosition;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        _startPosition = transform.localPosition;
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
        float x = _startPosition.x + _amplitude.x * Mathf.Sin((time * _frequency * 2f * Mathf.PI) + _offset.x);
        float y = _startPosition.y + _amplitude.y * Mathf.Sin((time * _frequency * 2f * Mathf.PI) + _offset.y);

        transform.localPosition = new Vector3(x, y, _startPosition.z);
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
        transform.localPosition = _startPosition;
    }

    public void SetAmplitude(Vector2 amplitude)
    {
        _amplitude = amplitude;
    }

    public void SetFrequency(float frequency)
    {
        _frequency = frequency;
    }
}
