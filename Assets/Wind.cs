using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] private Material _treeWindShader;
    [Range(0.0f, 1.0f)] public float _magnitude = 1;
    [Range(-0.5f, 0.5f)] public float _directionOffset = 0;
    public float _frequency = 0.1f;
    public float _windVector;

    // Update is called once per frame
    void Update()
    {
        _windVector = _magnitude * ((Mathf.Sin(2 * _frequency * Time.time) + Mathf.Sin(Mathf.PI * _frequency * Time.time)) / 4) + _directionOffset;
        _treeWindShader.SetFloat("_BendDirection", _windVector);
    }
}
