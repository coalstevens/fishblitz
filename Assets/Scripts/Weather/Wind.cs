using System;
using UnityEngine;
using ReactiveUnity;

public class Wind : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _windSFX;
    [SerializeField] private float _normalVolume = 0.2f;
    [SerializeField] private float _gustingVolume = 1.0f;

    // There is a forcefield for particle systems
    [Header("Non Particle System Affected Entities")]
    [SerializeField] private Material _larchMaterial;
    [SerializeField] private Material _spruceMaterial;
    [SerializeField] private float _larchGustSpeed = 5;
    [SerializeField] private float _spruceGustSpeed = 5;
    [SerializeField] private float _larchGustStrength = 1;
    [SerializeField] private float _spruceGustStrength = 0.5f;
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
    [SerializeField] private float _gustGrowthPerFrame = 0.001f;
    [SerializeField] private float _gustDecayPerFrame = 0.0002f;
    [SerializeField] private float _gustMagnitude = 1;
    [SerializeField] private float _gustMinIntervalSec = 10;
    [SerializeField] private float _gustMaxIntervalSec = 20;
    private enum GustDirections { East = 1, West = -1 }
    [SerializeField] private GustDirections _gustDirection = GustDirections.East;

    public enum WindStates { Fluctating, GustBuilding, GustPeak, GustDying }
    private Reactive<WindStates> WindState = new Reactive<WindStates>(WindStates.Fluctating);
    private float _windXVector; // State variable. (Wind only blows east/west)
    private float _gustStartTime;
    private float _flucStartTime;
    private float _flucDuration;
    private Action _unsubscribe;
    private Action _stopSoundCB;
    private float _larchOriginalSpeed = 0.4f;
    private float _larchOriginalStrength = 0.5f;
    private float _spruceOriginalSpeed = 0.4f;
    private float _spruceOriginalStrength = 0.5f;
    private Transform _playerCamera;

    void Start()
    {
        _playerMovementController = PlayerMovementController.Instance; 
        _playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _unsubscribe = WindState.OnChange((prev, curr) => OnStateChange(prev, curr));
        _stopSoundCB = AudioManager.Instance.PlayLoopingSFX(_windSFX, _normalVolume, false, true, 2); 
        GetOriginalTreeValues();
        EnterFluctuation();
    }

    void OnStateChange(WindStates previous, WindStates current)
    {
        switch (current)
        {
            case WindStates.GustBuilding: 
                AudioManager.Instance.TryAdjustVolume(_windSFX, _gustingVolume, 1f);
                break;
            case WindStates.GustPeak:
                StartTreeShake();
                break;
            case WindStates.GustDying:
                AudioManager.Instance.TryAdjustVolume(_windSFX, _normalVolume, 5f);
                StopTreeShake();
                break;
        }
    }

    void OnDisable()
    {
        StopTreeShake();
        if (AudioManager.Instance != null)
            _stopSoundCB?.Invoke();
        _stopSoundCB = null;
        _unsubscribe();
    }

    void Update()
    {
        _windAOE.transform.position = _playerCamera.transform.position;

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
        ApplyWindVectorToAffectedEntities();
    }

    private void ApplyWindVectorToAffectedEntities()
    {
        // Adjust the amount of bending on the tree material
        _larchMaterial.SetFloat("_BendDirection", _windXVector);
        _spruceMaterial.SetFloat("_BendDirection", _windXVector);

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
        _windXVector += (int)_gustDirection * _gustGrowthPerFrame;
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
        _windXVector -= (int)_gustDirection * _gustDecayPerFrame;
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
        
        ResetTreeMaterials();
    }

    public void GetOriginalTreeValues() {
        _spruceOriginalSpeed = _spruceMaterial.GetVector("_WindSpeed").x;
        _spruceOriginalStrength = _spruceMaterial.GetFloat("_WindStrength");
        _larchOriginalSpeed = _larchMaterial.GetVector("_WindSpeed").x;
        _larchOriginalStrength = _larchMaterial.GetFloat("_WindStrength");
    }
   public void StartTreeShake() {
        _spruceMaterial.SetVector("_WindSpeed", new Vector4(_spruceGustSpeed,0,0,0));
        _spruceMaterial.SetFloat("_WindStrength", _spruceGustStrength);
        _larchMaterial.SetVector("_WindSpeed", new Vector4(_larchGustSpeed,0,0,0));
        _larchMaterial.SetFloat("_WindStrength", _larchGustStrength);
    }

    public void StopTreeShake() {
        ResetTreeMaterials();
    } 

    private void ResetTreeMaterials() {
        _spruceMaterial.SetVector("_WindSpeed", new Vector4(_spruceOriginalSpeed,0,0,0));
        _spruceMaterial.SetFloat("_WindStrength", _spruceOriginalStrength);
        _larchMaterial.SetVector("_WindSpeed", new Vector4(_larchOriginalSpeed,0,0,0));
        _larchMaterial.SetFloat("_WindStrength", _larchOriginalStrength);
    }
}
