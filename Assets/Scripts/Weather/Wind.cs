using System;
using UnityEngine;
using ReactiveUnity;

public class Wind : MonoBehaviour
{
    [SerializeField] private AudioClip _gustSFX;

    // There is a forcefield for particle systems
    [Header("Non Particle System Affected Entities")]
    [SerializeField] private Material _treeWindShader;
    [SerializeField] private ParticleSystem _rain;
    [SerializeField] private ParticleSystemForceField _windAOE;
    private PlayerMovementController _playerMovementController;
    [SerializeField] private float _rainParticleRotationScalar = 0.1f;
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
    private enum GustDirections { East = 1, West = -1 }
    [SerializeField] private GustDirections _gustDirection = GustDirections.East;

    public enum WindStates { Fluctating, GustBuilding, GustPeak, GustDying }
    public Reactive<WindStates> WindState = new Reactive<WindStates>(WindStates.Fluctating);
    public float _windXVector; // Wind doesn't blow North/South
    private float _gustStartTime;
    private float _flucStartTime;
    private float _flucDuration;
    private Action _unsubscribe;
    private Action _stopSoundCB;

    void Start()
    {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _unsubscribe = WindState.OnChange((prev, curr) => OnStateChange(prev, curr));
        EnterFluctuation();
    }

    void OnStateChange(WindStates previous, WindStates current)
    {
        switch (current)
        {
            case WindStates.GustBuilding: 
                _stopSoundCB = AudioManager.Instance.PlayLoopingSFX(_gustSFX, 1, false, true, 2);
                break;
            case WindStates.GustDying:
                _stopSoundCB?.Invoke();
                _stopSoundCB = null;
                break;
        }
    }

    void OnDisable()
    {
        _unsubscribe();
    }

    void Update()
    {
        // Update wind vector
        switch (WindState.Value)
        {
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

    private void UpdateAffectedEntities()
    {
        // Adjust the amount of bending on the tree material
        _treeWindShader.SetFloat("_BendDirection", _windXVector);

        // Set the force field force
        _windAOE.directionX = _windXVector;

        // Add some slight rotation to the raindrops
        var rot = _rain.rotationOverLifetime;
        rot.z = new ParticleSystem.MinMaxCurve(-1 * _windXVector * _rainParticleRotationScalar);

        // Update player speed limits
        if (WindState.Value != WindStates.Fluctating)
        {
            CardinalVector _moveSpeedMultiplier;
            _moveSpeedMultiplier.north = 1;
            _moveSpeedMultiplier.south = 1;
            // movespeed is reduced 1/3 when its a headwind 
            _moveSpeedMultiplier.east = (_windXVector > 0) ?
                                        1 + _windXVector * _windXVector * _playerMoveSpeedMultiplier / 3 :
                                        1 + _windXVector * _windXVector * _playerMoveSpeedMultiplier;
            _moveSpeedMultiplier.west = (_windXVector > 0) ?
                                        1 - _windXVector * _windXVector * _playerMoveSpeedMultiplier :
                                        1 - _windXVector * _windXVector * _playerMoveSpeedMultiplier / 3;
            _playerMovementController.SetMoveSpeedMultiplier(_moveSpeedMultiplier);
        }
        else
        {
            _playerMovementController.SetMoveSpeedMultiplier(new CardinalVector(1));
        }
    }

    /// <summary>
    /// Returns a 1D vector from a semi-random sinusodial function to simulate wind oscillation.
    /// </summary>
    private float GetFluctuationValue()
    {
        return _flucMagnitude * ((Mathf.Sin(2 * _flucFrequency * Time.time) + Mathf.Sin(Mathf.PI * _flucFrequency * Time.time)) / 4);
    }

    private void FluctuatingHandler()
    {
        _windXVector = GetFluctuationValue();

        if (!_gustEnabled)
        {
            return;
        }

        if (Time.time - _flucStartTime >= _flucDuration)
        {
            WindState.Value = WindStates.GustBuilding;
        }
    }

    private void GustBuildingHandler()
    {
        _windXVector += (int)_gustDirection * _gustChangePerFrame;
        if (Mathf.Abs(_windXVector) >= Mathf.Abs(_gustMagnitude))
        {
            _windXVector = (int)_gustDirection * _gustMagnitude;
            _gustStartTime = Time.time;
            WindState.Value = WindStates.GustPeak;
        }
    }

    //TODO: add additional oscillation during peak gust?
    private void GustPeakHandler()
    {
        if (Time.time - _gustStartTime >= _gustPeakDurationSecs)
        {
            WindState.Value = WindStates.GustDying;
        }
    }

    private void GustDyingHandler()
    {
        _windXVector -= (int)_gustDirection * _gustChangePerFrame;
        float fluctuatingTarget = GetFluctuationValue();
        if (Mathf.Abs(_windXVector) <= Mathf.Abs(fluctuatingTarget))
        {
            _windXVector = fluctuatingTarget;
            EnterFluctuation();
        }
    }

    private void EnterFluctuation()
    {
        WindState.Value = WindStates.Fluctating;
        _flucDuration = UnityEngine.Random.Range(_gustMinIntervalSec, _gustMaxIntervalSec);
        _flucStartTime = Time.time;
    }

    private void OnDestroy()
    {
        // Reset multiplier when exiting scene
        _playerMovementController.SetMoveSpeedMultiplier(new CardinalVector(1));
    }
}
