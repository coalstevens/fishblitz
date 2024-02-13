using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ReactiveUnity;

public class PlacedMountedRodSaveData : WorldObjectSaveData {
    public bool fishOnState;
}

// Used for 4 different prefabs (different rod facing directions)
// Hence there is lots of serialized members
public class PlacedMountedRod : MonoBehaviour, IInteractable, ISaveable<PlacedMountedRodSaveData>
{
    [SerializeField] private string _identifier;
    
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
    private Inventory _inventory;
    private SpriteRenderer _spriteRenderer;
    private Reactive<bool> _fishOn = new Reactive<bool>(false);
    private Reactive<bool> _selected = new Reactive<bool>(false);
    private FishBar _fishBar;
    private Coroutine _changeStateRoutine;
    private ActiveGridCell _activeGridCell;

    public Collider2D ObjCollider {
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null) {
                return _collider;
            }
            else {
                Debug.LogError("PlacedMountedRod does not have a collider component");
                return null;
            }
        }
    }

    public string Identifier {
        get {
            if (_identifier == null) {
                Debug.LogError("PlacedMountedRod does not have an identifier.");
            }
            return _identifier;
        }
    }

    public int State { 
        get => _fishOn.Value ? 1 : 0;
        set => _fishOn.Value = value == 1; 
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fishOn.OnChange((_, curr) => ChangeSprite(curr, _selected.Value));
        _fishOn.OnChange((_, curr) => Shake());
        _selected.OnChange((_, selected) => ChangeSprite(_fishOn.Value, selected));
        ChangeSprite(_fishOn.Value, _selected.Value);
    }

    private void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }

    private void Start()
    {
        _activeGridCell = GameObject.FindWithTag("ActiveGridCell").GetComponent<ActiveGridCell>();
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _changeStateRoutine = StartCoroutine(ChangeStateRoutine());
    }

    private IEnumerator ChangeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishOn.Value = !_fishOn.Value;
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

    private void Update()
    {
        List<Collider2D> _results = new List<Collider2D>();
        Physics2D.OverlapBox(_activeGridCell.GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);

        bool _found = false;
        foreach (var _result in _results)
        {
            var _rod = _result.gameObject.GetComponent<PlacedMountedRod>();
            if (_rod == this)
            {
                _found = true;
                _selected.Value = true;
            }
        }

        if (!_found)
        {
            _selected.Value = false;
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (!_fishOn.Value) {
            Shake();
            return true;
        }

        StopCoroutine(_changeStateRoutine);
        _inventory.TryAddItem("MountedRod", 1);
        _fishBar.Play();
        Destroy(gameObject);
        return true;
    }

    public PlacedMountedRodSaveData Save()
    {
        return new PlacedMountedRodSaveData {
            Identifier = _identifier,
            Position = new SimpleVector3(transform.position),
            fishOnState = _fishOn.Value            
        };
    }

    public void Load(PlacedMountedRodSaveData saveData)
    {
        _fishOn.Value = saveData.fishOnState;
    }
}