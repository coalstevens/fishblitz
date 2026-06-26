using NUnit.Framework;
using UnityEngine;

public class FlyingChaser : MonoBehaviour
{
    private enum ChaserState { Wandering, Chasing }

    [Header("Wander")]
    [SerializeField] private float _wanderSpeed = 2f;
    [SerializeField] private float _wanderSteerForce = 1.5f;
    [SerializeField] private float _wanderRingDistance = 3f;
    [SerializeField] private float _wanderRingRadius = 2f;
    [SerializeField] private float _wanderForceUpdateInterval = 1f;

    [Header("Chase")]
    [SerializeField] private float _chaseSpeed = 5f;
    [SerializeField] private float _chaseSteerForce = 3f;

    [Header("Detection")]
    [SerializeField] private float _viewRadius = 8f;
    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private float _circleCastRadius = 0.5f;
    [SerializeField] private float _circleCastRange = 2f;
    [SerializeField] private float _avoidanceWeight = 5f;

    [Header("Combat")]
    [SerializeField] private float _detectionInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private Logger _logger = new();

    private ChaserState _state = ChaserState.Wandering;
    private Vector2 _wanderTarget;
    private Vector2 _wanderRingCenter;
    private Vector2 _wanderForce;
    private Vector2 _avoidForce;
    private Vector2 _gizAvoidTarget;
    private float _lastWanderForceUpdateTime;
    private float _lastDetectionTime;

    private Rigidbody2D _rb;
    private PlayerHurtbox _playerHurtbox;
    private EnemyHealth _enemyHealth;
    private LayerMask _interactionLayers;
    private float _lastHealth;
    private bool _aggressive;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rb);

        _enemyHealth = GetComponent<EnemyHealth>();
        Assert.IsNotNull(_enemyHealth);
        _lastHealth = _enemyHealth.CurrentHealth.Value;
        _enemyHealth.CurrentHealth.OnChange(OnHealthChanged);
    }

    private Transform GetPlayer()
    {
        if (_playerHurtbox == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Assert.IsNotNull(playerObj);
            _playerHurtbox = playerObj.GetComponentInChildren<PlayerHurtbox>();
            Assert.IsNotNull(_playerHurtbox);
        }
        return _playerHurtbox.transform;
    }

    private void Start()
    {
        _wanderTarget = transform.position;
        _interactionLayers = SteeringForces.GetInteractionLayers(gameObject, _rb);
    }

    private void Update()
    {
        Transform player = GetPlayer();

        if (Time.time - _lastDetectionTime >= _detectionInterval)
        {
            _lastDetectionTime = Time.time;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= _viewRadius)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, _obstacleLayers);
                if (!hit && _state != ChaserState.Chasing)
                {
                    _state = ChaserState.Chasing;
                    _aggressive = true;
                    _logger.Info("Changed state: Wandering -> Chasing (detected player)");
                }
            }
            else if (_state != ChaserState.Wandering && !_aggressive)
            {
                _state = ChaserState.Wandering;
                _logger.Info("Changed state: Chasing -> Wandering");
            }
        }
    }

    private void FixedUpdate()
    {
        switch (_state)
        {
            case ChaserState.Wandering:
                UpdateWandering();
                break;
            case ChaserState.Chasing:
                UpdateChasing();
                break;
        }
    }

    private void UpdateWandering()
    {
        if (Time.time - _lastWanderForceUpdateTime >= _wanderForceUpdateInterval)
        {
            _lastWanderForceUpdateTime = Time.time;
            _wanderForce = SteeringForces.CalculateWanderForce(
                _rb.position,
                _rb.linearVelocity,
                _wanderSpeed,
                _wanderSteerForce,
                _wanderRingDistance,
                _wanderRingRadius,
                ref _wanderTarget,
                out _wanderRingCenter);
        }

        _avoidForce = SteeringForces.CalculateAvoidanceForce(
            _rb.position,
            _rb.linearVelocity,
            _circleCastRadius,
            _circleCastRange,
            _avoidanceWeight,
            _interactionLayers,
            out _gizAvoidTarget);

        _rb.AddForce(_wanderForce + _avoidForce);
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, _wanderSpeed);
    }

    private void UpdateChasing()
    {
        Vector2 seekForce = SteeringForces.Seek(
            _rb.position,
            _rb.linearVelocity,
            _playerHurtbox.transform.position,
            _chaseSpeed,
            _chaseSteerForce);

        _avoidForce = SteeringForces.CalculateAvoidanceForce(
            _rb.position,
            _rb.linearVelocity,
            _circleCastRadius,
            _circleCastRange,
            _avoidanceWeight,
            _interactionLayers,
            out _gizAvoidTarget);

        _rb.AddForce(seekForce + _avoidForce);
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, _chaseSpeed);
    }

    private void OnHealthChanged(float newHealth)
    {
        if (newHealth < _lastHealth && !_aggressive)
        {
            _aggressive = true;
            _state = ChaserState.Chasing;
            _logger.Info("Changed state: * -> Chasing (took damage)");
        }
        _lastHealth = newHealth;
    }

    private void OnDrawGizmos()
    {
        Vector2 origin = transform.position;
        float visualScaling = 5f;
        float dotSize = 0.1f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, _viewRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_wanderRingCenter, _wanderRingRadius);
        Gizmos.DrawSphere(_wanderTarget, dotSize);
        Gizmos.DrawLine(origin, origin + _wanderForce * visualScaling);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + _avoidForce * visualScaling);
        Gizmos.DrawSphere(_gizAvoidTarget, dotSize);
    }
}
