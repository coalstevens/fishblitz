using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODOS
// Water Landing? Ducks?
// High flying, low flying
// Add shadows!


public interface IBirdState
{
    void Enter(BirdBrain bird);
    void Update(BirdBrain bird);
    void Exit(BirdBrain bird);
}
public interface IBirdLandingSpot
{
    public Vector2 GetPositionTarget();
    public void OnBirdEntry(BirdBrain bird);
    public void OnBirdExit(BirdBrain bird);
    public int GetSortingOrder();
}
public interface IShelterable : IBirdLandingSpot { };

public interface IPerchable : IBirdLandingSpot
{
    public bool IsThereSpace();
    public void ReserveSpace(BirdBrain bird);
}
public interface IPerchableLowElevation : IPerchable {};
public interface IPerchableHighElevation : IPerchable {};

public class BirdBrain : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public string _stateName;
    [SerializeField] public string _previousStateName;
    [SerializeField] public Collider2D ViewDistance;
    [SerializeField] private Collider2D _birdCollider;
    [SerializeField] public List<string> FlockableBirdsNames = new();
    [SerializeField] private float _reactionIntervalSecs = 2f;
    [SerializeField] private float _reactionTimeSecs = 0.5f;
    
    public delegate void BirdDestroyedHandler(Bird bird);
    public static event BirdDestroyedHandler BirdDestroyed;
    public delegate void BirdStateChangeHandler(Bird bird, Vector2 targetPosition, IBirdState newState);
    public static event BirdStateChangeHandler BirdStateChanged;

    // State
    public IBirdState BirdState;
    public IBirdState PreviousBirdState;
    private FacingDirection _facingDirection = FacingDirection.West;
    public float BehaviorDuration = 0;
    private float _behaviorElapsed = 0;
    public Vector2 TargetPosition;
    public IBirdLandingSpot LandingTargetSpot;
    private float _lastFlockReactionTime = 0;

    // States
    public FlyingState Flying;
    public LandingState Landing;
    public ShelteredState Sheltered;
    public PerchedState Perched;
    public GroundedState Grounded;
    public FleeingState Fleeing;
    public SoaringState Soaring;
    public SoaringLandingState SoarLanding;

    // Properties
    public Animator Animator { get => _animator; }
    public SpriteRenderer Renderer { get => _renderer; }
    public DynamicSpriteSorting SpriteSorting { get => _spriteSorting; }
    public NearbyBirdTracker NearbyBirdTracker { get => _nearbyBirdsTracker; }
    public ParticleSystem LeafSplash { get => _leafSplash; }
    public Renderer LeafSplashRenderer {get => _leafSplashRenderer; }
    public Rigidbody2D RigidBody { get => _rb; }
    public Collider2D BirdCollider { get => _birdCollider; }

    // References
    private Animator _animator;
    private SpriteRenderer _renderer;
    private DynamicSpriteSorting _spriteSorting;
    private NearbyBirdTracker _nearbyBirdsTracker;
    private ParticleSystem _leafSplash;
    private Renderer _leafSplashRenderer;
    private Rigidbody2D _rb;
    private Bird _thisBird;
    private Collider2D _worldCollider;
    private Bounds _worldBounds;
    private Collider2D _playerCollider;
    private List<Action> _unsubscribeHooks = new();

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _spriteSorting = _renderer.GetComponent<DynamicSpriteSorting>();
        _nearbyBirdsTracker = ViewDistance.GetComponent<NearbyBirdTracker>();
        _leafSplash = GetComponentInChildren<ParticleSystem>();
        _leafSplashRenderer = _leafSplash.GetComponent<Renderer>();
        _thisBird = GetComponent<Bird>();
        _birdCollider = GetComponent<Collider2D>();

        _worldCollider = GameObject.FindGameObjectWithTag("World").GetComponent<Collider2D>();
        _worldBounds = _worldCollider.bounds;

        TargetPosition = transform.position;
        TransitionToState(Flying);
    }

    private void Update()
    {
        UpdateStateText();
        SelfDestructIfWorldExited();
        UpdateFacingDirection();
        BirdState?.Update(this);
    }

    private void OnEnable()
    {
        BirdStateChanged += ReactToNearbyBirdStateChange;
    }

    private void OnDisable()
    {
        BirdStateChanged -= ReactToNearbyBirdStateChange;
    }

    private void ReactToNearbyBirdStateChange(Bird thatBird, Vector2 thatBirdTargetPosition, IBirdState thatBirdNewState)
    {
        // AKA GoBeWithYourFlockieBoys()

        if (thatBird == _thisBird) return; // that bird be this bird 
        if (!FlockableBirdsNames.Contains(thatBird.Name)) return; // that bird don't flock wit this bird 
        if (!_nearbyBirdsTracker.NearbyBirds.Contains(thatBird)) return; // that bird ain't nearby 
        if (Time.time - _lastFlockReactionTime < _reactionIntervalSecs) return;

        // Fleeing beats following the flock
        if (BirdState is FleeingState) 
            return; 

        // Flee with the flock!
        if (thatBirdNewState is FleeingState && BirdState is not FleeingState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Fleeing));
            return;
        }

        // React to a landing flockmate by landing near them, from Flying
        if (thatBirdNewState is LandingState && BirdState is FlyingState) 
        {
            Landing.SetLandingCircle(Landing.FlockLandingCircleRadius, thatBirdTargetPosition);
            StartCoroutine(ReactiveTransitionToStateWithDelay(Landing));
            return;
        }

        // React to a landing flockmate by landing near them, from Soaring
        if (thatBirdNewState is SoaringLandingState && BirdState is SoaringState) 
        {
            SoarLanding.SetLandingCircle(SoarLanding.FlockLandingCircleRadius, thatBirdTargetPosition); // react by landing near that bird
            StartCoroutine(ReactiveTransitionToStateWithDelay(SoarLanding));
            return;
        }

        // Fly with that bird
        if (thatBirdNewState is FlyingState && BirdState is not FlyingState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Flying));
            return;
        }
        
        // Soar with that bird
        if (thatBirdNewState is SoaringState && BirdState is not SoaringState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Soaring));
            return;
        }
    }   

    private IEnumerator ReactiveTransitionToStateWithDelay(IBirdState newState) {
        _lastFlockReactionTime = Time.time;
        float _delay = UnityEngine.Random.Range(0, _reactionTimeSecs);
        yield return new WaitForSeconds(_delay);
        TransitionToState(newState);
    }

    public void TransitionToState(IBirdState newState)
    {
        if (newState == null)
            Debug.LogError("Unexpected code path.");
        BirdState?.Exit(this);
        PreviousBirdState = BirdState;
        BirdState = newState;
        BirdState.Enter(this);
        BirdStateChanged(_thisBird, TargetPosition, newState); // this must be after the Enter() call
    }

    private void SelfDestructIfWorldExited()
    {
        if (!_worldBounds.Contains(transform.position))
        {
            BirdDestroyed(GetComponent<Bird>());
            Destroy(gameObject);
        }
    }

    public bool TickAndCheckBehaviorTimer()
    {
        _behaviorElapsed += Time.deltaTime;
        if (_behaviorElapsed >= BehaviorDuration)
        {
            _behaviorElapsed = 0;
            return true;
        }
        return false;
    }

    private void UpdateFacingDirection()
    {
        FacingDirection previousDirection = _facingDirection;

        if (_rb.velocity.x > 0)
            _facingDirection = FacingDirection.East;
        else if (_rb.velocity.x < 0)
            _facingDirection = FacingDirection.West;

        if (previousDirection != _facingDirection)
            MatchAnimationToFacingDirection();
    }

    public void FrightenBird()
    {
        if (BirdState is not FleeingState)
            TransitionToState(Fleeing);
    }

    private void MatchAnimationToFacingDirection()
    {
        transform.localScale = new Vector3
        (
            _facingDirection == FacingDirection.West ?
                Mathf.Abs(transform.localScale.x) :
                -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    public void PlayAnimationThenStop(string animationName)
    {
        StartCoroutine(PlayAnimationThenStopMotion(animationName));
    }

    private IEnumerator PlayAnimationThenStopMotion(string animationName)
    {
        _animator.Play(animationName);
        yield return null; // wait 1 frame for animation to begin
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _rb.velocity = Vector2.zero;
    }

    private void UpdateStateText()
    {
        _stateName = BirdState switch
        {
            LandingState => "Landing",
            FlyingState => "Flying",
            PerchedState => "Perched",
            FleeingState => "Fleeing",
            GroundedState => "Grounded",
            ShelteredState => "Sheltered",
            _ => ""
        };
        _previousStateName = PreviousBirdState switch
        {
            LandingState => "Landing",
            FlyingState => "Flying",
            PerchedState => "Perched",
            FleeingState => "Fleeing",
            GroundedState => "Grounded",
            ShelteredState => "Sheltered",
            _ => ""
        };
    }

    public bool TryFindLandingSpotOfType<T>(Vector2 landingCircleCenter, float landingCircleRadius) where T : IBirdLandingSpot
    {
        List<Collider2D> _collidersInLandingCircle = Physics2D.OverlapCircleAll(landingCircleCenter, landingCircleRadius).ToList();
        List<T> _availableSpots = new();
        
        foreach (var _collider in _collidersInLandingCircle)
            if (_collider.TryGetComponent<T>(out var spot) && (spot is IShelterable || (spot is IPerchable perchable && perchable.IsThereSpace())))
                _availableSpots.Add(spot);

        if (_availableSpots.Count == 0)
            return false;

        LandingTargetSpot = _availableSpots[UnityEngine.Random.Range(0, _availableSpots.Count)];
        TargetPosition = LandingTargetSpot.GetPositionTarget();
        if (LandingTargetSpot is IPerchable perch)
            perch.ReserveSpace(this);

        return true;
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