using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class Box : MonoBehaviour, IWeightyObjectContainer, UseItemInput.IUsableTarget
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasCanvasGroup;
    [SerializeField] private CanvasGroup _blurbCanvasGroup;
    [SerializeField] private GameObject _alert;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private float _fadeDelaySeconds = 3f;

    [Header("Shake Properties")]
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeStrength = 0.05f;
    [SerializeField] private int _shakeVibrato = 5;
    [SerializeField] private float _shakeRandomness = 90f;

    [Header("Quest Data")]
    [SerializeField] private BoxData _boxData;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    [Header("Sound Effects")]
    [SerializeField] private SoundData _winChimeSound;
    [SerializeField] private AudioSource _audioSource;

    private WeightyObjectStack _weightyContainer;
    private Dictionary<WeightyObjectType, int> _fulfilledQuantities = new();
    private bool _hasInteracted = false;
    private bool _isComplete = false;
    private Coroutine _fadeRoutine;

    private enum BoxAnimState { Closed, Opening, Open, Closing }
    private BoxAnimState _animState = BoxAnimState.Closed;

    public WeightyObjectStack WeightyStack => _weightyContainer;

    private void Awake()
    {
        _weightyContainer = GetComponent<WeightyObjectStack>();
        Assert.IsNotNull(_weightyContainer);
        Assert.IsNotNull(_canvasCanvasGroup);
        Assert.IsNotNull(_blurbCanvasGroup);
        Assert.IsNotNull(_alert);
        Assert.IsNotNull(_itemImage);
        Assert.IsNotNull(_quantityText);
        Assert.IsNotNull(_boxData);

        foreach (var required in _boxData.RequiredObjects)
        {
            _fulfilledQuantities[required.Type] = 0;
        }

        _blurbCanvasGroup.alpha = 0;
        _alert.SetActive(true);

        _animator?.Play("Closed");
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_isComplete) return false;
        UpdateUI();
        ShowBlurb();
        return true;
    }

    private void ShowBlurb()
    {
        _blurbCanvasGroup.alpha = 1;

        if (!_hasInteracted)
        {
            _hasInteracted = true;
            _alert.SetActive(false);
        }

        StartFadeTimer();
    }

    private bool ValidateItemType(WeightyObjectType type)
    {
        if (_boxData.RequiredObjects.Count == 0)
            return false;

        var targetObject = _boxData.RequiredObjects[0];
        int required = targetObject.Quantity;
        int fulfilled = _fulfilledQuantities[type];

        return fulfilled < required;
    }

    private void RejectInvalidItem()
    {
        Debug.Log("Box: Invalid item deposited");
    }

    public bool TryAddToBox(StoredWeightyObject item)
    {
        if (_isComplete) return false;
        UpdateUI();
        ShowBlurb();

        if (!ValidateItemType(item.Type))
        {
            RejectInvalidItem();
            return false;
        }

        Debug.Log("Added " + item.Type);

        _weightyContainer.Push(item);
        _fulfilledQuantities[item.Type]++;
        UpdateUI();
        Shake();
        CheckWinCondition();

        return true;
    }

    private void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }

    private void UpdateUI()
    {
        if (_boxData.RequiredObjects.Count == 0)
            return;

        var targetObject = _boxData.RequiredObjects[0];
        _itemImage.sprite = targetObject.Type.NSCarry;

        int remaining = targetObject.Quantity - _fulfilledQuantities[targetObject.Type];
        _quantityText.text = remaining.ToString();
    }

    private void StartFadeTimer()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeOutBlurb());
    }

    private IEnumerator FadeOutBlurb()
    {
        yield return new WaitForSeconds(_fadeDelaySeconds);

        float startAlpha = _blurbCanvasGroup.alpha;
        float time = 0f;
        float fadeDuration = 0.5f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            _blurbCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / fadeDuration);
            yield return null;
        }

        _blurbCanvasGroup.alpha = 0;
    }

    private void CheckWinCondition()
    {
        if (_boxData.RequiredObjects.Count == 0)
            return;

        var targetObject = _boxData.RequiredObjects[0];
        if (_fulfilledQuantities[targetObject.Type] >= targetObject.Quantity)
        {
            Win();
        }
    }

    private void Win()
    {
        _isComplete = true;

        AudioManager.PlaySFX(_audioSource, _winChimeSound);

        if (_animator != null)
            _animator.Play("Win");

        StartCoroutine(DeliverPrize());
    }

    private IEnumerator DeliverPrize()
    {
        yield return new WaitForSeconds(_animator.GetClipLength("Win"));

        if (_boxData.PrizePrefab != null)
        {
            Vector3 spawnPos = transform.position + _boxData.PrizeSpawnOffset;
            GameObject prize = Instantiate(_boxData.PrizePrefab, spawnPos, Quaternion.identity);
            if (prize.TryGetComponent<BoxData.IBoxPrize>(out var prizeComponent))
                prizeComponent.AwardPrize();
        }

        Destroy(gameObject);
    }

    private IEnumerator OnOpeningFinished()
    {
        yield return new WaitForSeconds(_animator.GetClipLength("Opening"));
        if (_isComplete) yield break;
        _animator?.Play("Open");
        _animState = BoxAnimState.Open;
    }

    public void OnPlayerProximityEnter()
    {
        if (_isComplete) return;

        if (_animState == BoxAnimState.Closed || _animState == BoxAnimState.Closing)
        {
            _animator?.Play("Opening");
            _animState = BoxAnimState.Opening;
            StartCoroutine(OnOpeningFinished());
        }
    }

    public void OnPlayerProximityExit()
    {
        if (_isComplete) return;

        if (_animState == BoxAnimState.Open)
        {
            _animator?.Play("Closing");
            _animState = BoxAnimState.Closing;
            StartCoroutine(OnClosingFinished());
        }
    }

    private IEnumerator OnClosingFinished()
    {
        yield return new WaitForSeconds(_animator.GetClipLength("Closing"));
        if (_isComplete) yield break;
        _animator?.Play("Closed");
        _animState = BoxAnimState.Closed;
    }
}
