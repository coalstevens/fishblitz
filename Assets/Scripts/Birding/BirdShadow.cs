using UnityEngine;

public class BirdShadow : MonoBehaviour
{
    [SerializeField] private float _idleYPosition = -0.03125f;
    [SerializeField] private float _flyingYPosition = -1.25f;
    [SerializeField] private float _soaringYPosition = -6.25f;
    [SerializeField] private float _idleOpacity = 0.3f;
    [SerializeField] private float _soaringOpacity = 0.03f;
    [SerializeField] private float _shadowTransitionSpeed = 1f;

    private float _targetYPosition;
    private bool _isTransitioning = false;
    private Vector2 _landingBirdStartPosition;
    private float _totalDisanceToLand;
    private SpriteRenderer _renderer;
    private BirdBrain _bird;
    private BirdBrain.IBirdState _currentState = null;
    private BirdBrain.IBirdState _previousState = null;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _bird = transform.parent.GetComponent<BirdBrain>();
    }

    private void Update()
    {
        _renderer.sortingOrder = _bird.Renderer.sortingOrder - 1; // shadow below bird

        if (_isTransitioning)
        {
            if (_currentState is BirdBrain.LandingState) 
                InterpolateShadowWithBirdTarget(); 
            else
                MoveShadowByDelta();
            InterpolateShadowOpacity();
        }

        _currentState = _bird.BirdState;
        if (_previousState != _currentState)
        {
            _previousState = _currentState;
            UpdateShadowTarget();
        }
    }

    private void InterpolateShadowWithBirdTarget() {
        float _distanceTravelled = Vector2.Distance(_bird.transform.position, _landingBirdStartPosition);
        float _interpolateValue = _distanceTravelled / _totalDisanceToLand;

        transform.localPosition = new Vector2
        (
            transform.localPosition.x,
            Mathf.Lerp(_flyingYPosition, _idleYPosition, _interpolateValue)
        );

        if (Mathf.Approximately(_interpolateValue, 1f))
        {
            transform.localPosition = new Vector2(transform.localPosition.x, _idleYPosition);
            _isTransitioning = false;
            return;
        } 
    }

    private void MoveShadowByDelta()
    {
        float _delta = _shadowTransitionSpeed * Time.deltaTime;
        float _currentY = transform.localPosition.y;
        float _distanceToTarget = Mathf.Abs(_targetYPosition - _currentY);

        // snap to target
        if (_distanceToTarget <= _delta)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, _targetYPosition);
            _isTransitioning = false;
            return;
        }

        // move delta to target
        transform.localPosition = new Vector2
        (
            transform.localPosition.x,
            _currentY < _targetYPosition ? _currentY + _delta : _currentY - _delta
        );
    }

    private void InterpolateShadowOpacity()
    {
        float _interpolateValue = Normalize(transform.localPosition.y, _idleYPosition, _soaringYPosition);
        _renderer.color = new Color
        (
            _renderer.color.r,
            _renderer.color.g,
            _renderer.color.b,
            Mathf.Clamp01(Mathf.Lerp(_idleOpacity, _soaringOpacity, _interpolateValue))
        );
    }

    private void UpdateShadowTarget()
    {
        // hidden
        if (_currentState is BirdBrain.ShelteredState)
        {
            _isTransitioning = false;
            _renderer.enabled = false;
            return;
        }

        _renderer.enabled = true;

        // standing
        if (_currentState is BirdBrain.PerchedState || _currentState is BirdBrain.GroundedState)
        {
            SetMoveShadowTarget(_idleYPosition);
            return;
        }

        // flying
        if (_currentState is BirdBrain.FlyingState || _currentState is BirdBrain.LandingState || _currentState is BirdBrain.FleeingState)
        {
            SetMoveShadowTarget(_flyingYPosition);
            return;
        }

        // landing
        if (_currentState is BirdBrain.LandingState) {
            _landingBirdStartPosition = _bird.transform.position;
            _totalDisanceToLand = Vector2.Distance(_bird.TargetPosition, _landingBirdStartPosition);
            _isTransitioning = true;
            return;
        }

        // soaring
        if (_currentState is BirdBrain.SoaringState || _currentState is BirdBrain.SoaringLandingState)
        {
            SetMoveShadowTarget(_soaringYPosition);
            return;
        }

        Debug.LogError("Unexpected state for bird shadow.");
    }

    private void SetMoveShadowTarget(float targetY)
    {
        _targetYPosition = targetY;
        _isTransitioning = true;
    }

    private static float Normalize(float value, float min, float max)
    {
        return (max == min) ? 0f : (value - min) / (max - min);
    }
}