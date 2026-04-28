using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class YDepthSorter : MonoBehaviour
{
    [Tooltip("If enabled, sorting updates every frame. If disabled, sorts once on enable/start.")]
    [SerializeField] private bool _enableDynamicSorting = false;

    [Header("Layer Offsets")]
    [SerializeField] private int _yPrecision = 100;
    [SerializeField] private int _groundOffset = 0;
    [SerializeField] private int _lowOffset = 0;
    [SerializeField] private int _highOffset = 0;
    [SerializeField] private int _birdOffset = 75;

    private SpriteRenderer _spriteRenderer;
    private SortingGroup _sortingGroup;
    private Canvas _canvas;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _sortingGroup = GetComponent<SortingGroup>();
        _canvas = GetComponent<Canvas>();

        Assert.IsTrue(
            _spriteRenderer != null || _sortingGroup != null || _canvas != null,
            "StaticSpriteSorting: No SpriteRenderer, SortingGroup, or Canvas found on this GameObject."
        );
    }

    private void OnEnable()
    {
        Sort();
    }

    private void Start()
    {
        Sort();
    }

    private void Update()
    {
        if (_enableDynamicSorting)
            Sort();
    }

    public void Sort()
    {
        int baseOrder = -Mathf.RoundToInt(transform.position.y * _yPrecision);
        int heightOffset = GetLayerOffset(gameObject.layer);
        int finalOrder = baseOrder + heightOffset;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = finalOrder;
        }
        else if (_sortingGroup != null)
        {
            _sortingGroup.sortingOrder = finalOrder;
        }
        else if (_canvas != null)
        {
            _canvas.sortingOrder = finalOrder;
        }
    }

    private int GetLayerOffset(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);

        switch (layerName)
        {
            case "GroundObstacle":
                return _groundOffset;
            case "LowObstacle":
                return _lowOffset;
            case "HighObstacle":
                return _highOffset;
            case "Birds":
                return _birdOffset;
            default:
                return 0;
        }
    }
}
