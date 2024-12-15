using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ReactiveUnity;
using Unity.Mathematics;
using UnityEngine;

public enum TreeStates { SummerAdult, FallAdult, DeadAdult };
public abstract class TreePlant : MonoBehaviour, PlayerInteractionManager.IInteractable, Axe.IUseableWithAxe, BirdBrain.IShelterable
{

    [Header("Tree Base State")]
    [SerializeField] protected Material _standardLit;
    [SerializeField] protected Material _windyTree;
    [SerializeField] protected Sprite _summerAdult;
    [SerializeField] protected Sprite _fallAdult;
    [SerializeField] protected Sprite _deadAdult;
    [SerializeField] protected Reactive<TreeStates> _treeState = new Reactive<TreeStates>(TreeStates.SummerAdult);

    [Header("Fallen Tree")]
    [SerializeField] protected GameObject _E_fallenTree;
    [SerializeField] protected GameObject _W_fallenTree;
    [SerializeField] protected GameObject _stump;

    [Header("Chop Shake Properties")]
    [SerializeField] protected float _chopShakeDuration = 0.5f;
    [SerializeField] protected float _chopShakeStrength = 0.05f;
    [SerializeField] protected int _chopShakeVibrato = 10;
    [SerializeField] protected float _chopShakeRandomness = 90f;

    [Header("Gust Shake Material Properties")]
    [SerializeField] protected float _gustWindStrength = 1f;
    [SerializeField] protected float _gustSpeed = 30f;
    [SerializeField] protected float _bendScaler = 1f;

    [Header("Birding")]
    [SerializeField] private Collider2D _birdShelterTarget;
    protected HashSet<BirdBrain> _birdsInTree = new HashSet<BirdBrain>();

    protected float _originalWindStrength = 0.5f;
    protected float _originalGustSpeed = 0.4f;
    protected float _originalBendScaler = 1f;

    protected SpriteRenderer _spriteRenderer;
    private PlayerMovementController _playerMovementController;
    protected Action _unsubscribe;
    protected Wind _windManager;
    protected Vector3 _originalPosition;

    protected const int _HITS_TO_FALL = 5;
    protected int _hitCount = 0;

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        OnStateChange();
    }
    protected virtual void OnEnable()
    {
        _unsubscribe = _treeState.OnChange((prev, curr) => OnStateChange());
    }

    protected virtual void OnDisable()
    {
        _unsubscribe();
    }

    protected virtual void OnStateChange()
    {
        switch (_treeState.Value)
        {
            case TreeStates.SummerAdult:
                _spriteRenderer.material = _windyTree;
                _spriteRenderer.sprite = _summerAdult;
                break;
            case TreeStates.FallAdult:
                _spriteRenderer.material = _windyTree;
                _spriteRenderer.sprite = _fallAdult;
                break;
            case TreeStates.DeadAdult:
                _spriteRenderer.material = _standardLit;
                _spriteRenderer.sprite = _deadAdult;
                break;
            default:
                Debug.LogError("TreePlant state machine defaulted");
                break;
        }
    }

    public void OnUseAxe()
    {
        if (_hitCount < _HITS_TO_FALL - 1)
        {
            _hitCount++;
            FrightenBirds();
            ShakeTree();
            return;
        }
        StartCoroutine(FallTree());
    }

    protected void ShakeTree()
    {
        transform.DOShakePosition(_chopShakeDuration, new Vector3(_chopShakeStrength, 0, 0), _chopShakeVibrato, _chopShakeRandomness);
    }

    /// <summary>
    /// Spawns a stump and a falling tree. Deletes the standing tree (this object).
    /// </summary>
    IEnumerator FallTree()
    {
        // Wait for axing animation to finish
        yield return new WaitUntil(() => _playerMovementController.PlayerState.Value != PlayerStates.Axing);

        // Determine which tree prefab to select
        bool _fallsEast = WillTreeFallEast();
        GameObject _treeToFall = _fallsEast ? _E_fallenTree : _W_fallenTree;

        // Falls a hand-picked distance to the side of the stump
        Vector3 _fallenTreePosition = _fallsEast ? new Vector3(6f, 1f, 0) : new Vector3(-0.5f, 1f, 0);

        // Instantiating
        GameObject _larchStump = UnityEngine.Object.Instantiate
        (
            _stump,
            transform.position,
            Quaternion.identity,
            GameObject.FindGameObjectWithTag("Impermanent").transform
        );

        GameObject _fallenTree = UnityEngine.Object.Instantiate
        (
            _treeToFall,
            transform.position + _fallenTreePosition, // falls some distance to the side of stump
            Quaternion.identity,
            GameObject.FindGameObjectWithTag("Impermanent").transform
        );

        _fallenTree.GetComponent<FallenTree>().PlayFallingAnimation();

        // Want falling tree to appear infront of stump.
        // FallingTree.cs re-enables sprite sorting after falling
        _fallenTree.GetComponentInChildren<StaticSpriteSorting>().enabled = false;
        _fallenTree.GetComponentInChildren<SpriteRenderer>().sortingOrder = _spriteRenderer.sortingOrder + 1;
        Destroy(gameObject);
    }

    /// <summary>
    /// Determines tree fall direction
    /// </summary>
    private bool WillTreeFallEast()
    {
        switch (_playerMovementController.FacingDirection.Value)
        {
            case FacingDirection.West:
                return false;
            case FacingDirection.East:
                return true;
            case FacingDirection.North:
            case FacingDirection.South:
                return UnityEngine.Random.value < 0.5;
            default:
                Debug.LogError("DoesTreeFallEast defaulted.");
                return false;
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        return false;
    }

    public void OnBirdEntry(BirdBrain bird)
    {
        if (!_birdsInTree.Contains(bird))
        {
            _birdsInTree.Add(bird);
            ShakeTree();
        }
    }

    public void OnBirdExit(BirdBrain bird)
    {
        if (_birdsInTree.Contains(bird))
        {
            _birdsInTree.Remove(bird);
            ShakeTree();
        }
    }

    public Vector2 GetPositionTarget()
    {
        Bounds _bounds = _birdShelterTarget.bounds;

        return new Vector2
        (
            UnityEngine.Random.Range(_bounds.min.x, _bounds.max.x),
            UnityEngine.Random.Range(_bounds.min.y, _bounds.max.y)
        );
    }

    private void FrightenBirds()
    {
        if (_birdsInTree.Count == 0) return;

        foreach (var _bird in _birdsInTree)
            _bird.FrightenBird();
    }

    public int GetSortingOrder()
    {
        return _spriteRenderer.sortingOrder;
    }
}