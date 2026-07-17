using System.Collections.Generic;
using UnityEngine;

public class PixelTextRenderer : MonoBehaviour
{
    public enum Alignment { Left, Center, Right }

    [SerializeField] private PixelFont _font;
    [SerializeField] private string _text = "";
    [SerializeField] private Alignment _alignment = Alignment.Left;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private Material _material;
    [SerializeField] private int _sortingOrder = 10;
    [SerializeField] private string _sortingLayerName = "Default";

    [Header("Layout")]
    [SerializeField] private int _lineHeight = 16;
    [SerializeField] private int _maxWrapWidth = 0;

    private List<SpriteRenderer> _glyphPool = new();
    private int _activeGlyphCount = 0;
    private float _totalWorldWidth = 0f;

    public string Text
    {
        get => _text;
        set
        {
            string val = (value ?? "").Replace("\\n", "\n");
            if (_text != val)
            {
                _text = val;
                Rebuild();
            }
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            ApplyColorToActiveGlyphs();
        }
    }

    public float Alpha
    {
        get => _color.a;
        set
        {
            _color.a = Mathf.Clamp01(value);
            ApplyColorToActiveGlyphs();
        }
    }

    public Material Material
    {
        get => _material;
        set
        {
            _material = value;
            ApplyMaterialToActiveGlyphs();
        }
    }

    public float TotalWidth => _totalWorldWidth;

    public int SortingOrder
    {
        set
        {
            _sortingOrder = value;
            for (int i = 0; i < _glyphPool.Count; i++)
                _glyphPool[i].sortingOrder = _sortingOrder;
        }
    }

    private void Awake()
    {
        Rebuild();
    }

    private void Start()
    {
        Rebuild();
    }

    private void OnDestroy()
    {
        DestroyAllGlyphs();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
                return;
            Rebuild();
        };
#endif
    }

    private void OnDrawGizmosSelected()
    {
        string text = _text.Replace("\\n", "\n");

        if (_font == null || _maxWrapWidth <= 0 || string.IsNullOrEmpty(text))
            return;

        float ppu = _font.PixelsPerUnit;
        float unitsPerPixel = 1f / ppu;

        List<string> lines = BuildLines(text);
        int lineCount = Mathf.Max(lines.Count, 1);
        float boxWidth = _maxWrapWidth * unitsPerPixel;
        float boxHeight = lineCount * _lineHeight * unitsPerPixel;

        float bottomY = -(lineCount - 1) * _lineHeight * unitsPerPixel;
        float centerY = bottomY + boxHeight / 2f;
        Vector3 rectCenter = new Vector3(boxWidth / 2f, centerY, 0f);
        Vector3 rectSize = new Vector3(boxWidth, boxHeight, 0f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.4f);
        Gizmos.DrawWireCube(rectCenter, rectSize);
    }

    public void Rebuild()
    {
        string text = _text.Replace("\\n", "\n");

        if (_font == null || string.IsNullOrEmpty(text))
        {
            HideAllGlyphs();
            _totalWorldWidth = 0f;
            return;
        }

        float ppu = _font.PixelsPerUnit;
        float unitsPerPixel = 1f / ppu;

        DestroyAllGlyphs(immediate: true);

        List<string> lines = BuildLines(text);
        float maxVisualWidth = 0f;
        foreach (string line in lines)
        {
            int w = _font.GetTotalPixelWidth(line);
            int visualW = w - _font.Tracking;
            if (visualW > maxVisualWidth)
                maxVisualWidth = visualW;
        }

        _totalWorldWidth = maxVisualWidth * unitsPerPixel;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            int lineWidth = _font.GetTotalPixelWidth(line);
            int visualLineWidth = lineWidth - _font.Tracking;
            float refWidth = _maxWrapWidth > 0 ? _maxWrapWidth * unitsPerPixel : maxVisualWidth * unitsPerPixel;
            float visualLineWorldWidth = visualLineWidth * unitsPerPixel;
            float lineStartX = _alignment switch
            {
                Alignment.Center => (refWidth - visualLineWorldWidth) / 2f,
                Alignment.Right => refWidth - visualLineWorldWidth,
                _ => 0f
            };

            int currentPixelX = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (!_font.TryGetGlyph(c, out Sprite sprite, out int advance))
                    continue;

                if (sprite == null)
                {
                    currentPixelX += advance;
                    continue;
                }

                SpriteRenderer glyph = GetOrCreateGlyph(_activeGlyphCount);
                glyph.sprite = sprite;
                glyph.color = _color;
                glyph.sortingOrder = _sortingOrder;
                glyph.sortingLayerName = _sortingLayerName;
                glyph.gameObject.SetActive(true);

                float worldX = lineStartX + currentPixelX * unitsPerPixel;
                float worldY = -(float)(lineIndex * _lineHeight) * unitsPerPixel;
                glyph.transform.localPosition = new Vector3(worldX, worldY, 0f);

                currentPixelX += advance;
                _activeGlyphCount++;
            }
        }

        for (int i = _activeGlyphCount; i < _glyphPool.Count; i++)
            _glyphPool[i].gameObject.SetActive(false);
    }

    private List<string> BuildLines(string text)
    {
        string[] manualLines = text.Split("\n");
        List<string> result = new List<string>();

        foreach (string manualLine in manualLines)
        {
            if (_maxWrapWidth <= 0)
            {
                result.Add(manualLine);
                continue;
            }

            WrapLine(manualLine, result);
        }

        return result;
    }

    private void WrapLine(string line, List<string> result)
    {
        int length = line.Length;
        int lineStart = 0;

        while (lineStart < length)
        {
            int width = 0;
            int lastSpace = -1;
            int i = lineStart;

            while (i < length)
            {
                char c = line[i];

                if (c == ' ')
                    lastSpace = i;

                if (!_font.TryGetGlyph(c, out _, out int advance))
                    advance = 0;

                int wrapAdvance = Mathf.Max(1, advance - _font.Tracking);
                if (width + wrapAdvance > _maxWrapWidth)
                {
                    if (lastSpace > lineStart)
                    {
                        int segLen = lastSpace - lineStart;
                        result.Add(line.Substring(lineStart, segLen));
                        lineStart = lastSpace + 1;
                    }
                    else
                    {
                        int segLen = i - lineStart;
                        if (segLen > 0)
                        {
                            result.Add(line.Substring(lineStart, segLen));
                            lineStart = i;
                        }
                        else
                        {
                            result.Add(line.Substring(lineStart, 1));
                            lineStart = i + 1;
                        }
                    }
                    break;
                }

                width += advance;
                i++;

                if (i >= length)
                {
                    result.Add(line.Substring(lineStart));
                    lineStart = length;
                }
            }
        }
    }

    private SpriteRenderer GetOrCreateGlyph(int index)
    {
        while (index >= _glyphPool.Count)
        {
            GameObject go = new GameObject("Glyph");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            _glyphPool.Add(go.AddComponent<SpriteRenderer>());
        }
        return _glyphPool[index];
    }

    private void DestroyAllGlyphs(bool immediate = false)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (immediate || !Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Destroy(transform.GetChild(i).gameObject);
        }
        _glyphPool.Clear();
        _activeGlyphCount = 0;
    }

    private void HideAllGlyphs()
    {
        _activeGlyphCount = 0;
        foreach (SpriteRenderer sr in _glyphPool)
            sr.gameObject.SetActive(false);
    }

    private void ApplyColorToActiveGlyphs()
    {
        for (int i = 0; i < _activeGlyphCount; i++)
            _glyphPool[i].color = _color;
    }

    private void ApplyMaterialToActiveGlyphs()
    {
        for (int i = 0; i < _activeGlyphCount; i++)
            _glyphPool[i].material = _material;
    }
}
