using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ReactiveUnity;
using UnityEngine.SceneManagement;

public class FishBar : MonoBehaviour
{
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;
    [SerializeField] private SpriteRenderer _barSprite;
    [SerializeField] private GameObject _fishCursor;
    [SerializeField] private GameObject _triggersContainer;
    [SerializeField] private GameObject _fishBarTriggerPrefab;
    [SerializeField] private GameObject _fishContainer;
    [SerializeField] private PlayerSoundController _playerSoundController;

    [Header("Shake Options")]
    [SerializeField] private float _shakeDuration = 1;
    [SerializeField] private float _shakeStrength = 1;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90;

    [Header("Fish Indicator Options")]
    [SerializeField] private float _forceStrength = 2f;
    [SerializeField] private float _positionRadius = 1f;
    [SerializeField] private float _mass = 1f;

    [Header("Overlay Options")]
    [SerializeField] private SpriteRenderer _overlaySpriteRenderer; // Highlights all of fish bar
    [SerializeField] private Sprite _greyOverlay;
    [SerializeField] private Sprite _redOverlay;
    [SerializeField] private float _blinkDuration = 0.1f;

    [SerializeField] Logger _logger = new();

    private PlayerMovementController _playerMovementController;
    private Rigidbody2D _fishObjectRb;
    private Collider2D _indicatorCollider;
    private List<FishBarTrigger> _triggers;

    private int _triggerIndex = 0;
    private Reactive<bool> _failed = new Reactive<bool>(false);
    private Vector2 _originalFishCursorPos;
    private bool _won = false;
    private FishType _fishType;
    private Inventory _inventory;
    
    // Fields below map the normalized trigger positions to the 
    // actual length of the fishbar play area, uses y = mx + b
    public static readonly float TRIGGER_MAPPING_SLOPE = 3.35f;
    public static readonly float TRIGGER_MAPPING_INTERCEPT = 0.35f;

    private void Awake()
    {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _failed.When((prev, curr) => curr, (prev, curr) => OnFail());
        _originalFishCursorPos = _fishCursor.transform.localPosition;
    }

    // Play the fish bar game
    public void Play()
    {
        _logger.Info("New game started.");
        InitializeNewGame();
        StartCoroutine(PlayRoutine(_fishType.GameSpeed));
    }

    private void InitializeNewGame()
    {
        gameObject.SetActive(true);

        // Get references
        _playerSoundController = GameObject.FindWithTag("PlayerSounds").GetComponent<PlayerSoundController>();
        _indicatorCollider = _fishCursor.GetComponent<Collider2D>();
        _fishObjectRb = _fishCursor.GetComponent<Rigidbody2D>();

        // Reset Game
        _triggerIndex = 0;
        foreach (Transform _child in _triggersContainer.transform)
            Destroy(_child.gameObject);
        ResetFishCursor();
        _overlaySpriteRenderer.sprite = null;
        _failed.Value = false;
        _won = false;

        // Configure new game
        _fishType = GetRandomValidFishType();
        _triggers = InstantiateTriggers(GenerateNormalizedTriggerPositions(_fishType));
        if (_fishType.HasOscillatingTriggers) ConfigureTriggerOscillation(_triggers, _fishType);
    }

    private void OnFail()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
        StartCoroutine(FailRoutine());
    }

    private IEnumerator FailRoutine()
    {
        // Blink fishbar red/grey twice
        _overlaySpriteRenderer.sprite = _redOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _greyOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _redOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _greyOverlay;

        LaunchFishCursor();
    }

    private void ResetFishCursor()
    {
        _fishObjectRb.transform.SetLocalPositionAndRotation(_originalFishCursorPos, Quaternion.identity);
        _fishObjectRb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void LaunchFishCursor()
    {
        _fishObjectRb.bodyType = RigidbodyType2D.Dynamic;
        _fishObjectRb.mass = _mass;

        Vector2 randomForce = new Vector2(Random.Range(-_forceStrength, _forceStrength), Random.Range(_forceStrength - 1, _forceStrength));
        _fishObjectRb.AddForceAtPosition(randomForce, (Vector2)_fishCursor.transform.position + (Random.insideUnitCircle * _positionRadius), ForceMode2D.Impulse);
        _playerSoundController.PlaySound("Missed");
    }

    // Called from FishBarTrigger
    private void PassedFishBarTrigger(FishBarTrigger fishBarTrigger)
    {
        // returns if final trigger was hit successfully
        if (_won)
        {
            return;
        }

        // if the next trigger to hit is the one just passed, it was missed
        if (GetNextTrigger() == fishBarTrigger)
        {
            _failed.Value = true;
        }
    }

    // Returns next trigger player is supposed to hit
    private FishBarTrigger GetNextTrigger()
    {
        int idx = _triggerIndex >= _triggers.Count ? _triggers.Count - 1 : _triggerIndex;
        return _triggers[idx];
    }

    // Move cursor and stretch fillbar to match
    public void SetCompletion(float percent)
    {
        float _currHeight = Mathf.Lerp(_startHeight, _endHeight, percent);
        _barSprite.size = new Vector2(_barSprite.size.x, _currHeight);

        // TODO: it is supposedly bad to move the transform of a kinematic rigidbody like this.
        // - Using a RB is giving control of the position to Unity so it can handle collisions
        //   It's hopefully fine in this case since everything is a trigger and only overlap each other
        if (!_failed.Value)
        {
            _fishCursor.transform.localPosition = new Vector2(_fishCursor.transform.localPosition.x, _currHeight);
        }
    }

    private IEnumerator PlayRoutine(float duration)
    {
        // Start
        _playerMovementController.PlayerState.Value = PlayerStates.Catching;
        float _time = 0f;

        // Move cursor and stretch fillbar
        while (_time < duration)
        {
            SetCompletion(Mathf.InverseLerp(0, duration, _time));
            float _elapsed = Time.deltaTime;
            yield return new WaitForSeconds(_elapsed);
            _time += _elapsed;
        }

        // Check result
        if (_failed.Value)
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Idle;
        }
        else
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Celebrating;
            _playerSoundController.PlaySound("Caught");
            // TODO: Add fish item to inventory, make fish items
        }

        // End game
        gameObject.SetActive(false);
    }


    /// <summary>
    /// Determines result of keypress. Success, Hit, or Failed
    /// </summary>
    private void OnUseTool()
    {
        // ignore if lost already
        if (_failed.Value)
        {
            return;
        }

        // Check for hit or miss
        FishBarTrigger _next = GetNextTrigger();
        List<Collider2D> _results = new List<Collider2D>();
        _indicatorCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _results);
        if (_results.Contains(_next.GetCollider()))
        {
            // yay! a hit!
            _next.SetSprite(true);
            _triggerIndex += 1;

            // That was the last one. we won!
            if (_triggerIndex == _triggers.Count)
            {
                _won = true;
            }
        }
        else
        {
            // Missed! Pressed over nothing or wrong trigger
            _failed.Value = true;
        }
    }

    /// <summary>
    /// Generates trigger positions for game.
    /// Ordered from bottom to top.
    /// </summary>
    private float[] GenerateNormalizedTriggerPositions(FishType fishType)
    {
        float[] _triggerPositions = new float[fishType.NumberOfTriggers];

        bool _triggersTooClose;
        _triggerPositions[0] = 1.0f; // Trigger right at end

        switch (fishType.GameModifier)
        {
            // Generates triggers in singles spaced apart
            case FishType.StackedTriggerType.none:
                for (int i = 1; i < fishType.NumberOfTriggers; i++)
                {
                    _triggerPositions[i] = Random.Range(0.1f, 0.9f);
                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < i; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[i] - _triggerPositions[j]) < fishType.MinimumTriggerSpacing)
                            {
                                _triggerPositions[i] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);
                }
                break;

            // Generates triggers in pairs spaced apart
            case FishType.StackedTriggerType.doubles:
                int k = 1;
                if (fishType.NumberOfTriggers % 2 == 0)
                {
                    _triggerPositions[1] = 1.0f - fishType.StackedTriggerSpacing;
                    k++;
                }

                for (; k < fishType.NumberOfTriggers; k += 2)
                {
                    _triggerPositions[k] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < k; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[k] - _triggerPositions[j]) < fishType.MinimumTriggerSpacing)
                            {
                                _triggerPositions[k] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);

                    _triggerPositions[k + 1] = _triggerPositions[k] + fishType.StackedTriggerSpacing;
                }

                break;

            // generates a stack of triggers
            case FishType.StackedTriggerType.mega:
                _triggerPositions[1] = Random.Range(0.1f, 0.9f - fishType.StackedTriggerSpacing * fishType.NumberOfTriggers);
                for (int i = 1; i < fishType.NumberOfTriggers; i++)
                {
                    _triggerPositions[i + 1] = _triggerPositions[1] + fishType.StackedTriggerSpacing;
                }
                break;
        }

        System.Array.Sort(_triggerPositions);
        _logger.Info("Trigger Normalized Positions: " + Logger.FloatArrayToString(_triggerPositions));
        return _triggerPositions;
    }


    private List<FishBarTrigger> InstantiateTriggers(float[] positions)
    {
        List<FishBarTrigger> _fishBarTriggers = new();
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 _localPosition = new Vector3(0, positions[i] * TRIGGER_MAPPING_SLOPE + TRIGGER_MAPPING_INTERCEPT, 0);
            var _fishBarTrigger = Instantiate(_fishBarTriggerPrefab, transform.position + _localPosition, Quaternion.identity, _triggersContainer.transform);
            _fishBarTriggers.Add(_fishBarTrigger.GetComponent<FishBarTrigger>());
        }

        _logger.Info("Number of triggers instantiated: " + _fishBarTriggers.Count);
        return _fishBarTriggers;
    }

    private FishType GetRandomValidFishType()
    {
        FishType _fish;
        List<FishType> _fishes = new List<FishType>();
        foreach (Transform _child in _fishContainer.transform)
        {
            _fish = _child.GetComponent<FishType>();
            if (_fish.CatchableSceneNames.Contains(SceneManager.GetActiveScene().name))
            {
                _fishes.Add(_fish);
            }
        }
        if (_fishes.Count == 0) Debug.LogError("Did not find any fish types");
        _logger.Info("Valid fishes in area: " + Logger.GetGameObjectNames<FishType>(_fishes.ToArray()));
        _fish = _fishes[Random.Range(0, _fishes.Count)];
        _logger.Info("Selected fish: " + _fish.gameObject.name);
        return _fish;
    }

    void ConfigureTriggerOscillation(List<FishBarTrigger> triggers, FishType fishType)
    {
        // Ends because the final index is the final trigger,
        // and the final trigger should not oscillate
        for (int i = 0; i < triggers.Count - 1; i++)
            triggers[i].InitalizeOscillation(fishType);
    }
}
