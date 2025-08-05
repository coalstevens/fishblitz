using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Rendering;

// TODOS
// Water Landing? Ducks?
// might be a bug when the bird is hopping on the ground and the state changes during this process?
// ^ does this make the bird slide along the ground in idle positon?

[Serializable]
public partial class BirdBrain : MonoBehaviour
{
    public class BirdInstanceData
    {
        [SerializeField] public GameClock.DayPeriods PeriodSpawned;
        [SerializeField] public GameClock.Seasons SeasonSpawned;
        public Vector3 SpawnPosition;
        public Reactive<bool> IsTagged = new Reactive<bool>(false);

        public void Tag(BirdSpeciesData SpeciesData)
        {
            IsTagged.Value = true;
            AudioManager.Instance.PlaySFX(SpeciesData.CaptureSound, SpeciesData.SoundVolume);
            SpeciesData.PlayerData.BirdingLog.AddToLog(SpeciesData.SpeciesName, SeasonSpawned, PeriodSpawned);
        }
    }

    public interface IBirdState
    {
        public void Enter(BirdBrain bird);
        public void Update(BirdBrain bird);
        public void Exit(BirdBrain bird);
        public void DrawGizmos(BirdBrain bird);
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
    public interface IPerchableLowElevation : IPerchable { };
    public interface IPerchableHighElevation : IPerchable { };

    [Header("General")]
    public BirdSpeciesData SpeciesData;
    public Collider2D ViewDistance;

    public delegate void BirdDestroyedHandler(BirdBrain bird);
    public static event BirdDestroyedHandler BirdDestroyed;
    private delegate void BirdStateChangeHandler(BirdBrain bird, Vector2 targetPosition, IBirdState newState);
    private static event BirdStateChangeHandler BirdStateChanged;

    // Properties
    public IBirdState BirdState { get => _birdState; }
    public BirdBehaviourConfig Config { get => SpeciesData.BehaviourConfig; }

    // State
    public string _stateName;
    public string _previousStateName;
    private IBirdState _birdState;
    private IBirdState _previousBirdState;
    private FacingDirection _facingDirection = FacingDirection.West;
    public float _behaviorDuration = 0;
    private float _behaviorElapsed = 0;
    public Vector2 TargetPosition = Vector2.zero;
    public IBirdLandingSpot LandingTargetSpot;
    private float _lastFlockReactionTime = 0;
    private bool _isReacting = false;
    private bool _isFrightened = false;

    // States
    [HideInInspector] public BirdInstanceData InstanceData;
    private LowFlyingState LowFlying = new();
    private LandingState Landing = new();
    private ShelteredState Sheltered = new();
    private PerchedState Perched = new();
    private GroundedState Grounded = new();
    private FleeingState Fleeing = new();
    private HighFlyingState HighFlying = new();
    private HighLandingState HighLanding = new();

    // References
    private Collider2D _birdCollider;
    private BirdAnimatorController _animator; 
    private SortingGroup _sortingGroup;
    private DynamicSpriteSorting _spriteSorting;
    private NearbyBirdTracker _nearbyBirdsTracker;
    private ParticleSystem _leafSplash;
    private Renderer _leafSplashRenderer;
    private Rigidbody2D _rb;
    private Collider2D _worldCollider;
    private Bounds _worldBounds;

    private const string WATER_LAYER = "Water";
    private const string BIRDS_LAYER = "Birds";
    private int WaterLayer;
    private int BirdsLayer;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<BirdAnimatorController>();
        _sortingGroup = GetComponent<SortingGroup>();
        _spriteSorting = GetComponent<DynamicSpriteSorting>();
        _nearbyBirdsTracker = GetComponentInChildren<NearbyBirdTracker>();
        _leafSplash = GetComponentInChildren<ParticleSystem>();
        _leafSplashRenderer = _leafSplash.GetComponent<Renderer>();
        _birdCollider = GetComponent<Collider2D>();

        WaterLayer = LayerMask.NameToLayer(WATER_LAYER);
        BirdsLayer = LayerMask.NameToLayer(BIRDS_LAYER);

        _worldCollider = GameObject.FindGameObjectWithTag("World").GetComponent<Collider2D>();
        _worldBounds = _worldCollider.bounds;

        TargetPosition = transform.position;
        TransitionToState(LowFlying);
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

    private void ReactToNearbyBirdStateChange(BirdBrain thatBird, Vector2 thatBirdTargetPosition, IBirdState thatBirdNewState)
    {
        // AKA GoBeWithYourFlockieBoys()
        if (Time.time - _lastFlockReactionTime < Config.ReactionCooldownSecs) return;
        if (thatBird == null) return; // that bird don't be
        if (_nearbyBirdsTracker == null) return;
        if (thatBird == this) return; // that bird be this bird 
        if (!thatBird.SpeciesData.FlockableSpecies.Contains(thatBird.SpeciesData)) return; // that bird don't flock wit this bird 
        if (!_nearbyBirdsTracker.NearbyBirds.Contains(thatBird)) return; // that bird ain't nearby 

        // Fleeing beats following the flock
        if (_birdState is FleeingState)
            return;

        // Flee with the flock!
        if (thatBirdNewState is FleeingState && _birdState is not FleeingState)
        {
            StartCoroutine(ReactiveTransitionToStateWithDelay(Fleeing));
            return;
        }

        // React to a landing flockmate by landing near them, from Low Flying
        if (thatBirdNewState is LandingState && _birdState is LowFlyingState)
        {
            Landing.SetLandingTargetArea(Config.LowLanding.FlockLandingCircleRadius, thatBirdTargetPosition);
            StartCoroutine(ReactiveTransitionToStateWithDelay(Landing));
            return;
        }

        // React to a landing flockmate by landing near them, from High Flying
        if (thatBirdNewState is HighLandingState && _birdState is HighFlyingState)
        {
            HighLanding.SetLandingCircle(Config.HighLanding.FlockLandingAreaRadius, thatBirdTargetPosition); // react by landing near that bird
            StartCoroutine(ReactiveTransitionToStateWithDelay(HighLanding));
            return;
        }

        // Fly Low with that bird
        if (thatBirdNewState is LowFlyingState && _birdState is not LowFlyingState)
        {
            StartCoroutine(ReactiveTransitionToStateWithDelay(LowFlying));
            return;
        }

        // Fly High with that bird
        if (thatBirdNewState is HighFlyingState && _birdState is not HighFlyingState)
        {
            StartCoroutine(ReactiveTransitionToStateWithDelay(HighFlying));
            return;
        }
    }

    private IEnumerator ReactiveTransitionToStateWithDelay(IBirdState newState)
    {
        _lastFlockReactionTime = Time.time;
        _isReacting = true;
        float _delay = UnityEngine.Random.Range(Config.ReactionTimeRangeSecs.x, Config.ReactionTimeRangeSecs.y);
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
        _previousBirdState = _birdState;
        _birdState = newState;
        _birdState.Enter(this);
        BirdStateChanged(this, TargetPosition, newState); // this must be after the Enter() call
    }

    private void SelfDestructIfWorldExited()
    {
        if (!_worldBounds.Contains(transform.position))
        {
            BirdDestroyed(this);
            Destroy(gameObject);
        }
    }

    private bool HasBehaviorTimerElapsed()
    {
        _behaviorElapsed += Time.deltaTime;
        if (_behaviorElapsed >= _behaviorDuration)
        {
            _behaviorElapsed = 0;
            return true;
        }
        return false;
    }

    private void UpdateFacingDirection()
    {
        FacingDirection previousDirection = _facingDirection;

        if (_rb.linearVelocity.x > 0)
            _facingDirection = FacingDirection.East;
        else if (_rb.linearVelocity.x < 0)
            _facingDirection = FacingDirection.West;

        if (previousDirection != _facingDirection)
            _animator.MatchAnimationToFacingDirection(_facingDirection);
    }

    public void FrightenBird()
    {
        _isFrightened = true;
    }

    private void UpdateStateText()
    {
        static string GetStateName(IBirdState state) => state switch
        {
            LandingState => "Landing",
            LowFlyingState => "LowFlying",
            PerchedState => "Perched",
            FleeingState => "Fleeing",
            GroundedState => "Grounded",
            ShelteredState => "Sheltered",
            HighFlyingState => "HighFlying",
            HighLandingState => "HighLanding",
            _ => ""
        };

        _stateName = GetStateName(_birdState);
        _previousStateName = GetStateName(_previousBirdState);
    }

    private bool TrySetLandingSpotOfType<T>(Vector2 landingCircleCenter, float landingCircleRadius) where T : IBirdLandingSpot
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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public Vector2 GetVelocity()
    {
        return _rb.linearVelocity;
    }

    private void OnDrawGizmos()
    {
        _birdState?.DrawGizmos(this);
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