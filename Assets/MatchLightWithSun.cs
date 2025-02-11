using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MatchLightWithSun : MonoBehaviour
{
    private SunLightControl _sun;
    private Light2D _light;

    private void Awake()
    {
        _light = GetComponent<Light2D>();

        GameObject sunObject = GameObject.FindGameObjectWithTag("Sun");
        if (sunObject != null)
            _sun = sunObject.GetComponent<SunLightControl>();
    }

    private void LateUpdate()
    {
        if (_sun != null)
        {
            _light.intensity = _sun.Intensity;
            _light.color = _sun.LightColor;
        }
    }
}
