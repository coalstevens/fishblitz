using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;

public class Box : MonoBehaviour, IWeightyObjectContainer, UseItemInput.IUsableTarget, ISceneSaveable
{
    [Header("References")]
    [SerializeField] private GameObject _blurb;
    [SerializeField] private GameObject _alert;
    [SerializeField] private SpriteRenderer _itemImage;
    [SerializeField] private PixelTextRenderer _quantityText;
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

    [SerializeField] private WeightyObjectStack _weightyContainer = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;
    private Dictionary<WeightyObjectType, int> _fulfilledQuantities = new();
    private bool _hasInteracted = false;
    private bool _isComplete = false;
    private Coroutine _fadeRoutine;

    private enum BoxAnimState { Closed, Opening, Open, Closing }
    private BoxAnimState _animState = BoxAnimState.Closed;

    private class BoxSaveData
    {
        public string BoxDataName;
        public Dictionary<string, int> FulfilledQuantities = new();
        public bool HasInteracted;
        public bool IsComplete;
    }

    private string _persistentID;
    public string PrefabId => "Box";
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var extended = new BoxSaveData
        {
            BoxDataName = _boxData.name,
            HasInteracted = _hasInteracted,
            IsComplete = _isComplete
        };
        foreach (var kv in _fulfilledQuantities)
            extended.FulfilledQuantities[kv.Key.name] = kv.Value;

        return JsonConvert.SerializeObject(extended);
    }

    public void RestoreState(string json)
    {
        var extended = JsonConvert.DeserializeObject<BoxSaveData>(json);
        _boxData = Resources.Load<BoxData>("BoxPrizes/" + extended.BoxDataName);
        _hasInteracted = extended.HasInteracted;
        _isComplete = extended.IsComplete;

        foreach (var kv in extended.FulfilledQuantities)
            foreach (var required in _boxData.RequiredObjects)
                if (required.Type.name == kv.Key)
                    _fulfilledQuantities[required.Type] = kv.Value;

        if (_isComplete)
        {
            SetBlurbVisible(false);
            _animator?.Play("Closed");
        }
        UpdateUI();
    }

    public void ResetState() { }

    public WeightyObjectStack WeightyStack => _weightyContainer;

    private void Awake()
    {
        Assert.IsNotNull(_blurb);
        Assert.IsNotNull(_alert);
        Assert.IsNotNull(_itemImage);
        Assert.IsNotNull(_quantityText);
        Assert.IsNotNull(_boxData);

        foreach (var required in _boxData.RequiredObjects)
        {
            _fulfilledQuantities[required.Type] = 0;
        }

    }

    private void Start()
    {
        SetBlurbVisible(false);
        _alert.SetActive(true);
        _animator?.Play("Closed");
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_isComplete) return false;
        UpdateUI();
        ShowBlurb();
        _alert.SetActive(false);
        return true;
    }

    private void ShowBlurb()
    {
        SetBlurbVisible(true);

        if (!_hasInteracted)
        {
            _hasInteracted = true;
            _alert.SetActive(false);
            OnPlayerProximityEnter();
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
        if (_stackConfig != null && _stackConfig.InsertSound != null)
            PlayerAudioManager.Instance.PlayOneShot(_stackConfig.InsertSound);
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
        _quantityText.Text = remaining.ToString();
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
        yield return FadeRenderersToZero(0.5f);
    }

    private IEnumerator FadeRenderersToZero(float duration)
    {
        var renderers = _blurb.GetComponentsInChildren<SpriteRenderer>(true);
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = originalColors[i];
                renderers[i].color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, t));
            }
            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = originalColors[i];
            renderers[i].color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    private void SetBlurbVisible(bool visible)
    {
        var renderers = _blurb.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = renderers[i].color;
            renderers[i].color = new Color(c.r, c.g, c.b, visible ? 1f : 0f);
        }
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

        ReduceOpacityWhenPlayerBehind opacityScript = _blurb.GetComponent<ReduceOpacityWhenPlayerBehind>();
        if (opacityScript != null)
            opacityScript.enabled = false;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        StartCoroutine(FadeRenderersToZero(0.5f));

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
        else if (_boxData.PrizeItem != null && _boxData.PrizePlaceholderPrefab != null)
        {
            Vector3 spawnPos = transform.position + _boxData.PrizeSpawnOffset;
            GameObject prize = Instantiate(_boxData.PrizePlaceholderPrefab, spawnPos, Quaternion.identity);
            if (prize.TryGetComponent<PrizePlaceholder>(out var placeholder))
                placeholder.SetItem(_boxData.PrizeItem);
            if (prize.TryGetComponent<BoxData.IBoxPrize>(out var prizeComponent))
                prizeComponent.AwardPrize();
        }
        else
        {
            Debug.LogWarning("BoxData has no prize configured");
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
        if (_isComplete || !_hasInteracted) return;

        if (_animState == BoxAnimState.Closed || _animState == BoxAnimState.Closing)
        {
            _animator?.Play("Opening");
            _animState = BoxAnimState.Opening;
            StartCoroutine(OnOpeningFinished());
        }
    }

    public void OnPlayerProximityExit()
    {
        if (_isComplete || !_hasInteracted) return;

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
