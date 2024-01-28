using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainManager : MonoBehaviour
{
    private Transform _playerCamera;
    void Start()
    {
        _playerCamera = GameObject.FindWithTag("MainCamera").transform;
    }

    void Update()
    {
        transform.position = _playerCamera.position;    
    }
}
