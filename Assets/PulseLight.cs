using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulseLight : MonoBehaviour
{
    private Light2D _light;
    private bool _brighten = true;
    [SerializeField] float _minIntensity = 1f;
    [SerializeField] float _maxIntensity = 1.5f;
    [SerializeField] float _intensityChangePerFrame = 0.001f;
    void Start()
    {
        _light = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_brighten) { 
            if (_light.intensity >= _maxIntensity) {
                _brighten = false;
            }
            _light.intensity += _intensityChangePerFrame;
        }
        else {
            if (_light.intensity <= _minIntensity) {
                _brighten = true;
            }
            _light.intensity -= _intensityChangePerFrame;
        }
    }

    public void SetIntensity(float min, float max) {
        _minIntensity = min;
        _maxIntensity = max;
        _light.intensity = _minIntensity;
    }
}
