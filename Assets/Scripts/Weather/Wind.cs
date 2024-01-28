using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [Header("Affected Entities")]
    [SerializeField] private Material _treeWindShader;
    [SerializeField] private ParticleSystem _rain;
    [SerializeField] private ParticleSystemForceField _rainWindForceField;
    private PlayerMovementController _playerMovementController;
    [SerializeField] private float _rainParticleRotationScalar = 0.1f;
    [SerializeField] private float _rainXForceMultiplier = 1;
    [SerializeField] private float _playerMoveSpeedMultiplier = 0.4f;
    
    [Header("Fluctuation Settings")]
    [Range(0.0f, 1.0f)] public float _flucMagnitude = 0.5f;
    [Range(-0.5f, 0.5f)] public float _flucDirectionOffset = 0;
    [SerializeField] private float _flucFrequency = 0.5f;
    
    [Header("Gust Settings")]
    [SerializeField] private bool _gustEnabled = true;
    [SerializeField] private float _gustPeakDurationSecs = 5;
    [SerializeField] private float _gustChangePerFrame = 0.001f;
    [SerializeField] private float _gustMagnitude = 1;
    [SerializeField] private float _gustMinIntervalSec = 10;
    [SerializeField] private float _gustMaxIntervalSec = 20;
    private enum GustDirections {East = 1, West = -1}
    [SerializeField] private GustDirections _gustDirection = GustDirections.East;

    public enum WindStates {Fluctating, GustBuilding, GustPeak, GustDying }
    public WindStates _windState;
    public float _windXVector; // Wind doesn't blow North/South
    private float _gustStartTime;
    private float _flucStartTime;
    private float _flucDuration;

    void Start() {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        EnterFluctuation();
    }
    void Update()
    {
        switch (_windState) {
            case WindStates.Fluctating: 
                FluctuatingHandler();
                break;
            case WindStates.GustBuilding:
                GustBuildingHandler();
                break;
            case WindStates.GustPeak:
                GustPeakHandler();
                break;
            case WindStates.GustDying:
                GustDyingHandler();
                break;
        }

        UpdateAffectedEntities();
    }

    private void UpdateAffectedEntities() {
        // Adjust the amount of bending on the tree material
        _treeWindShader.SetFloat("_BendDirection", _windXVector);

        // Add a force to the falling rain 
        _rainWindForceField.directionX = _windXVector * _rainXForceMultiplier;

        // Add some slight rotation to the raindrops
        var rot = _rain.rotationOverLifetime;
        rot.z = new ParticleSystem.MinMaxCurve(-1 * _windXVector * _rainParticleRotationScalar);
        
        // Update player speed limits
        if (_windState != WindStates.Fluctating) {
            CardinalVector _moveSpeedMultiplier;
            _moveSpeedMultiplier.north = 1;
            _moveSpeedMultiplier.south = 1;
            _moveSpeedMultiplier.east = 1 + _windXVector * _playerMoveSpeedMultiplier;   
            _moveSpeedMultiplier.west = 1 - _windXVector * _playerMoveSpeedMultiplier;
            _playerMovementController.SetMoveSpeedMultiplier(_moveSpeedMultiplier);
        }
        else {
            _playerMovementController.SetMoveSpeedMultiplier(new CardinalVector(1));
        }
    }

    /// <summary>
    /// Returns a float from a semi-random sinusodial function to simulate wind oscillation.
    /// </summary>
    private float GetFluctuationValue() {
        return _flucMagnitude * ((Mathf.Sin(2 * _flucFrequency * Time.time) + Mathf.Sin(Mathf.PI * _flucFrequency * Time.time)) / 4);
    }
    private void FluctuatingHandler() {
        _windXVector = GetFluctuationValue();

        if (!_gustEnabled) {
            return;
        }

        if (Time.time - _flucStartTime >= _flucDuration) {
            _windState = WindStates.GustBuilding;
        }
    }
    
    private void GustBuildingHandler() {
        _windXVector += (int) _gustDirection * _gustChangePerFrame;
        if (Mathf.Abs(_windXVector) >= Mathf.Abs(_gustMagnitude)) {
            _windXVector = (int) _gustDirection * _gustMagnitude;
            _gustStartTime = Time.time;
            _windState = WindStates.GustPeak;
        }
    }

    //TODO: add additional oscillation during peak gust?
    private void GustPeakHandler() {
        if (Time.time - _gustStartTime >= _gustPeakDurationSecs) {
                _windState = WindStates.GustDying;
        }
    }

    private void GustDyingHandler() {
        _windXVector -= (int) _gustDirection * _gustChangePerFrame;
        float fluctuatingTarget = GetFluctuationValue();
        if (Mathf.Abs(_windXVector) <= Mathf.Abs(fluctuatingTarget)) {
            _windXVector = fluctuatingTarget;
            EnterFluctuation();
        }
    }

    private void EnterFluctuation() {
        _windState = WindStates.Fluctating;
        _flucDuration = UnityEngine.Random.Range(_gustMinIntervalSec, _gustMaxIntervalSec);
        _flucStartTime = Time.time;
    }
}
