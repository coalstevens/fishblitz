using System;
using System.Collections;
using ReactiveUnity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SunLightControl : MonoBehaviour
{
    private GameClock _gameClock;
    [SerializeField] private int _lightUpdateIntervalGameMins = 10;
    [Header("Daytime and Nightime")]
    [SerializeField] private float _dayLightIntensity = 1f;
    [SerializeField] private float _nightLightIntensity = 0.1f;
    [SerializeField] private Color _dayTimeLight = Color.white;
    [SerializeField] private Color _nightTimeLight = Color.white;

    [Header("Sunset and Sunrise")]
    [SerializeField] private float _sunriseStartHour24h = 6f;
    [SerializeField] private float _sunriseEndHour24h = 10f;

    [SerializeField] private float _sunsetStartHour24h = 18f;
    [SerializeField] private float _sunsetEndHour24h = 22f;
    [SerializeField] Gradient _sunrise;
    [SerializeField] Gradient _sunset;
    private Light2D _light;
    private float _intensity;
    private bool _isFaded = false;
    enum LightStates { Sunrise, Sunset, Day, Night }
    private Reactive<LightStates> _lightState = new Reactive<LightStates>(LightStates.Sunrise);
    private int _minuteCounter = 0;

    public float Intensity => _intensity;
    public Color LightColor => _light.color;
    private Coroutine _fadeRoutine;

    void Awake()
    {
        _light = GetComponent<Light2D>();
        _gameClock = GameClock.Instance;
        _gameClock.GameMinute.OnChange((prev, curr) => OnMinuteChange());
        _lightState.OnChange((prev, curr) => UpdateLight());
        _intensity = _light.intensity;

        UpdateLightState();
        UpdateLight();
    }

    void OnMinuteChange()
    {
        if (++_minuteCounter < _lightUpdateIntervalGameMins) return;
        _minuteCounter = 0;
        UpdateLightState();
        if (_lightState.Value == LightStates.Sunrise || _lightState.Value == LightStates.Sunset)
            UpdateLightForTwilight();
    }

    void UpdateLightState()
    {
        int _currentHour = _gameClock.GameHour.Value;
        if (IsWithinRange(_currentHour, _sunsetStartHour24h, _sunsetEndHour24h))
        {
            _lightState.Value = LightStates.Sunset;
            return;
        }
        if (IsWithinRange(_currentHour, _sunriseStartHour24h, _sunriseEndHour24h))
        {
            _lightState.Value = LightStates.Sunrise;
            return;
        }
        _lightState.Value = _currentHour >= _sunriseEndHour24h && _currentHour < _sunsetStartHour24h ? 
            LightStates.Day : LightStates.Night;
    }

    void UpdateLight()
    {
        switch (_lightState.Value)
        {
            case LightStates.Day:
                SetLightSettings(_dayLightIntensity, _dayTimeLight);
                break;
            case LightStates.Night:
                SetLightSettings(_nightLightIntensity, _nightTimeLight);
                break;
            case LightStates.Sunrise:
            case LightStates.Sunset:
                UpdateLightForTwilight();
                break;
        }
    }

    private void UpdateLightForTwilight()
    {
        Gradient gradient = _lightState.Value == LightStates.Sunrise ? _sunrise : _sunset;
        float startHour = _lightState.Value == LightStates.Sunrise ? _sunriseStartHour24h : _sunsetStartHour24h;
        float endHour = _lightState.Value == LightStates.Sunrise ? _sunriseEndHour24h : _sunsetEndHour24h;
        float minIntensity = _lightState.Value == LightStates.Sunrise ? _nightLightIntensity : _dayLightIntensity;
        float maxIntensity = _lightState.Value == LightStates.Sunrise ? _dayLightIntensity : _nightLightIntensity;

        float currentMinutes = (_gameClock.GameHour.Value * 60f) + _gameClock.GameMinute.Value;

        float normalizedTime = Map(currentMinutes, startHour * 60f, endHour * 60f, 0, 1);
        SetLightSettings
        (
            Map(currentMinutes, startHour * 60f, endHour * 60f, minIntensity, maxIntensity),
            gradient.Evaluate(normalizedTime)
        );
    }

    private void SetLightSettings(float intensity, Color color) 
    {
        _intensity = intensity;
        if (!_isFaded)
            _light.intensity = intensity;
        _light.color = color;
    }

    public void FadeOutLight(float duration)
    {
        _isFaded = true;
        _fadeRoutine = StartCoroutine(FadeLightCoroutine(0.005f, duration));
    }

    public void FadeInLight(float duration)
    {
        _isFaded = false;
        _fadeRoutine = StartCoroutine(FadeLightCoroutine(_intensity, duration));
    }

    private IEnumerator FadeLightCoroutine(float targetIntensity, float duration)
    {
        float startIntensity = _light.intensity;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            //_light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
            _light.intensity = ParabolicInterpolate(startIntensity, targetIntensity, elapsedTime / duration, false);
            yield return null;
        }

        _light.intensity = targetIntensity;
    }

    // maps value s of range a into range b
    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    private bool IsWithinRange(float value, float min, float max) => value >= min && value < max;
    public static float ParabolicInterpolate(float a, float b, float t, bool easeIn)
    {
        if (easeIn)
            t = t * t; // start slow, then speed up
        else // easeOut
            t = 1 - (1 - t) * (1 - t); // start fast, then slow down
        return Mathf.Lerp(a, b, t);
    }
}
