using System;
using ReactiveUnity;
using UnityEngine;

public enum TreeStates {SummerAdult, FallAdult, DeadAdult, Stump};
public abstract class TreePlant : MonoBehaviour {

    [Header("Tree Base State")]
    [SerializeField] protected Material _standardLit;
    [SerializeField] protected Material _windyTree;
    [SerializeField] protected Sprite _summerAdult;
    [SerializeField] protected Sprite _fallAdult;
    [SerializeField] protected Sprite _deadAdult;
    [SerializeField] protected Sprite _stump;
    [SerializeField] protected Reactive<TreeStates> _treeState = new Reactive<TreeStates>(TreeStates.SummerAdult);
    protected SpriteRenderer _spriteRenderer;
    protected Action _unsubscribe;

    public virtual Collider2D ObjCollider {
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider == null) {
                Debug.LogError("Tree does not have a collider");
                return null;
            }
            else {
                return _collider;
            }
        }
    } 

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        OnStateChange();
    }
    protected virtual void OnEnable() {
        _unsubscribe = _treeState.OnChange((prev, curr) => OnStateChange());
    }
    protected virtual void OnDisable() {
        _unsubscribe();
    }

    protected virtual void OnStateChange() {
        switch (_treeState.Value) {
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
            case TreeStates.Stump:
                _spriteRenderer.material = _standardLit;
                _spriteRenderer.sprite = _stump;
                break;
        }
    }
}