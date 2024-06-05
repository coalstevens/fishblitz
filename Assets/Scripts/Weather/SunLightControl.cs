using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SunLightControl : MonoBehaviour
{
    
    private GameClock _gameClock;
    public int _lightUpdateIntervalGameMins = 10;
    [Header("Daytime and Nightime")]
    [SerializeField] private float _dayLightIntensity = 1f;
    [SerializeField] private float _nightLightIntensity = 0.1f;
    [SerializeField] private Color _dayTimeLight = Color.white;
    [SerializeField] private Color _nightTimeLight = Color.white;
    
    [Header("Sunset and Sunrise")]
    [SerializeField] private float _sunriseStartGameHour24h = 6f;
    [SerializeField] private float _sunriseEndGameHour24h = 10f;

    [SerializeField] private float _sunsetStartGameHour24h = 18f;
    [SerializeField] private float _sunsetEndGameHour24h = 22f;
    [SerializeField] Gradient _sunrise;
    [SerializeField] Gradient _sunset;
    private Light2D _light;
    enum LightStates {Sunrise, Sunset, Bright, Dark}
    private Reactive<LightStates> _lightState = new Reactive<LightStates>(LightStates.Sunrise);
    private int _minuteCounter = 0;
    public float debug;

    void Awake()
    {
        _light = GetComponent<Light2D>();
        _gameClock = GameObject.FindWithTag("GameClock").GetComponent<GameClock>();
        _gameClock.GameMinute.OnChange((prev, curr) => OnMinuteChange());
        _lightState.OnChange((prev,curr) => UpdateLight());
        UpdateLight();
    }

    // Update is called once per frame
    void OnMinuteChange()
    {
        _minuteCounter++;
        if (_minuteCounter != _lightUpdateIntervalGameMins) {
            return;
        }
        _minuteCounter = 0;

        // sunset
        if (_gameClock.GameHour.Value >= _sunsetStartGameHour24h && _gameClock.GameHour.Value < _sunsetEndGameHour24h) {
            _lightState.Value = LightStates.Sunset;
            TimeAdjustLightWithGradient(_sunset);
            return;
        }
        // sunrise
        if (_gameClock.GameHour.Value >= _sunriseStartGameHour24h && _gameClock.GameHour.Value < _sunriseEndGameHour24h) {
            _lightState.Value = LightStates.Sunrise;
            TimeAdjustLightWithGradient(_sunrise);
            return;
        }
        // daytime
        if (_gameClock.GameHour.Value >= _sunriseEndGameHour24h && _gameClock.GameHour.Value < _sunsetStartGameHour24h) {
            _lightState.Value = LightStates.Bright;
            return;
        }
        //night
        _lightState.Value = LightStates.Dark;
    }

    void UpdateLight() {
        switch(_lightState.Value) {
            case LightStates.Bright:
                _light.intensity = _dayLightIntensity;
                _light.color = _dayTimeLight;
                break;
            case LightStates.Dark:
                _light.intensity = _nightLightIntensity;
                _light.color = _nightTimeLight;
                break;
            case LightStates.Sunrise:
                break;
            case LightStates.Sunset:
                break;
        }
    }

    void TimeAdjustLightWithGradient(Gradient gradient) {
        // _hack is required to prevent an off-by-one error at the hour.
        // It's required because this function is run after _gameMinute is updated
        // but before _gameHour is updated. 
        int _hack = _gameClock.GameMinute.Value == 0 ? 1 : 0;

        float _current = (_gameClock.GameHour.Value + _hack) * 60f + _gameClock.GameMinute.Value;
        debug = _current;
        float _input;
        if (_lightState.Value == LightStates.Sunrise) {
            _input = Map(_current, _sunriseStartGameHour24h * 60f, _sunriseEndGameHour24h * 60f, 0, 1);
            _light.intensity = Map(_current, _sunriseStartGameHour24h * 60f, _sunriseEndGameHour24h * 60f, _nightLightIntensity, _dayLightIntensity);
            _light.color = gradient.Evaluate(_input);
            return;
        }
        if (_lightState.Value == LightStates.Sunset) {
            _input = Map(_current, _sunsetStartGameHour24h * 60f, _sunsetEndGameHour24h * 60f, 0, 1);
            _light.intensity = Map(_current, _sunsetStartGameHour24h * 60f, _sunsetEndGameHour24h * 60f, _dayLightIntensity, _nightLightIntensity);
            _light.color = gradient.Evaluate(_input);
            return;
        }
    }

    // maps value s of range a into range b
    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }
}
