using ReactiveUnity;
using UnityEngine;

public class BorderTree : MonoBehaviour
{
    // Start is called before the first frame update
    private enum TreeState {SummerAdult, FallAdult, DeadAdult, Stump};
    [SerializeField] private Sprite _summerAdult;
    [SerializeField] private Sprite _fallAdult;
    [SerializeField] private Sprite _deadAdult;
    [SerializeField] private Sprite _stump;
    [SerializeField] private Reactive<TreeState> _treeState; 
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _treeState.OnChange((prev, curr) => UpdateSprite());
        UpdateSprite();
    }

    void UpdateSprite() {
        switch (_treeState.Value) {
            case TreeState.SummerAdult:
                _spriteRenderer.sprite = _summerAdult;
                break;
            case TreeState.FallAdult:
                _spriteRenderer.sprite = _fallAdult;
                break;
            case TreeState.DeadAdult:
                _spriteRenderer.sprite = _deadAdult;
                break;
            case TreeState.Stump:
                _spriteRenderer.sprite = _stump;
                break;
        }
    }
}
