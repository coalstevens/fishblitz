using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRod : MonoBehaviour
{
    [Header("Sprite Options")]
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Sprite _onSelectedSprite;
    [SerializeField] private Sprite _offSelectedSprite;

    [Header("Behavioural Options")]
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;

    [Header("Shake Options")]
    [SerializeField] private float _shakeDuration = 1;
    [SerializeField] private float _shakeStrength = 1;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90;

    [Header("Input Options")]
    [SerializeField] private InputActionReference _inputActionReference;

    private SpriteRenderer _spriteRenderer;
    private Reactive<bool> _fishOn = new Reactive<bool>(false);
    private Reactive<bool> _selected = new Reactive<bool>(false);
    private FishBar _fishBar;
    private Coroutine _changeStateRoutine;
    private Inventory _inventory;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fishOn.OnChange((_, curr) => ChangeSprite(curr, _selected.Get()));
        _fishOn.OnChange((_, curr) => Shake());
        _selected.OnChange((_, selected) => ChangeSprite(_fishOn.Get(), selected));
        ChangeSprite(_fishOn.Get(), _selected.Get());
    }

    private void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }

    private void Start()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _changeStateRoutine = StartCoroutine(ChangeStateRoutine());
    }

    private IEnumerator ChangeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishOn.Set(!_fishOn.Get());
        }
    }

    private void ChangeSprite(bool fishOn, bool selected)
    {
        if (fishOn)
        {
            _spriteRenderer.sprite = selected ? _onSelectedSprite : _onSprite;
        }
        else
        {
            _spriteRenderer.sprite = selected ? _offSelectedSprite : _offSprite;
        }
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

        if (!_inputActionReference.action.WasPressedThisFrame())
        {
            return;
        }

        if (!_fishOn.Get())
        {
            Shake();
        }
        else
        {
            StartFishingGame();
        }
    }

    private void StartFishingGame()
    {
        StopCoroutine(_changeStateRoutine);
        _inventory.Rods += 1;
        _fishBar.Play(3);
        Destroy(gameObject);
    }
}