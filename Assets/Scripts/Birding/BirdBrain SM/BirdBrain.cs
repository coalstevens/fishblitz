using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODOS
// Fleeing
// Water Landing? Ducks?
// High flying, low flying
// obstacle avoidance

public interface IBirdLandingSpot
{
    public Vector2 GetPositionTarget();
    public bool AreBirdsFrightened();
    public void OnBirdEntry(BirdBrain bird);
    public void OnBirdExit(BirdBrain bird);
}

public interface IBirdState
{
    void Enter(BirdBrain bird);
    void Update(BirdBrain bird);
    void Exit(BirdBrain bird);
}

public interface IShelterable : IBirdLandingSpot { };
public interface IPerchable : IBirdLandingSpot
{
    public bool IsThereSpace();
    public void ReserveSpace(BirdBrain bird);
    public int GetSortingOrder();
}

public class BirdBrain : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public Collider2D ViewDistance;
    [SerializeField] private Collider2D _frightDistance;
    [SerializeField] private Collider2D _birdCollider;
    [SerializeField] public List<string> FlockableBirdsNames = new();
    [SerializeField] public float FlightSpeedLimit = 2f;
    [SerializeField] private float _steerForceLimit = 4f;

    public delegate void BirdDestroyedHandler(Bird bird);
    public static event BirdDestroyedHandler BirdDestroyed;
    public delegate void BirdStateChangeHandler(Bird bird, Vector2 targetPosition, IBirdState newState);
    public static event BirdStateChangeHandler BirdStateChanged;

    // State
    public IBirdState BirdState;
    private FacingDirection _facingDirection = FacingDirection.West;
    public float BehaviorDuration = 0;
    private float _behaviorElapsed = 0;
    public Vector2 TargetPosition;
    public IBirdLandingSpot LandingTargetSpot;

    // States
    public FlyingState Flying;
    public LandingState Landing;
    public ShelteredState Sheltered;
    public PerchedState Perched;
    public GroundedState Grounded;
    public FleeingState Fleeing;

    // Properties
    public Animator Animator { get => _animator; }
    public SpriteRenderer Renderer { get => _renderer; }
    public DynamicSpriteSorting SpriteSorting { get => _spriteSorting; }
    public NearbyBirdTracker NearbyBirdTracker { get => _nearbyBirdsTracker; }
    public ParticleSystem LeafSplash { get => _leafSplash; }
    public Rigidbody2D RigidBody { get => _rb; }
    public Collider2D BirdCollider { get => _birdCollider; }

    // References
    private Animator _animator;
    private SpriteRenderer _renderer;
    private DynamicSpriteSorting _spriteSorting;
    private NearbyBirdTracker _nearbyBirdsTracker;
    private ParticleSystem _leafSplash;
    private Rigidbody2D _rb;
    private Bird _thisBird;
    private Collider2D _worldCollider;
    private Bounds _worldBounds;
    private List<Action> _unsubscribeHooks = new();

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _spriteSorting = _renderer.GetComponent<DynamicSpriteSorting>();
        _nearbyBirdsTracker = ViewDistance.GetComponent<NearbyBirdTracker>();
        _leafSplash = GetComponentInChildren<ParticleSystem>();
        _thisBird = GetComponent<Bird>();
        _birdCollider = GetComponent<Collider2D>();

        _worldCollider = GameObject.FindGameObjectWithTag("World").GetComponent<Collider2D>();
        _worldBounds = _worldCollider.bounds;

        TargetPosition = transform.position;
        TransitionToState(Flying);
    }

    private void Update()
    {
        SelfDestructIfExitedWorld();
        UpdateFacingDirection();
        BirdState?.Update(this);
    }

    private void OnEnable() {
        BirdStateChanged += ReactToNearbyBirdStateChange;       
    }

    private void OnDisable() {
        BirdStateChanged -= ReactToNearbyBirdStateChange;       
    }

    private void ReactToNearbyBirdStateChange(Bird thatBird, Vector2 thatBirdTargetPosition, IBirdState thatBirdNewState)
    {
        if (thatBird == _thisBird) return; // that bird be this bird 
        if (!FlockableBirdsNames.Contains(thatBird.Name)) return; // that bird don't flock wit this bird 
        if (!_nearbyBirdsTracker.NearbyBirds.Contains(thatBird)) return; // that bird ain't nearby 

        bool thatBirdJustLanded = 
            thatBirdNewState is ShelteredState ||
            thatBirdNewState is PerchedState ||
            thatBirdNewState is GroundedState;

        if (thatBirdJustLanded && BirdState is FlyingState) {
            Landing.SetLandingCircle(Landing.FlockLandingCircleRadius, thatBirdTargetPosition); // React by landing near that bird
            TransitionToState(Landing);
            return;
        }

        bool thisBirdIsLanded = 
            BirdState is ShelteredState ||
            BirdState is PerchedState ||
            BirdState is GroundedState;

        if ((thatBirdNewState is FlyingState || thatBirdNewState is FleeingState) && thisBirdIsLanded) {
            if (thatBirdNewState is FlyingState)
                TransitionToState(Flying);
            else if (thatBirdNewState is FleeingState)
                TransitionToState(Fleeing);
        }
    }
    
    public void TransitionToState(IBirdState newState)
    {
        if (newState == null) 
            Debug.LogError("Unexpected code path.");
        BirdState?.Exit(this);
        BirdState = newState;
        BirdState.Enter(this);
        if (_thisBird == null)
            Debug.LogError("thisbird");
        if (TargetPosition == null)
            Debug.LogError("targetpostion");
        if (newState == null)
            Debug.LogError("newstaet");
        BirdStateChanged(_thisBird, TargetPosition, newState);
    }

    private void SelfDestructIfExitedWorld() {
        if (!_worldBounds.Contains(transform.position)) {
            BirdDestroyed(GetComponent<Bird>());
            Destroy(gameObject);
        }
    }

    public bool TickAndCheckBehaviorTimer()
    {
        _behaviorElapsed += Time.deltaTime;
        bool _behaviorExpired = _behaviorElapsed >= BehaviorDuration;
        if (_behaviorExpired)
        {
            _behaviorElapsed = 0;
            return true;
        }
        else
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

    public void CheckIfFrightened()
    {
        return; // not implemented
        // if (TargetBirdSpot.AreBirdsFrightened())
        //     TransitionToState(Fleeing);
    }

    public Vector2 Seek(Vector2 target)
    {
        Vector2 _desired = (target - (Vector2)transform.position).normalized * FlightSpeedLimit;
        Vector2 _steer = _desired - _rb.velocity;
        if (_steer.magnitude >= _steerForceLimit)
            _steer = _steer.normalized * _steerForceLimit;

        return _steer;
    }

    private void MatchAnimationToFacingDirection()
    {
        transform.localScale = new Vector3(
                    _facingDirection == FacingDirection.West ?
                        Mathf.Abs(transform.localScale.x) :
                        -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);
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