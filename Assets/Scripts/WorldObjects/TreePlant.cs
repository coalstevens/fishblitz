using ReactiveUnity;
using UnityEngine;
public class TreePlant : MonoBehaviour, IWorldObject
{
    private enum TreeState {SummerAdult, FallAdult, DeadAdult, Stump};
    [SerializeField] private string _identifier;
    [SerializeField] private Sprite _summerAdult;
    [SerializeField] private Sprite _fallAdult;
    [SerializeField] private Sprite _deadAdult;
    [SerializeField] private Sprite _stump;
    private Reactive<TreeState> _treeState = new Reactive<TreeState>(TreeState.SummerAdult);
    private SpriteRenderer _spriteRenderer;

    public string Identifier {
        get {
            if (_identifier == null) {
                Debug.LogError("This Tree doesn't have an identifier");
                return null;
            }
            else {
                return _identifier; 
            }
        }
    }
    public int State { 
        get {
            return (int) _treeState.Value;
        }
        set {
            _treeState.Value = (TreeState) value;
        }
    }

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