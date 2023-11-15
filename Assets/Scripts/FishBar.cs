
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FishBar : MonoBehaviour
{
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;
    [SerializeField] private SpriteRenderer _barSprite;
    [SerializeField] private GameObject _fishSpriteObject;
    [SerializeField] private GameObject _triggersContainer;
    [SerializeField] private GameObject _fishBarTriggerPrefab;
    [SerializeField] private float _playDuration = 5f;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private playerSoundController _playerSoundController;


    [Header("Challenge Setup")]

    // [SerializeField] private float duration;
    [SerializeField] private int numTriggers; // always a trigger press at the end 
    [SerializeField] private float minimumTriggerGap;
    [SerializeField] private float specialGap;

    enum modifier { normal, doubles, triples, mega };
    [SerializeField] private modifier gameModifier;

    private float[] triggerPositions;


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

    private Rigidbody2D _fishObjectRb;
    private Collider2D _indicatorCollider;
    private List<FishBarTrigger> _fishBarTriggers = new List<FishBarTrigger>();
    private int _triggerIdx = 0;
    private Reactive<bool> _failed = new Reactive<bool>(false);
    private Vector2 _originalFishObjectPos;
    private bool _won = false;

    private void Awake()
    {
        _failed.When(curr => curr, (prev, curr) => OnFail());
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
            _failed.Set(true);
        }
    }

    private void Initialize()
    {
        gameObject.SetActive(true);
        _triggerIdx = 0;

        foreach (Transform child in _triggersContainer.transform)
        {
            Destroy(child.gameObject);
        }
        generateTriggerPositions();
        System.Array.Sort(triggerPositions);
        _fishBarTriggers.Clear();

        for (int i = 0; i < triggerPositions.Length; i++)
        {
            Vector3 localPosition = new Vector3(0, triggerPositions[i] * 3.35f + 0.35f, 0);
            var _fishBarTrigger = Instantiate(_fishBarTriggerPrefab, transform.position + localPosition, Quaternion.identity, _triggersContainer.transform);
            _fishBarTriggers.Add(_fishBarTrigger.GetComponent<FishBarTrigger>());
        }

        _playerSoundController = GameObject.FindWithTag("PlayerSounds").GetComponent<playerSoundController>();
        _indicatorCollider = _fishSpriteObject.GetComponent<Collider2D>();
        _fishObjectRb = _fishSpriteObject.GetComponent<Rigidbody2D>();
        ResetFishObject();
        _overlaySpriteRenderer.sprite = null;
        _failed.Set(false);
        _won = false;
        foreach (FishBarTrigger trigger in _fishBarTriggers)
        {
            trigger.Initialize();
            trigger.SetSprite(false);
        }
    }

    private FishBarTrigger GetNextTrigger()
    {
        int idx = _triggerIdx >= _fishBarTriggers.Count ? _fishBarTriggers.Count - 1 : _triggerIdx;
        return _fishBarTriggers[idx];
    }

    public void SetCompletion(float percent)
    {
        float currHeight = Mathf.Lerp(_startHeight, _endHeight, percent);
        _barSprite.size = new Vector2(_barSprite.size.x, currHeight);

        // TODO: it is supposedly bad to move the transform of a kinematic rigidbody like this.
        if (!_failed.Get())
        {
            _fishSpriteObject.transform.localPosition = new Vector2(_fishSpriteObject.transform.localPosition.x, currHeight);
        }
    }

    // Play the fish bar game.
    // duration: time it takes for the progress bar to fill up to full
    public void Play()
    {
        Initialize();
        StartCoroutine(PlayRoutine(_playDuration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        _playerMovementController.CurrState.Set(State.Fishing);
        float time = 0;
        while (time < duration)
        {
            SetCompletion(Mathf.InverseLerp(0, duration, time));
            float elapsed = Time.deltaTime;
            yield return new WaitForSeconds(elapsed);
            time += elapsed;
        }

        // If you didn't fail, you get a coin
        if (!_failed.Get())
        {
            _inventory.Money += 1;
            _playerMovementController.CurrState.Set(State.Celebrating);
            Invoke(nameof(BackToIdle), 1.5f);
            _playerSoundController.PlaySound("Caught");
        }
        else
        {
            _playerMovementController.CurrState.Set(State.Idle);
        }

        gameObject.SetActive(false);
    }

    private void BackToIdle()
    {
        _playerMovementController.CurrState.Set(State.Idle);
    }

    private void OnFire()
    {
        if (_failed.Get())
        {
            return;
        }

        FishBarTrigger next = GetNextTrigger();
        List<Collider2D> results = new List<Collider2D>();

        _indicatorCollider.OverlapCollider(new ContactFilter2D().NoFilter(), results);
        if (results.Contains(next.GetCollider()))
        {
            // yay!
            next.SetSprite(true);
            _triggerIdx += 1;

            // That was the last one. we won!
            if (_triggerIdx == _fishBarTriggers.Count)
            {
                _won = true;
            }
        }
        else
        {
            _failed.Set(true);
        }
    }
    private void generateTriggerPositions()
    {
        bool triggersTooClose;
        triggerPositions = new float[numTriggers + 1];
        triggerPositions[0] = 1.0f; // Final press

        switch (gameModifier)
        {
            case modifier.normal:
                for (int i = 1; i < numTriggers + 1; i++)
                {
                    triggerPositions[i] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        triggersTooClose = false;
                        for (int j = 0; j < i; j++)
                        {
                            if (Mathf.Abs(triggerPositions[i] - triggerPositions[j]) < minimumTriggerGap)
                            {
                                triggerPositions[i] = Random.Range(0.1f, 0.9f);
                                triggersTooClose = true;
                            }
                        }
                    } while (triggersTooClose);
                }
                break;

            case modifier.doubles:
                int k = 1;
                if (numTriggers % 2 != 0)
                {
                    triggerPositions[1] = 1.0f - specialGap;
                    k++;
                }

                for (; k < numTriggers; k += 2)
                {
                    triggerPositions[k] = Random.Range(0.1f, 0.9f);

                    do
                    {
                        triggersTooClose = false;
                        for (int j = 0; j < k; j++)
                        {
                            if (Mathf.Abs(triggerPositions[k] - triggerPositions[j]) < minimumTriggerGap)
                            {
                                triggerPositions[k] = Random.Range(0.1f, 0.9f);
                                triggersTooClose = true;
                            }
                        }
                    } while (triggersTooClose);

                    triggerPositions[k + 1] = triggerPositions[k] + specialGap;
                }

                break;

            case modifier.mega:
                triggerPositions[1] = Random.Range(0.1f, 0.9f - specialGap * numTriggers);
                for (int i = 1; i < numTriggers; i++)
                {
                    triggerPositions[i + 1] = triggerPositions[1] + specialGap;
                }
                break;
        }
    }
}
