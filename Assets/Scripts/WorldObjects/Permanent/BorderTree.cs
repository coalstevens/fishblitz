using System;
using ReactiveUnity;
using UnityEngine;

public class BorderTree : MonoBehaviour
{
    // Start is called before the first frame update
    //private enum TreeState {SummerAdult, FallAdult, DeadAdult, Stump};
    [SerializeField] private Sprite _summerAdult;
    [SerializeField] private Sprite _fallAdult;
    [SerializeField] private Sprite _deadAdult;
    [SerializeField] private Sprite _stump;
    [SerializeField] private Reactive<TreeStates> _treeState = new Reactive<TreeStates>(TreeStates.SummerAdult);
    private SpriteRenderer _spriteRenderer;
    private Action _unsubscribeHook;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    void OnEnable() {
       _unsubscribeHook = _treeState.OnChange((prev, curr) => UpdateSprite());
    }
    
    void OnDisable() {
        _unsubscribeHook();
    }

    void UpdateSprite() {
        switch (_treeState.Value) {
            case TreeStates.SummerAdult:
                _spriteRenderer.sprite = _summerAdult;
                break;
            case TreeStates.FallAdult:
                _spriteRenderer.sprite = _fallAdult;
                break;
            case TreeStates.DeadAdult:
                _spriteRenderer.sprite = _deadAdult;
                break;
            default:
                Debug.Log("Invalid State Reached");
                break;
        }
    }
}
