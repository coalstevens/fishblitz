using UnityEngine;

public class GlobalWindProperties : MonoBehaviour
{
    [Header("Wind Fog Parameters")]
    [SerializeField] private Vector2 _windVelocity = new Vector2(1, 0);
    [SerializeField] private float _windDensity = 5f;
    [SerializeField] private Vector2 _windXYScaling = Vector2.zero;
    [SerializeField] private Vector2 _windSmoothStep = new Vector2(0, 1);

    [SerializeField] private bool _updateEveryFrame;

    private void Start()
    {
        PushToShaders();
    }

    private void Update()
    {
        if (_updateEveryFrame)
            PushToShaders();
    }

    private void PushToShaders()
    {
        Shader.SetGlobalVector("_WindVelocity", _windVelocity);
        Shader.SetGlobalFloat("_WindDensity", _windDensity);
        Shader.SetGlobalVector("_WindXYScaling", _windXYScaling);
        Shader.SetGlobalVector("_WindSmoothStep", _windSmoothStep);
    }

    public void SetWindVelocity(Vector2 v) { _windVelocity = v; PushToShaders(); }
    public void SetWindDensity(float d) { _windDensity = d; PushToShaders(); }
    public void SetWindXYScaling(Vector2 s) { _windXYScaling = s; PushToShaders(); }
    public void SetWindSmoothStep(Vector2 s) { _windSmoothStep = s; PushToShaders(); }
}
