using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FishBar : MonoBehaviour
{
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;

    [SerializeField] private SpriteRenderer _barSprite;
    [SerializeField] private GameObject _fishCursor;
    [SerializeField] private GameObject _triggersContainer;
    [SerializeField] private GameObject _fishBarTriggerPrefab;
    [SerializeField] private GameObject _fishTypeContainer;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _missedSFX;
    [SerializeField] private AudioClip _caughtSFX;
    [SerializeField] private AudioClip _hitTriggerSFX;
    [SerializeField] private AudioClip _reelingInSFX;

    private System.Action _stopReelingSFXCB;

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
    [SerializeField] private float _blinkDuration = 0.2f;

    [SerializeField] Logger _logger = new();

    private PlayerMovementController _playerMovementController;
    private Collider2D _playerCollider;
    private Inventory _inventory;
    private Collider2D _gameCursorCollider;
    private Rigidbody2D _gameCursorRB;
    private List<FishBarTrigger> _triggers;
    private bool _failed;
    private int _roundNumber;
    private Vector2 _gameCursorStartPosition = new Vector2(0f, 3.68658f); // Start position of cursor
    private FishType _fishType;
    
    // Fields below map the normalized trigger positions to the 
    // actual length of the fishbar play area, uses y = mx + b
    public static readonly float TRIGGER_MAPPING_SLOPE = 3.35f;
    public static readonly float TRIGGER_MAPPING_INTERCEPT = 0.35f;

    // Play the fishing mini game
    public void Play()
    {
        // Get references
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _playerCollider = GameObject.FindWithTag("Player").GetComponent<Collider2D>();
        _gameCursorCollider = _fishCursor.GetComponent<Collider2D>();
        _gameCursorRB = _fishCursor.GetComponent<Rigidbody2D>();

        InitializeNewGame();
        InitializeNewRound(_fishType.Rounds[_roundNumber]);
        StartCoroutine(PlayRound(_fishType.Rounds[_roundNumber].GameSpeed));
    }

    private void InitializeNewGame()
    {
        _logger.Info("New game started.");
        _stopReelingSFXCB = AudioManager.Instance.PlayLoopingSFX(_reelingInSFX, 0.2f);
        _playerMovementController.PlayerState.Value = PlayerStates.Catching;
        gameObject.SetActive(true); 
        _fishType = GetRandomValidFishType();
        _overlaySpriteRenderer.sprite = null;
        _failed = false;
        _roundNumber = 0;
    }

    private void InitializeNewRound(FishingRound round) 
    {
        _logger.Info($"Round {_roundNumber} start");

        // Reset game
        foreach (Transform _child in _triggersContainer.transform)
            Destroy(_child.gameObject);
        ResetFishCursor();

        // Configure next round
        _triggers = InstantiateTriggers(GenerateNormalizedTriggerPositions(round));
        if (round.HasOscillatingTriggers) 
            ConfigureTriggerOscillation(_triggers, round);
    }

    // Called from FishBarTrigger
    // Checks whether passed trigger was unfulfilled
    private void PassedFishBarTrigger(FishBarTrigger fishBarTrigger)
    {
        if (fishBarTrigger.Fulfilled == false)
            _failed = true;
    }

    private IEnumerator PlayRound(float duration)
    {
        // Gameplay loop
        float _time = 0f;
        while (_time < duration)
        {
            UpdateGameCursor(Mathf.InverseLerp(0, duration, _time));
            float _elapsed = Time.deltaTime;
            yield return new WaitForSeconds(_elapsed);
            _time += _elapsed;
            if (_failed)
                break;
        }
        
        // Check if failed
        if (_failed || _triggers.Any(trigger => trigger.Fulfilled == false)) {
            OnFail();
            yield break;
        }

        // Round won! Check if it was the last round
        _roundNumber++;
        if (_roundNumber >= _fishType.Rounds.Count) {
            OnGameWin();
            yield break;
        }

        // Move to next round
        InitializeNewRound(_fishType.Rounds[_roundNumber]);
        StartCoroutine(PlayRound(_fishType.Rounds[_roundNumber].GameSpeed));
    }

    // Move cursor and stretch fillbar to match
    public void UpdateGameCursor(float percent)
    {
        float _currHeight = Mathf.Lerp(_startHeight, _endHeight, percent);
        _barSprite.size = new Vector2(_barSprite.size.x, _currHeight);

        // TODO: it is supposedly bad to move the transform of a kinematic rigidbody like this.
        // - Using a RB is giving control of the position to Unity so it can handle collisions
        //   It's hopefully fine in this case since everything is a trigger and only overlap each other
        if (!_failed)
            _fishCursor.transform.localPosition = new Vector2(_fishCursor.transform.localPosition.x, _currHeight);
    }

    private void OnGameWin() {
        _playerMovementController.PlayerState.Value = PlayerStates.Celebrating; // controller will auto leave state after some itme
        AudioManager.Instance.PlaySFX(_caughtSFX);
        _inventory.AddItemOrDrop(_fishType.CaughtItem, 1, _playerCollider);
        _stopReelingSFXCB();
        _stopReelingSFXCB = null;
        EndGame();
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
        _stopReelingSFXCB();
        yield return new WaitForSeconds(1.5f);
        _playerMovementController.PlayerState.Value = PlayerStates.Idle;
        EndGame();
    }

    private void EndGame() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Determines result of keypress. Success, Hit, or Failed
    /// </summary>
    private void OnUseTool()
    {
        // ignore if lost already
        if (_failed)
            return;

        // Check for hit or miss
        // Checks for unfulfilled triggers under game cursor
        List<Collider2D> _results = new List<Collider2D>();
        _gameCursorCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _results);
        List<FishBarTrigger> _overlappedTriggers = _results
            .Select(collider => collider.GetComponent<FishBarTrigger>())
            .Where(trigger => trigger != null)
            .OrderBy(trigger => trigger.transform.position.y)
            .ToList();

        foreach(var trigger in _overlappedTriggers) {
            if (trigger.Fulfilled) 
                continue;
            // Yay, a hit!
            trigger.Fulfilled = true;
            AudioManager.Instance.PlaySFX(_hitTriggerSFX);
            return;
        }

        // Missed!
        _failed = true;
    }

    private void ResetFishCursor()
    {
        _gameCursorRB.transform.SetLocalPositionAndRotation(_gameCursorStartPosition, Quaternion.identity);
        _gameCursorRB.bodyType = RigidbodyType2D.Kinematic;
    }

    private void LaunchFishCursor()
    {
        _gameCursorRB.bodyType = RigidbodyType2D.Dynamic;
        _gameCursorRB.mass = _mass;

        Vector2 randomForce = new Vector2(Random.Range(-_forceStrength, _forceStrength), Random.Range(_forceStrength - 1, _forceStrength));
        _gameCursorRB.AddForceAtPosition(randomForce, (Vector2)_fishCursor.transform.position + (Random.insideUnitCircle * _positionRadius), ForceMode2D.Impulse);
        AudioManager.Instance.PlaySFX(_missedSFX);
    }

    /// <summary>
    /// Generates trigger positions for game.
    /// Ordered from bottom to top of gamebar 
    /// </summary>
    private float[] GenerateNormalizedTriggerPositions(FishingRound fishingRound)
    {
        float[] _triggerPositions = new float[fishingRound.NumberOfTriggers];

        bool _triggersTooClose;
        _triggerPositions[0] = 1.0f; // Trigger right at end

        switch (fishingRound.GameModifier)
        {
            // Generates triggers in singles spaced apart
            case FishingRound.StackedTriggerType.none:
                for (int i = 1; i < fishingRound.NumberOfTriggers; i++)
                {
                    _triggerPositions[i] = Random.Range(0.1f, 0.9f);
                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < i; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[i] - _triggerPositions[j]) < fishingRound.MinimumTriggerSpacing)
                            {
                                _triggerPositions[i] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);
                }
                break;

            // Generates triggers in pairs spaced apart
            case FishingRound.StackedTriggerType.doubles:
                int k = 1;
                if (fishingRound.NumberOfTriggers % 2 == 0)
                {
                    _triggerPositions[1] = 1.0f - fishingRound.StackedTriggerSpacing;
                    k++;
                }

                for (; k < fishingRound.NumberOfTriggers; k += 2)
                {
                    _triggerPositions[k] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < k; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[k] - _triggerPositions[j]) < fishingRound.MinimumTriggerSpacing)
                            {
                                _triggerPositions[k] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);

                    _triggerPositions[k + 1] = _triggerPositions[k] + fishingRound.StackedTriggerSpacing;
                }

                break;

            // generates a stack of triggers
            case FishingRound.StackedTriggerType.mega:
                _triggerPositions[1] = Random.Range(0.1f, 0.9f - fishingRound.StackedTriggerSpacing * fishingRound.NumberOfTriggers);
                for (int i = 1; i < fishingRound.NumberOfTriggers; i++)
                {
                    _triggerPositions[i + 1] = _triggerPositions[1] + fishingRound.StackedTriggerSpacing;
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
        foreach (Transform _child in _fishTypeContainer.transform)
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

    void ConfigureTriggerOscillation(List<FishBarTrigger> triggers, FishingRound fishingRound)
    {
        // Ends because the final index is the final trigger,
        // and the final trigger should not oscillate
        for (int i = 0; i < triggers.Count - 1; i++)
            triggers[i].InitalizeOscillation(fishingRound);
    }
}
