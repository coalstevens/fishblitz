using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

// TODOS
// Water Landing? Ducks?
// might be a bug when the bird is hopping on the ground and the state changes during this process?
// ^ does this make the bird slide along the ground in idle positon?

public partial class BirdBrain : MonoBehaviour
{
    public interface IBirdState
    {
        public void Enter(BirdBrain bird);
        public void Update(BirdBrain bird);
        public void Exit(BirdBrain bird);
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

    [Header("General")]
    [SerializeField] public string _stateName;
    [SerializeField] public string _previousStateName;
    [SerializeField] public Collider2D ViewDistance;

    [Header("Flocking")]
    [SerializeField] private List<string> FlockableBirdsNames = new();
    [SerializeField] private float _reactionIntervalSecs = 2f;
    [SerializeField] private float _slowestReactionTimeSecs = 0.5f;
    
    public delegate void BirdDestroyedHandler(Bird bird);
    public static event BirdDestroyedHandler BirdDestroyed;
    private delegate void BirdStateChangeHandler(Bird bird, Vector2 targetPosition, IBirdState newState);
    private static event BirdStateChangeHandler BirdStateChanged;

    // Properties
    public Renderer Renderer { get => _renderer; }
    public IBirdState BirdState { get => _birdState; }

    // State
    private IBirdState _birdState;
    private IBirdState PreviousBirdState;
    private FacingDirection _facingDirection = FacingDirection.West;
    public float BehaviorDuration = 0;
    private float _behaviorElapsed = 0;
    public Vector2 TargetPosition = Vector2.zero;
    public IBirdLandingSpot LandingTargetSpot;
    private float _lastFlockReactionTime = 0;
    private bool _isReacting = false;
    private bool _isFrightened = false;

    // States
    private FlyingState Flying = new();
    private LandingState Landing = new();
    private ShelteredState Sheltered = new();
    private PerchedState Perched = new();
    private GroundedState Grounded = new();
    private FleeingState Fleeing = new();
    private SoaringState Soaring = new();
    private SoaringLandingState SoarLanding = new();

    // References
    private Collider2D _birdCollider;
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
    
    private const string WATER_LAYER = "Water";
    private const string BIRDS_LAYER = "Birds";
    private int WaterLayer;
    private int BirdsLayer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _spriteSorting = GetComponentInChildren<DynamicSpriteSorting>();
        _nearbyBirdsTracker = GetComponentInChildren<NearbyBirdTracker>();
        _leafSplash = GetComponentInChildren<ParticleSystem>();
        _leafSplashRenderer = _leafSplash.GetComponent<Renderer>();
        _thisBird = GetComponent<Bird>();
        _birdCollider = GetComponent<Collider2D>();

        WaterLayer = LayerMask.NameToLayer(WATER_LAYER);
        BirdsLayer = LayerMask.NameToLayer(BIRDS_LAYER);

        _worldCollider = GameObject.FindGameObjectWithTag("World").GetComponent<Collider2D>();
        _worldBounds = _worldCollider.bounds;

        TargetPosition = transform.position;
        TransitionToState(Flying);
    }

    private void Update()
    {
        if (_isFrightened && _birdState is not FleeingState) 
            TransitionToState(Fleeing);
        UpdateStateText();
        SelfDestructIfWorldExited();
        UpdateFacingDirection();
        _birdState?.Update(this);
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
        if (Time.time - _lastFlockReactionTime < _reactionIntervalSecs) return;
        if (thatBird == null) return; // that bird don't be
        if (_nearbyBirdsTracker == null) return;
        if (thatBird == _thisBird) return; // that bird be this bird 
        if (!FlockableBirdsNames.Contains(thatBird.BirdName)) return; // that bird don't flock wit this bird 
        if (!_nearbyBirdsTracker.NearbyBirds.Contains(thatBird)) return; // that bird ain't nearby 

        // Fleeing beats following the flock
        if (_birdState is FleeingState) 
            return; 

        // Flee with the flock!
        if (thatBirdNewState is FleeingState && _birdState is not FleeingState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Fleeing));
            return;
        }

        // React to a landing flockmate by landing near them, from Flying
        if (thatBirdNewState is LandingState && _birdState is FlyingState) 
        {
            Landing.SetLandingCircle(Landing.FlockLandingCircleRadius, thatBirdTargetPosition);
            StartCoroutine(ReactiveTransitionToStateWithDelay(Landing));
            return;
        }

        // React to a landing flockmate by landing near them, from Soaring
        if (thatBirdNewState is SoaringLandingState && _birdState is SoaringState) 
        {
            SoarLanding.SetLandingCircle(SoarLanding.FlockLandingCircleRadius, thatBirdTargetPosition); // react by landing near that bird
            StartCoroutine(ReactiveTransitionToStateWithDelay(SoarLanding));
            return;
        }

        // Fly with that bird
        if (thatBirdNewState is FlyingState && _birdState is not FlyingState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Flying));
            return;
        }
        
        // Soar with that bird
        if (thatBirdNewState is SoaringState && _birdState is not SoaringState) {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Soaring));
            return;
        }
    }   

    private IEnumerator ReactiveTransitionToStateWithDelay(IBirdState newState) {
        _lastFlockReactionTime = Time.time;
        _isReacting = true;
        float _delay = UnityEngine.Random.Range(0, _slowestReactionTimeSecs);
        yield return new WaitForSeconds(_delay);
        _isReacting = false;
        TransitionToState(newState);
    }

    private void TransitionToState(IBirdState newState)
    {
        if (newState == null)
            Debug.LogError("Unexpected code path.");

        // ignore any other transitions while bird is reacting
        if (_isReacting)
            return; 
        _birdState?.Exit(this);
        PreviousBirdState = _birdState;
        _birdState = newState;
        _birdState.Enter(this);
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

    private bool TickAndCheckBehaviorTimer()
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
        _isFrightened = true;
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

    private void PlayAnimationThenStop(string animationName)
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
        _stateName = _birdState switch
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

    private bool TryFindLandingSpotOfType<T>(Vector2 landingCircleCenter, float landingCircleRadius) where T : IBirdLandingSpot
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