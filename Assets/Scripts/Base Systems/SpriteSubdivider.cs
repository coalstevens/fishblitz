using UnityEngine;

public class SpriteSubdivider : MonoBehaviour
{
    [SerializeField] private int _horizontalSegments = 4;
    [SerializeField] private int _verticalSegments = 4;
    [SerializeField] private bool _cloneSprite = true;

    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;
    private Sprite _overrideSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Regenerate();
    }

    public void Regenerate()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        var currentSprite = _spriteRenderer.sprite;
        if (currentSprite == null)
            return;

        if (_cloneSprite)
        {
            if (_overrideSprite == null || currentSprite != _originalSprite)
            {
                if (_overrideSprite != null && _overrideSprite != currentSprite)
                    Destroy(_overrideSprite);

                _originalSprite = currentSprite;
                _overrideSprite = Instantiate(currentSprite);
                _spriteRenderer.sprite = _overrideSprite;
            }
            BuildAndOverride(_overrideSprite);
        }
        else
        {
            BuildAndOverride(currentSprite);
        }
    }

    private void BuildAndOverride(Sprite sprite)
    {
        var rect = sprite.rect;

        int hSegs = Mathf.Max(1, _horizontalSegments);
        int vSegs = Mathf.Max(1, _verticalSegments);
        int vertCount = (hSegs + 1) * (vSegs + 1);
        int triCount = hSegs * vSegs * 6;

        var vertices = new Vector2[vertCount];
        var triangles = new ushort[triCount];

        for (int row = 0; row <= vSegs; row++)
        {
            float t = (float)row / vSegs;

            for (int col = 0; col <= hSegs; col++)
            {
                float s = (float)col / hSegs;

                int idx = row * (hSegs + 1) + col;
                vertices[idx] = new Vector2(s * rect.width, t * rect.height);
            }
        }

        ushort triIdx = 0;
        for (int row = 0; row < vSegs; row++)
        {
            for (int col = 0; col < hSegs; col++)
            {
                ushort bl = (ushort)(row * (hSegs + 1) + col);
                ushort tl = (ushort)((row + 1) * (hSegs + 1) + col);
                ushort tr = (ushort)((row + 1) * (hSegs + 1) + col + 1);
                ushort br = (ushort)(row * (hSegs + 1) + col + 1);

                triangles[triIdx++] = bl;
                triangles[triIdx++] = tl;
                triangles[triIdx++] = tr;

                triangles[triIdx++] = bl;
                triangles[triIdx++] = tr;
                triangles[triIdx++] = br;
            }
        }

        sprite.OverrideGeometry(vertices, triangles);
    }

    private void OnDestroy()
    {
        if (_overrideSprite != null && _overrideSprite != _originalSprite)
            Destroy(_overrideSprite);
    }
}
