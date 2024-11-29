using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ReactiveUnity;
using UnityEditor.Rendering;
using UnityEngine;

// TODOS
// Add shrub entry splash
// Fleeing
// Sprinting 
// Water Landing
// Flocking
// High flying, low flying
// default preference should be flight, as it's always an option

public interface IBirdLandingSpot
{
    public Vector2 GetPositionTarget();
    public bool AreBirdsFrightened();
    public void OnBirdEntry(BirdBrain bird);
    public void OnBirdExit(BirdBrain bird);
}

public interface IShelterable : IBirdLandingSpot { };
public interface IPerchable : IBirdLandingSpot
{
    public bool IsThereSpace();
    public int GetSortingOrder();
}

public class BirdBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D _viewDistance;
    [SerializeField] private Collider2D _frightDistance;

    [Header("Flying Behavior")]
    [SerializeField] private float _flightSpeedLimit = 2f;
    [SerializeField] private float _steerForceLimit = 4f;
    [SerializeField] private float _flyingForceUpdateInterval = 0.5f;
    [SerializeField] private float _wanderRingRadius = 2f;
    [SerializeField] private float _wanderRingDistance = 2f;
    [SerializeField] private Vector2 _flyingDurationRange = new Vector2(2f, 20f);
    [SerializeField] private BirdStates _defaultPreference = BirdStates.GROUNDED;
    [Range(0f, 1f)][SerializeField] private float _perchPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _shelterPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _groundPreference = 0.34f;

    [Header("Flying - Boids Flocking")]
    [SerializeField] private List<string> _flockableBirdsNames = new();
    [SerializeField] private float _separationWeight = 1.0f;
    [SerializeField] private float _alignmentWeight = 1.0f;
    [SerializeField] private float _cohesionWeight = 1.0f;

    [Header("Grounded Behavior")]
    [SerializeField] private Vector2 _groundedDurationRange = new Vector2(5f, 20f);
    [SerializeField] private Vector2 _timeTillHopLimits = new Vector2(2f, 5f);
    [SerializeField] private Vector2 _twoHopForceLimits = new Vector2(1f, 1f);

    [Header("Sheltered Behavior")]
    [SerializeField] private Vector2 _shelteredDurationRange = new Vector2(5f, 20f);

    [Header("Perched Behavior")]
    [SerializeField] private Vector2 _perchedDurationRange = new Vector2(5f, 20f);

    public delegate void BirdDestroyedHandler(Transform bird);
    public static event BirdDestroyedHandler BirdDestroyed;
    
    private float _behaviorDuration = 0;
    private float _behaviorElapsed = 0;
    private float _timeUntilNextHop = 0;
    private float _timeSinceHop = 0;
    private float _lastFlyingForceUpdateTime = 0;

    private Animator _animator;
    private SpriteRenderer _renderer;
    private IBirdLandingSpot _targetBirdSpot;
    private BirdStates _landingSpotStateType;
    private Bounds _worldBounds;
    private Rigidbody2D _rb;
    private List<Action> _unsubscribeHooks = new();
    public Vector2 _targetPosition;
    private Collider2D _worldCollider;
    private Vector2 _flyingForce = Vector2.zero;

    public enum BirdStates { FLYING, FLEEING, PERCHED, SHELTERED, GROUNDED, LANDING };
    public Reactive<BirdStates> _birdState = new Reactive<BirdStates>(BirdStates.FLYING);
    public Reactive<FacingDirection> _facingDirection = new Reactive<FacingDirection>(FacingDirection.West);

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _worldCollider = GameObject.FindGameObjectWithTag("World").GetComponent<Collider2D>();
        _worldBounds = _worldCollider.bounds;

        _unsubscribeHooks.Add(_facingDirection.OnChange((_, _) => MatchAnimationToFacingDirection()));
        _unsubscribeHooks.Add(_birdState.OnChange((prev, curr) => OnStateChange(prev, curr)));

        transform.position = GetRandomWorldPosition();
        _targetPosition = transform.position;
    }

    private void OnDestroy()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook();

        if (gameObject != null)   
            BirdDestroyed(transform);
    }

    private void Update()
    {
        if (IsBehaviourDurationExpired())
        {
            _behaviorElapsed = 0;
            _birdState.Value = _birdState.Value != BirdStates.FLYING ? BirdStates.FLYING : BirdStates.LANDING;
        }

        UpdateFacingDirectionBasedOnVelocity();
        HandleCurrentState();
    }

    private bool IsBehaviourDurationExpired()
    {
        _behaviorElapsed += Time.deltaTime;
        return _behaviorElapsed >= _behaviorDuration;
    }

    private void HandleCurrentState()
    {
        switch (_birdState.Value)
        {
            case BirdStates.FLYING: Flying(); break;
            case BirdStates.FLEEING:
            case BirdStates.PERCHED: CheckIfFrightened(); break;
            case BirdStates.SHELTERED: CheckIfFrightened(); break;
            case BirdStates.GROUNDED: Ground(); break;
            case BirdStates.LANDING: Land(); break;
            default: break;
        }
    }

    private void OnStateChange(BirdStates previousState, BirdStates newState)
    {
        ExitState(previousState);
        EnterState(newState);
    }

    private void EnterState(BirdStates newState)
    {
        switch (_birdState.Value)
        {
            case BirdStates.FLYING:
                _animator.Play("Flying");
                _behaviorDuration = UnityEngine.Random.Range(_flyingDurationRange.x, _flyingDurationRange.y);
                break;
            case BirdStates.FLEEING:
            case BirdStates.PERCHED:
                _targetBirdSpot.OnBirdEntry(this);
                _animator.Play("Idle");
                _renderer.transform.GetComponent<DynamicSpriteSorting>().enabled = false;
                _renderer.sortingOrder = (_targetBirdSpot as IPerchable).GetSortingOrder() + 1;
                _behaviorDuration = UnityEngine.Random.Range(_perchedDurationRange.x, _perchedDurationRange.y);
                break;
            case BirdStates.SHELTERED:
                _renderer.enabled = false;
                _targetBirdSpot.OnBirdEntry(this);
                _behaviorDuration = UnityEngine.Random.Range(_shelteredDurationRange.x, _shelteredDurationRange.y);
                break;
            case BirdStates.GROUNDED:
                _animator.Play("Idle");
                _behaviorDuration = UnityEngine.Random.Range(_groundedDurationRange.x, _groundedDurationRange.y);
                ResetHopTimer();
                break;
            case BirdStates.LANDING:
                SelectPreferredLandingSpot(); // Fly to a shelter, perch, ground, etc
                break;
            default:
                break;
        }
    }

    private void ExitState(BirdStates previousState)
    {
        switch (previousState)
        {
            case BirdStates.FLEEING:
            case BirdStates.PERCHED:
                _targetBirdSpot.OnBirdExit(this);
                _renderer.transform.GetComponent<DynamicSpriteSorting>().enabled = true;
                break;
            case BirdStates.SHELTERED:
                _renderer.enabled = true;
                _targetBirdSpot.OnBirdExit(this);
                break;
            case BirdStates.LANDING:
                _rb.velocity = Vector2.zero;
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _worldCollider)
            Destroy(gameObject);
    }

    private void SelectPreferredLandingSpot()
    {
        float _randomValue = UnityEngine.Random.Range(0f, _perchPreference + _shelterPreference + _groundPreference);
        if (_randomValue < _perchPreference)
        {
            if (SelectLandingSpotByType<IPerchable>())
            {
                _landingSpotStateType = BirdStates.PERCHED;
                return;
            }
        }
        else if (_randomValue < _perchPreference + _shelterPreference)
        {
            if (SelectLandingSpotByType<IShelterable>())
            {
                _landingSpotStateType = BirdStates.SHELTERED;
                return;
            }
        }
        else if (_randomValue <= _perchPreference + _shelterPreference + _groundPreference)
        {
            // The current Wander() target is the target lock. 
            _landingSpotStateType = BirdStates.GROUNDED;
            return;
        }

        _birdState.Value = _defaultPreference;
    }

    private void Land()
    {
        float threshold = 0.05f;
        if (Vector2.Distance(_targetPosition, transform.position) > threshold)
        {
            _rb.AddForce(Seek(_targetPosition));
            return;
        }
        _birdState.Value = _landingSpotStateType;
    }

    private void UpdateFacingDirectionBasedOnVelocity()
    {
        if (_rb.velocity.x > 0)
            _facingDirection.Value = FacingDirection.East;
        else if (_rb.velocity.x < 0)
            _facingDirection.Value = FacingDirection.West;
    }

    private bool SelectLandingSpotByType<T>() where T : IBirdLandingSpot
    {
        List<Collider2D> _collidersInViewRange = new();
        List<T> _availableSpots = new();
        _viewDistance.OverlapCollider(new ContactFilter2D().NoFilter(), _collidersInViewRange);

        foreach (var _collider in _collidersInViewRange)
            if (_collider.TryGetComponent<T>(out var spot) && (spot is not IPerchable perchable || perchable.IsThereSpace()))
                _availableSpots.Add(spot);

        if (_availableSpots.Count == 0)
            return false;

        _targetBirdSpot = _availableSpots[UnityEngine.Random.Range(0, _availableSpots.Count)];
        _targetPosition = _targetBirdSpot.GetPositionTarget();

        return true;
    }

    private void Flying()
    {
        _rb.AddForce(_flyingForce);
        if (Time.time - _lastFlyingForceUpdateTime >= _flyingForceUpdateInterval) 
            UpdateFlyingForce();
    }
    private void UpdateFlyingForce() {
        // update wander ring target
        Vector2 _ringCenter = (Vector2)transform.position + _rb.velocity.normalized * _wanderRingDistance;
        _targetPosition = _ringCenter + _wanderRingRadius * UnityEngine.Random.insideUnitCircle.normalized;
        _lastFlyingForceUpdateTime = Time.time;

        _flyingForce = Seek(_targetPosition);
        if (_flockableBirdsNames.Count > 0 )
            _flyingForce += CalculateBoidForce();
    }

    private void Ground()
    {
        _timeSinceHop += Time.deltaTime;
        if (_timeSinceHop < _timeUntilNextHop)
            return;

        Vector2 _hopForce = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(_twoHopForceLimits.x, _twoHopForceLimits.y);
        _rb.AddForce(_hopForce, ForceMode2D.Impulse);
        StartCoroutine(PlayAnimationThenStopMotion("Two Hop"));
        ResetHopTimer();
    }

    private void CheckIfFrightened()
    {
        if (_targetBirdSpot.AreBirdsFrightened())
            _birdState.Value = BirdStates.FLEEING;
    }

    private void ResetHopTimer()
    {
        _timeSinceHop = 0;
        _timeUntilNextHop = UnityEngine.Random.Range(_timeTillHopLimits.x, _timeTillHopLimits.y);
    }

    private IEnumerator PlayAnimationThenStopMotion(string animationName)
    {
        _animator.Play(animationName);
        yield return null; // wait 1 frame for animation to begin
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _rb.velocity = Vector2.zero;
    }

    private Vector2 GetRandomWorldPosition()
    {
        return new Vector2
        (
            UnityEngine.Random.Range(_worldBounds.min.x, _worldBounds.max.x),
            UnityEngine.Random.Range(_worldBounds.min.y, _worldBounds.max.y)
        );
    }

    private Vector2 Seek(Vector2 target)
    {
        Vector2 _desired = (target - (Vector2)transform.position).normalized * _flightSpeedLimit;
        Vector2 _steer = _desired - _rb.velocity;
        if (_steer.magnitude >= _steerForceLimit)
            _steer = _steer.normalized * _steerForceLimit;

        return _steer;
    }

    private void MatchAnimationToFacingDirection()
    {
        transform.localScale = new Vector3(
                    _facingDirection.Value == FacingDirection.West ?
                        Mathf.Abs(transform.localScale.x) :
                        -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);
    }

    private Vector2 CalculateBoidForce()
    {
        Vector2 _separation = Vector2.zero; // Prevents birds getting too close
        Vector2 _alignment = Vector2.zero; // Urge to match direction of others
        Vector2 _cohesion = Vector2.zero; // Urge to move towards centroid of flock
        int _count = 0;

        foreach (var _bird in _viewDistance.GetComponent<NearbyBirdTracker>().NearbyBirds)
        {
            if (_bird.gameObject == null) continue;
            if (!_flockableBirdsNames.Contains(_bird.GetComponent<Bird>().Name)) continue;
            float _distance = Vector2.Distance(transform.position, _bird.position);
            _separation += (Vector2)(transform.position - _bird.position) / _distance;
            _alignment += _bird.transform.GetComponent<Rigidbody2D>().velocity;
            _cohesion += (Vector2)_bird.position;
            _count++;
        }

        if (_count > 0)
        {
            _separation /= _count;
            _alignment /= _count;
            _cohesion /= _count;
            _cohesion = (_cohesion - (Vector2)transform.position).normalized;
        }

        return 
            _separation.normalized * _separationWeight +
            _alignment.normalized * _alignmentWeight +
            _cohesion * _cohesionWeight;
    }

}

/**********************************************************************
BEHAVIOR NOTES

Chickadee
- Active, inquisitive
- Approach humans and feeders boldly
- Shrub forager
- They form and lead mixed-species flocks in Winter

Juncos
- Ground Feeders
- Active in winter
- Shy of humans, but tolerant of feeders.
- Often flocking in winter, sometimes mixed with sparrows or other species

Bluebirds
- Territorial cavity nester
- Aggresively defend nesting sites
- Aeriel foragers
- Berry eaters
- Found in small family groups or pairs

Kinglets
- Hyperactive, tiny and constantly moving
- Terriortial singing, especially during breeding
- Winter resilient
- Mixed flocks with chickadees or titmice

House Sparrows
- Highly Social
- Large flocks
- Very urban
- Aggresive

White Crowned Sparrows
- Ground foragers
- Migratory
- Shy to humans, regular feeders

***********************************************************************/