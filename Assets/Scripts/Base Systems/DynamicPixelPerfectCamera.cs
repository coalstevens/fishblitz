using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicPixelPerfectCamera : MonoBehaviour
{
    [Header("Pixel Perfect")]
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField] private int scaleFactor = 1;
    [SerializeField] private int minimumWidth = 960;
    [SerializeField] private int minimumHeight = 540;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Transform _tracked;

    public float PixelsPerUnit => pixelsPerUnit;
    public int ScaleFactor => scaleFactor;

    private Camera _camera;
    private Transform _player;
    private int _lastWidth;
    private int _lastHeight;
    private List<Action> _unsubscribeHooks = new();

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        UpdateOrthoSize();
        Screen.fullScreen = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void LateUpdate()
    {
        if (_player != null)
        {
            transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        }
    }

    private void Update()
    {
        if (Screen.width != _lastWidth || Screen.height != _lastHeight)
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            UpdateOrthoSize();
        }
    }

    private void UpdateOrthoSize()
    {
        // int width = Mathf.Max(Screen.width, minimumWidth);
        int height = Mathf.Max(Screen.height, minimumHeight);

        float orthoSize = height / 2f / (pixelsPerUnit * scaleFactor);
        _camera.orthographicSize = orthoSize;
    }
}
