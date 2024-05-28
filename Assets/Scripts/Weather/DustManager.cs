using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustManager : MonoBehaviour
{
    private Transform _playerCamera;
    void Start()
    {
        _playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
    void Update()
    {
        transform.position = new Vector3(_playerCamera.position.x, _playerCamera.position.y, transform.position.z);
    }
}
