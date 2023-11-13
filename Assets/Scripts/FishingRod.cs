using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRod : MonoBehaviour
{
    [Header("Sprite Options")]
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;

    [Header("Behavioural Options")]
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;

    [Header("Shake Options")]
    [SerializeField] private float _shakeDuration = 1;
    [SerializeField] private float _shakeStrength = 1;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90;

    [Header("Input Options")]
    [SerializeField] private InputActionReference _inputActionRef;

    private SpriteRenderer _spriteRenderer;
    private Reactive<bool> _fishOn = new Reactive<bool>(false);
    private Reactive<bool> _selected = new Reactive<bool>(false);
    private FishBar _fishBar;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fishOn.OnChange((_, curr) => ChangeSprite(curr));
        _fishOn.OnChange((_, curr) => Shake());
        _selected.OnChange((_, selected) => ChangeOutline(selected));
        ChangeSprite(_fishOn.Get());
    }

    private void ChangeOutline(bool selected)
    {
        _spriteRenderer.color = selected ? Color.green : Color.white;
    }

    private void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }

    private void Start()
    {
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>();
        StartCoroutine(ChangeStateRoutine());
    }

    private IEnumerator ChangeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishOn.Set(!_fishOn.Get());
        }
    }

    private void ChangeSprite(bool fishOn)
    {
        _spriteRenderer.sprite = fishOn ? _onSprite : _offSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _selected.Set(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _selected.Set(false);
    }

    private void Update()
    {
        if (!_selected.Get())
        {
            return;
        }


    }
}
