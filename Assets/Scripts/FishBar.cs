using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FishBar : MonoBehaviour
{
    [SerializeField] private float _startHeight;
    [SerializeField] private float _endHeight;
    [SerializeField] private SpriteRenderer _barSprite;
    [SerializeField] private GameObject _fishSpriteObject;
    [SerializeField] private float _playDuration = 5f;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private Inventory _inventory;

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
    private FishBarTrigger[] _fishBarTriggers;
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
        _fishBarTriggers = GetComponentsInChildren<FishBarTrigger>(true);
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
        int idx = _triggerIdx >= _fishBarTriggers.Length ? _fishBarTriggers.Length - 1 : _triggerIdx;
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
    public void Play(float duration)
    {
        Initialize();
        StartCoroutine(PlayRoutine(duration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        _playerMovementController.Fishing = true;
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
        }
        gameObject.SetActive(false);
        _playerMovementController.Fishing = false;
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
            if (_triggerIdx == _fishBarTriggers.Length)
            {
                _won = true;
            }
        }
        else
        {
            _failed.Set(true);
        }
    }
}
