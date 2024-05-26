using System;
using System.Collections;
using ReactiveUnity;
using UnityEngine;

public enum TreeStates { SummerAdult, FallAdult, DeadAdult, Stump };
public abstract class TreePlant : MonoBehaviour, IInteractable, IUseableWithAxe
{

    [Header("Tree Base State")]
    [SerializeField] protected Material _standardLit;
    [SerializeField] protected Material _windyTree;
    [SerializeField] protected Sprite _summerAdult;
    [SerializeField] protected Sprite _fallAdult;
    [SerializeField] protected Sprite _deadAdult;
    [SerializeField] protected Reactive<TreeStates> _treeState = new Reactive<TreeStates>(TreeStates.SummerAdult);
    protected SpriteRenderer _spriteRenderer;
    private PlayerMovementController _playerMovementController;
    protected Action _unsubscribe;

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
            return;
        }
        StartCoroutine(FallTree());
    }

    /// <summary>
    /// Spawns a stump and a falling tree. Deletes the standing tree (this object).
    /// </summary>
    IEnumerator FallTree() {
        // wait for axing animation to finish
        yield return new WaitUntil(() => _playerMovementController.PlayerState.Value != PlayerStates.Axing);
        bool _fallsEast = WillTreeFallEast();
        string _fallenTreeIdentifier = _fallsEast ? "WorldObjects/E_FallenLarch" :  "WorldObjects/W_FallenLarch";
        Vector3 _fallenTreePosition = _fallsEast ? new Vector3(6f, 1f, 0) : new Vector3(-0.5f, 1f, 0);

        GameObject _larchStump = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("WorldObjects/LarchStump"),
                                                                transform.position,
                                                                Quaternion.identity,
                                                                GameObject.FindGameObjectWithTag("Impermanent").transform);

        GameObject _fallenLarch = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(_fallenTreeIdentifier),
                                                                transform.position + _fallenTreePosition, // falls some distance to the side of stump
                                                                Quaternion.identity,
                                                                GameObject.FindGameObjectWithTag("Impermanent").transform);
        
        _fallenLarch.GetComponentInChildren<StaticSpriteSorting>().enabled = false;
        _fallenLarch.GetComponentInChildren<SpriteRenderer>().sortingOrder = _spriteRenderer.sortingOrder + 1;
        Destroy(gameObject);
    }

    /// <summary>
    /// Determines tree fall direction
    /// </summary>
    private bool WillTreeFallEast() { 
        switch (_playerMovementController.FacingDirection.Value) {
            case FacingDirections.West:
                return false;
            case FacingDirections.East:
                return true;
            case FacingDirections.North:
            case FacingDirections.South:
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
}