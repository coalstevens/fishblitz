using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class BirdElevationVisualTweaks : MonoBehaviour
{
    [SerializeField] private float _transitionSpeed = 0.3f;

    [Header("Shadow")]
    [SerializeField] private Transform _shadow;
    [SerializeField] private SpriteRenderer _shadowRenderer;
    [SerializeField] private float _groundedShadowYPosition = -0.03125f;
    [SerializeField] private float _lowFlyingShadowYPosition = -1.25f;
    [SerializeField] private float _highFlyingShadowYPosition = -6.25f;
    [SerializeField] private float _groundedShadowOpacity = 0.3f;
    [SerializeField] private float _highFlyingShadowOpacity = 0.03f;

    [Header("Bird")]
    [SerializeField] private BirdBrain _bird;
    [SerializeField] private SpriteRenderer _birdRenderer;
    [SerializeField] private BirdAnimatorController _birdAnimator;
    [SerializeField] private float _groundedBirdOpacity = 1f;
    [SerializeField] private float _highFlyingBirdOpacity = 0.8f;
    private float _elevation; // Normalized. 0 = grounded. 1 = High Flying
    private float _previousElevation;
    private enum Elevations { HIGH, LOW, GROUND };
    private Dictionary<Elevations, float> _elevationMap = new Dictionary<Elevations, float>
    {
        {Elevations.GROUND, 0 },
        {Elevations.LOW, 0 }, // determined on init from shadow positions
        {Elevations.HIGH, 1 }
    };
    private Elevations _targetElevation;
    private bool _atTargetElevation = true;
    private Vector2 _landingBirdStartPosition;
    private float _totalDistanceToLand;
    private BirdBrain.IBirdState _currentState = null;
    private BirdBrain.IBirdState _previousState = null;

    private void Start()
    {
        Assert.IsNotNull(_shadow);
        Assert.IsNotNull(_bird);
        Assert.IsNotNull(_shadowRenderer);
        Assert.IsNotNull(_birdRenderer);
        Assert.IsNotNull(_birdAnimator);

        _elevationMap[Elevations.LOW] = _lowFlyingShadowYPosition / (_highFlyingShadowYPosition - _groundedShadowYPosition);
    }

    private void Update()
    {
        _currentState = _bird.BirdState;
        if (_previousState != _currentState)
        {
            _previousState = _currentState;
            TransitionToState(_currentState);
        }

        MoveTowardsTargetElevation();

        if (_previousElevation != _elevation)
        {
            _previousElevation = _elevation;
            InterpolateYPositionByElevation(_elevation, _shadow, _groundedShadowYPosition, _highFlyingShadowYPosition);
            InterpolateOpacityByElevation(_elevation, _shadowRenderer, _groundedShadowOpacity, _highFlyingShadowOpacity);
            InterpolateOpacityByElevation(_elevation, _birdRenderer, _groundedBirdOpacity, _highFlyingBirdOpacity);
        }
    }

    private void MoveTowardsTargetElevation()
    {
        if (_atTargetElevation)
            return;

        // landing, lerp over landing distance 
            if (_targetElevation == Elevations.GROUND)
            {
                float distanceTravelled = Vector2.Distance(_bird.transform.position, _landingBirdStartPosition);
                float landingCompletionNormalized = Mathf.Clamp01(distanceTravelled / _totalDistanceToLand);
                _elevation = Mathf.Lerp(_elevationMap[Elevations.LOW], _elevationMap[Elevations.GROUND], landingCompletionNormalized);
                if (Mathf.Approximately(_elevation, _elevationMap[Elevations.GROUND]))
                {
                    _elevation = _elevationMap[_targetElevation];
                    _atTargetElevation = true;
                }
                return;
            }

        // increase or decrease by fixed delta
        float elevationChangeDelta = _transitionSpeed * Time.deltaTime;
        if (_targetElevation == Elevations.HIGH && _elevation < _elevationMap[Elevations.HIGH] ||
            _targetElevation == Elevations.LOW && _elevation < _elevationMap[Elevations.LOW])
        {
            _elevation += elevationChangeDelta;

        }
        else if (_targetElevation == Elevations.HIGH && _elevation > _elevationMap[Elevations.HIGH] ||
                 _targetElevation == Elevations.LOW && _elevation > _elevationMap[Elevations.LOW])
        {
            _elevation -= elevationChangeDelta;
        }

        _elevation = Mathf.Clamp01(_elevation);

        if (Mathf.Abs(_elevationMap[_targetElevation] - _elevation) <= elevationChangeDelta)
        {
            _elevation = _elevationMap[_targetElevation];
            _atTargetElevation = true;
            if (_currentState is BirdBrain.HighFlyingState || _currentState is BirdBrain.LowFlyingState)
                _birdAnimator.PlayFlying();
        }
    }

    private void InterpolateYPositionByElevation(float elevation, Transform objectTransform, float groundedYPosition, float highFlyingYPosition)
    {
        objectTransform.localPosition = new Vector2
        (
            objectTransform.localPosition.x,
            Mathf.Lerp(groundedYPosition, highFlyingYPosition, elevation)
        );
    }

    private void InterpolateOpacityByElevation(float elevation, SpriteRenderer renderer, float groundedOpacity, float highFlyingOpacity)
    {
        renderer.color = new Color
        (
            renderer.color.r,
            renderer.color.g,
            renderer.color.b,
            Mathf.Clamp01(Mathf.Lerp(groundedOpacity, highFlyingOpacity, elevation))
        );
    }

    private void TransitionToState(BirdBrain.IBirdState newState)
    {
        _shadowRenderer.enabled = true; // enabled by default

        switch (newState)
        {
            // bird is hidden
            case BirdBrain.ShelteredState: 
                _shadowRenderer.enabled = false; // hide the shadow
                _elevation = _elevationMap[Elevations.LOW];
                _targetElevation = Elevations.LOW;
                break;
            // grounded states
            case BirdBrain.PerchedState:
            case BirdBrain.GroundedState:
                _targetElevation = Elevations.GROUND;
                _elevation = _elevationMap[Elevations.GROUND];
                break;
            // low flying states
            case BirdBrain.LowFlyingState:
            case BirdBrain.FleeingState:
                _targetElevation = Elevations.LOW;
                break;
            // high flying states
            case BirdBrain.HighFlyingState:
                _targetElevation = Elevations.HIGH;
                break;
            // special case, elevation lerped over landing distance
            case BirdBrain.LandingState:
            case BirdBrain.HighLandingState:
                _targetElevation = Elevations.GROUND;
                _landingBirdStartPosition = _bird.transform.position;
                _totalDistanceToLand = Vector2.Distance(_bird.TargetPosition, _landingBirdStartPosition);
                break;
            default:
                Debug.LogError("Unexpected state for bird elevation.");
                break;
        }

        // Trigger animation if changing elevation
        _atTargetElevation = Mathf.Approximately(_elevation, _elevationMap[_targetElevation]);
        if (!_atTargetElevation)
        {
            if (_elevation < _elevationMap[_targetElevation])
                _birdAnimator.PlayFlapping();
            else
                _birdAnimator.PlayGliding();
        }
    }
}