
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ReactiveUnity;

public class FishBar : MonoBehaviour
{
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;
    [SerializeField] private SpriteRenderer _barSprite;
    [SerializeField] private GameObject _fishSpriteObject;
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
    [SerializeField] private SpriteRenderer _overlaySpriteRenderer;
    [SerializeField] private Sprite _greyOverlay;
    [SerializeField] private Sprite _redOverlay;
    [SerializeField] private float _blinkDuration = 0.1f;

    private PlayerMovementController _playerMovementController;
    private Rigidbody2D _fishObjectRb;
    private Collider2D _indicatorCollider;
    private List<FishBarTrigger> _fishBarTriggers = new List<FishBarTrigger>();
    
    private int _triggerIdx = 0;
    private Reactive<bool> _failed = new Reactive<bool>(false);
    private Vector2 _originalFishObjectPos;
    private bool _won = false;
    private Fish _fish;
    private Inventory _inventory;

    private void Awake()
    {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _failed.When((prev, curr) => curr, (prev, curr) => OnFail());
        _originalFishObjectPos = _fishSpriteObject.transform.localPosition;
    }

    private void OnFail()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
        StartCoroutine(FailRoutine());
    }

    private IEnumerator FailRoutine()
    {
        _overlaySpriteRenderer.sprite = _redOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _greyOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _redOverlay;
        yield return new WaitForSeconds(_blinkDuration);
        _overlaySpriteRenderer.sprite = _greyOverlay;
        AddFishForce();
    }

    private void ResetFishObject()
    {
        _fishObjectRb.transform.localPosition = _originalFishObjectPos;
        _fishObjectRb.transform.localRotation = Quaternion.identity;
        _fishObjectRb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void AddFishForce()
    {
        _fishObjectRb.bodyType = RigidbodyType2D.Dynamic;
        _fishObjectRb.mass = _mass;

        Vector2 randomForce = new Vector2(Random.Range(-_forceStrength, _forceStrength), Random.Range(_forceStrength - 1, _forceStrength));
        _fishObjectRb.AddForceAtPosition(randomForce, (Vector2)_fishSpriteObject.transform.position + (Random.insideUnitCircle * _positionRadius), ForceMode2D.Impulse);
        _playerSoundController.PlaySound("Missed");
    }

    // Called from FishBarTrigger
    private void PassedFishBarTrigger(FishBarTrigger fishBarTrigger)
    {
        if (_won)
        {
            return;
        }

        if (GetNextTrigger() == fishBarTrigger)
        {
            _failed.Value = true;
        }
    }

    private void Initialize()
    {
        gameObject.SetActive(true);
        _triggerIdx = 0;
        float[] _triggerPositions;  
        

        foreach (Transform _child in _triggersContainer.transform)
        {
            Destroy(_child.gameObject);
        }

        _fish = GetRandomValidFish();
        _triggerPositions = GenerateNormalizedTriggerPositions(_fish);
        GenerateTriggers(_triggerPositions);

        _playerSoundController = GameObject.FindWithTag("PlayerSounds").GetComponent<PlayerSoundController>();
        _indicatorCollider = _fishSpriteObject.GetComponent<Collider2D>();
        _fishObjectRb = _fishSpriteObject.GetComponent<Rigidbody2D>();
        ResetFishObject();
        _overlaySpriteRenderer.sprite = null;
        _failed.Value = false;
        _won = false;
    }

    private FishBarTrigger GetNextTrigger()
    {
        int idx = _triggerIdx >= _fishBarTriggers.Count ? _fishBarTriggers.Count - 1 : _triggerIdx;
        return _fishBarTriggers[idx];
    }

    public void SetCompletion(float percent)
    {
        float _currHeight = Mathf.Lerp(_startHeight, _endHeight, percent);
        _barSprite.size = new Vector2(_barSprite.size.x, _currHeight);

        // TODO: it is supposedly bad to move the transform of a kinematic rigidbody like this.
        if (!_failed.Value)
        {
            _fishSpriteObject.transform.localPosition = new Vector2(_fishSpriteObject.transform.localPosition.x, _currHeight);
        }
    }

    // Play the fish bar game.
    // duration: time it takes for the progress bar to fill up to full
    public void Play()
    {
        Initialize();
        StartCoroutine(PlayRoutine(_fish.PlayDuration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        _playerMovementController.PlayerState.Value = PlayerStates.Catching;
        float _time = 0;
        while (_time < duration)
        {
            SetCompletion(Mathf.InverseLerp(0, duration, _time));
            float _elapsed = Time.deltaTime;
            yield return new WaitForSeconds(_elapsed);
            _time += _elapsed;
        }

        // If you didn't fail, you get a coin
        if (!_failed.Value)
        {
            _inventory.Gold += 1;
            _playerMovementController.PlayerState.Value = PlayerStates.Celebrating;
            _playerSoundController.PlaySound("Caught");
        }
        else
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Idle;
        }

        gameObject.SetActive(false);
    }

    private void BackToIdle()
    {
        _playerMovementController.PlayerState.Value = PlayerStates.Idle;
    }

    private void OnUseTool()
    {
        if (_failed.Value)
        {
            return;
        }

        FishBarTrigger _next = GetNextTrigger();
        List<Collider2D> _results = new List<Collider2D>();

        _indicatorCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _results);
        if (_results.Contains(_next.GetCollider()))
        {
            // yay!
            _next.SetSprite(true);
            _triggerIdx += 1;

            // That was the last one. we won!
            if (_triggerIdx == _fishBarTriggers.Count)
            {
                _won = true;
            }
        }
        else
        {
            _failed.Value = true;
        }
    }

    private float[] GenerateNormalizedTriggerPositions(Fish fish)
    {
        float[] _triggerPositions = new float[fish.NumTriggers];

        bool _triggersTooClose;
        _triggerPositions = new float[fish.NumTriggers];
        _triggerPositions[0] = 1.0f; // Trigger right at end

        switch (fish.gameModifier)
        {
            case Fish.modifier.normal:
                for (int i = 1; i < fish.NumTriggers; i++)
                {
                    _triggerPositions[i] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < i; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[i] - _triggerPositions[j]) < fish.MinimumTriggerGap)
                            {
                                _triggerPositions[i] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);
                }
                break;

            case Fish.modifier.doubles:
                int k = 1;
                if (fish.NumTriggers % 2 == 0)
                {
                    _triggerPositions[1] = 1.0f - fish.SpecialGap;
                    k++;
                }

                for (; k < fish.NumTriggers; k += 2)
                {
                    _triggerPositions[k] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        _triggersTooClose = false;
                        for (int j = 0; j < k; j++)
                        {
                            if (Mathf.Abs(_triggerPositions[k] - _triggerPositions[j]) < fish.MinimumTriggerGap)
                            {
                                _triggerPositions[k] = Random.Range(0.1f, 0.9f);
                                _triggersTooClose = true;
                            }
                        }
                    } while (_triggersTooClose);

                    _triggerPositions[k + 1] = _triggerPositions[k] + fish.SpecialGap;
                }

                break;

            case Fish.modifier.mega:
                _triggerPositions[1] = Random.Range(0.1f, 0.9f - fish.SpecialGap * fish.NumTriggers);
                for (int i = 1; i < fish.NumTriggers; i++)
                {
                    _triggerPositions[i + 1] = _triggerPositions[1] + fish.SpecialGap;
                }
                break;
        }

        System.Array.Sort(_triggerPositions);
        return _triggerPositions;
    }
    private void GenerateTriggers(float[] positions) {
        _fishBarTriggers.Clear();
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 _localPosition = new Vector3(0, positions[i] * 3.35f + 0.35f, 0);
            var _fishBarTrigger = Instantiate(_fishBarTriggerPrefab, transform.position + _localPosition, Quaternion.identity, _triggersContainer.transform);
            _fishBarTriggers.Add(_fishBarTrigger.GetComponent<FishBarTrigger>());
        }
       
        foreach (FishBarTrigger _trigger in _fishBarTriggers)
        {
            _trigger.Initialize();
            _trigger.SetSprite(false);
        }
    }
    
    private Fish GetRandomValidFish() {
        Fish _fish;
        List<Fish> _fishes = new List<Fish>();

        foreach (Transform _child in _fishContainer.transform) {
            _fish = _child.GetComponent<Fish>();
            if (_fish.validSceneName == gameObject.scene.name) 
            {
                _fishes.Add(_fish);
            }
        }
        
        return _fishes[Random.Range(0, _fishes.Count)];
    }
}
