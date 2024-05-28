using UnityEngine;

public class FogManager : MonoBehaviour
{
    private SpriteRenderer _fog;
    private Transform _playerCamera;
    bool _isRaining = true;
    void Start()
    {
        _fog = GetComponent<SpriteRenderer>();
        _playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void OnEnable() {
        RainManager.Instance.RainStateChange += OnRainStateChange;
    }

    void OnDisable() {
        RainManager.Instance.RainStateChange -= OnRainStateChange;
    }

    private void OnRainStateChange(RainStates state)
    {
        if (state == RainStates.Raining)
        {
            _isRaining = true;
            _fog.enabled = false;
        }
        else
        {
            _isRaining = false;
            _fog.enabled = true;
        }
    }
    void Update()
    {
        if (_isRaining)
            transform.position = new Vector3(_playerCamera.position.x, _playerCamera.position.y, transform.position.z);
    }
}
