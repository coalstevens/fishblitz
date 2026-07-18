using System.Collections;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(EnemyHealth))]
public class ProjectileFiringEnemy : MonoBehaviour
{
    public enum FiringPattern
    {
        SingleShot,
        SpreadShot,
        ParallelSpread,
        BurstFire,
        AimedShot,
        RingShot
    }

    private enum EnemyState
    {
        Idle,
        Engaging,
        Firing
    }

    [Header("Firing")]
    [SerializeField] private FiringPattern _firingPattern = FiringPattern.SingleShot;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private float _projectileSpawnRadius = 0.5f;
    [SerializeField] private Vector2 _projectileSpawnOffset = Vector2.zero;

    [Header("Pattern: Spread")]
    [SerializeField] private int _spreadCount = 3;
    [SerializeField] private float _spreadAngle = 30f;

    [Header("Pattern: Parallel")]
    [SerializeField] private int _parallelCount = 3;
    [SerializeField] private float _parallelSpacing = 0.5f;

    [Header("Pattern: Burst")]
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstInterval = 0.1f;

    [Header("Pattern: Aimed")]
    [SerializeField] private float _chargeDuration = 0.8f;
    [SerializeField] private float _aimedSpeedMult = 2f;
    [SerializeField] private float _aimedDamageMult = 2f;

    [Header("Pattern: Ring")]
    [SerializeField] private int _ringCount = 8;

    [Header("Detection")]
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField] private float _detectionInterval = 0.5f;
    [SerializeField] private LayerMask _obstacleLayers;

    [Header("Movement")]
    [SerializeField] private float _idleSpeed = 1.5f;
    [SerializeField] private float _engageSpeed = 3f;
    [SerializeField] private float _wanderRadius = 5f;
    [SerializeField] private float _idleWaitMin = 1f;
    [SerializeField] private float _idleWaitMax = 3f;
    [SerializeField] private float _dangerDistance = 3f;
    [SerializeField] private float _safeDistance = 7f;

    [Header("Debug")]
    [SerializeField] private Logger _logger = new();

    private EnemyState _state = EnemyState.Idle;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private EnemyHealth _enemyHealth;
    private PlayerHurtbox _playerHurtbox;
    private Transform _playerTransform;

    private Vector2 _wanderTarget;
    private bool _isWaiting;
    private Coroutine _waitCoroutine;

    private float _lastDetectionTime;
    private float _lastFireTime;
    private bool _isFiringActive;
    private Coroutine _activeFireCoroutine;

    private float _lastHealth;
    private bool _aggressive;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _lastHealth = _enemyHealth.CurrentHealth.Value;
        _enemyHealth.CurrentHealth.OnChange(OnHealthChanged);

        _originalColor = _spriteRenderer.color;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(playerObj);
        _playerHurtbox = playerObj.GetComponentInChildren<PlayerHurtbox>();
        Assert.IsNotNull(_playerHurtbox);
        _playerTransform = _playerHurtbox.transform;

        PickNewWanderTarget();
    }

    private void Update()
    {
        HandleDetection();

        switch (_state)
        {
            case EnemyState.Firing:
                HandleFiring();
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (_state)
        {
            case EnemyState.Idle:
                UpdateIdleMovement();
                break;
            case EnemyState.Engaging:
                UpdateEngagingMovement();
                break;
            case EnemyState.Firing:
                UpdateFiringMovement();
                break;
        }
    }

    private void HandleDetection()
    {
        if (Time.time - _lastDetectionTime < _detectionInterval)
            return;

        _lastDetectionTime = Time.time;

        if (_playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance <= _viewRadius)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, _playerTransform.position, _obstacleLayers);
            if (!hit)
            {
                if (_state == EnemyState.Idle)
                {
                    _state = EnemyState.Engaging;
                    _aggressive = true;
                    StopIdleWait();
                    _logger.Info("Changed state: Idle -> Engaging (detected player)");
                }
            }
        }
        else if (_state != EnemyState.Idle && !_aggressive)
        {
            _state = EnemyState.Idle;
            StopActiveFireCoroutine();
            PickNewWanderTarget();
            _logger.Info("Changed state: Engaging/Firing -> Idle (player lost)");
        }
    }

    private void UpdateIdleMovement()
    {
        if (_isWaiting)
            return;

        Vector2 currentPos = (Vector2)transform.position;
        float distance = Vector2.Distance(currentPos, _wanderTarget);

        if (distance < 0.3f)
        {
            if (_waitCoroutine == null)
                _waitCoroutine = StartCoroutine(IdleWaitCoroutine());
            return;
        }

        Vector2 direction = (_wanderTarget - currentPos).normalized;
        _rb.linearVelocity = direction * _idleSpeed;
    }

    private IEnumerator IdleWaitCoroutine()
    {
        _isWaiting = true;
        _rb.linearVelocity = Vector2.zero;

        float waitTime = Random.Range(_idleWaitMin, _idleWaitMax);
        yield return new WaitForSeconds(waitTime);

        PickNewWanderTarget();
        _isWaiting = false;
        _waitCoroutine = null;
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * _wanderRadius;
        _wanderTarget = (Vector2)transform.position + randomOffset;
    }

    private void StopIdleWait()
    {
        _isWaiting = false;
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }
    }

    private void UpdateEngagingMovement()
    {
        if (_playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance <= _safeDistance && distance >= _dangerDistance)
        {
            _state = EnemyState.Firing;
            _rb.linearVelocity = Vector2.zero;
            _lastFireTime = Time.time;
            _logger.Info("Changed state: Engaging -> Firing (in range)");
            return;
        }

        Vector2 direction;
        if (distance < _dangerDistance)
            direction = ((Vector2)transform.position - (Vector2)_playerTransform.position).normalized;
        else
            direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;

        _rb.linearVelocity = direction * _engageSpeed;
    }

    private void UpdateFiringMovement()
    {
        _rb.linearVelocity = Vector2.zero;

        if (_playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance < _dangerDistance)
        {
            _state = EnemyState.Engaging;
            StopActiveFireCoroutine();
            _logger.Info("Changed state: Firing -> Engaging (too close)");
        }
        else if (distance > _safeDistance)
        {
            _state = EnemyState.Engaging;
            StopActiveFireCoroutine();
            _logger.Info("Changed state: Firing -> Engaging (too far)");
        }
    }

    private void HandleFiring()
    {
        if (_isFiringActive)
            return;

        if (Time.time - _lastFireTime < _fireRate)
            return;

        ExecuteFiringPattern();
    }

    private void ExecuteFiringPattern()
    {
        _lastFireTime = Time.time;

        switch (_firingPattern)
        {
            case FiringPattern.SingleShot:
                FireSingleShot();
                break;
            case FiringPattern.SpreadShot:
                FireSpreadShot();
                break;
            case FiringPattern.ParallelSpread:
                FireParallelSpread();
                break;
            case FiringPattern.BurstFire:
                _activeFireCoroutine = StartCoroutine(BurstFireCoroutine());
                break;
            case FiringPattern.AimedShot:
                _activeFireCoroutine = StartCoroutine(AimedShotCoroutine());
                break;
            case FiringPattern.RingShot:
                FireRingShot();
                break;
        }
    }

    private void StopActiveFireCoroutine()
    {
        if (_activeFireCoroutine != null)
        {
            StopCoroutine(_activeFireCoroutine);
            _activeFireCoroutine = null;
            _isFiringActive = false;
            _spriteRenderer.color = _originalColor;
        }
    }

    private Vector2 GetDirectionToPlayer()
    {
        return ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
    }

    private void SpawnAndLaunchProjectile(Vector2 direction, float speedMult = 1f, float damageMult = 1f)
    {
        Vector2 spawnPos = (Vector2)transform.position + direction * _projectileSpawnRadius + _projectileSpawnOffset;
        GameObject obj = ObjectPooling.SpawnObject(_projectilePrefab, spawnPos, Quaternion.identity);
        obj.transform.localRotation = Quaternion.FromToRotation(Vector2.left, direction);

        Projectile projectile = obj.GetComponent<Projectile>();
        projectile.Launch(direction, speedMult);

        if (damageMult > 1f)
        {
            ContactHitbox hitbox = obj.GetComponentInChildren<ContactHitbox>();
            if (hitbox != null)
                hitbox.SetDamage(hitbox.Damage * damageMult);
        }
    }

    private void FireSingleShot()
    {
        SpawnAndLaunchProjectile(GetDirectionToPlayer());
    }

    private void FireSpreadShot()
    {
        Vector2 baseDirection = GetDirectionToPlayer();
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - _spreadAngle / 2f;
        float angleStep = _spreadCount > 1 ? _spreadAngle / (_spreadCount - 1) : 0f;

        for (int i = 0; i < _spreadCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnAndLaunchProjectile(dir);
        }
    }

    private void FireParallelSpread()
    {
        Vector2 baseDirection = GetDirectionToPlayer();
        Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);
        float totalWidth = (_parallelCount - 1) * _parallelSpacing;
        Vector2 startPos = (Vector2)transform.position - perpendicular * (totalWidth / 2f);

        for (int i = 0; i < _parallelCount; i++)
        {
            Vector2 spawnPos = startPos + perpendicular * (_parallelSpacing * i);
            GameObject obj = ObjectPooling.SpawnObject(_projectilePrefab, spawnPos, Quaternion.identity);
            obj.transform.localRotation = Quaternion.FromToRotation(Vector2.left, baseDirection);

            Projectile projectile = obj.GetComponent<Projectile>();
            projectile.Launch(baseDirection);
        }
    }

    private IEnumerator BurstFireCoroutine()
    {
        _isFiringActive = true;

        for (int i = 0; i < _burstCount; i++)
        {
            SpawnAndLaunchProjectile(GetDirectionToPlayer());
            yield return new WaitForSeconds(_burstInterval);
        }

        _isFiringActive = false;
        _activeFireCoroutine = null;
    }

    private IEnumerator AimedShotCoroutine()
    {
        _isFiringActive = true;
        _spriteRenderer.color = Color.red;
        _rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(_chargeDuration);

        SpawnAndLaunchProjectile(GetDirectionToPlayer(), _aimedSpeedMult, _aimedDamageMult);

        _spriteRenderer.color = _originalColor;
        _isFiringActive = false;
        _activeFireCoroutine = null;
    }

    private void FireRingShot()
    {
        float angleStep = 360f / _ringCount;

        for (int i = 0; i < _ringCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnAndLaunchProjectile(dir);
        }
    }

    private void OnHealthChanged(float newHealth)
    {
        if (newHealth < _lastHealth && !_aggressive)
        {
            _aggressive = true;
            if (_state == EnemyState.Idle)
            {
                StopIdleWait();
                _state = EnemyState.Engaging;
                _logger.Info("Changed state: Idle -> Engaging (took damage)");
            }
        }
        _lastHealth = newHealth;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _safeDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _dangerDistance);
    }
}
