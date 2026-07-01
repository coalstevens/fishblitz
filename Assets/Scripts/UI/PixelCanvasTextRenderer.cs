using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PixelCanvasTextRenderer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public enum Alignment { Left, Center, Right }

    [SerializeField] private PixelFont _font;
    [SerializeField] private string _text = "";
    [SerializeField] private Alignment _alignment = Alignment.Left;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private Material _material;

    [Header("Layout")]
    [SerializeField] private int _lineHeight = 16;
    [SerializeField] private int _maxWrapWidth = 0;

    [Header("Selectable States")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _hoveredMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _pressedMaterial;

    private int _activeGlyphCount = 0;
    private float _totalPixelWidth = 0f;

    private bool _isPointerOver;
    private bool _isPointerDown;
    private bool _isSelected;

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

    public float TotalWidth => _totalPixelWidth;

    private void Awake()
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
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
                Rebuild();
        };
#endif
    }

    private void OnDrawGizmosSelected()
    {
        string text = _text.Replace("\\n", "\n");

        if (_font == null || _maxWrapWidth <= 0 || string.IsNullOrEmpty(text))
            return;

        List<string> lines = BuildLines(text);
        int lineCount = Mathf.Max(lines.Count, 1);
        float boxWidth = _maxWrapWidth;
        float boxHeight = lineCount * _lineHeight;

        float bottomY = -(lineCount - 1) * _lineHeight;
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
            DestroyAllGlyphs();
            _totalPixelWidth = 0f;
            return;
        }

        DestroyAllGlyphs(immediate: true);

        List<string> lines = BuildLines(text);
        _totalPixelWidth = 0f;
        float maxVisualWidth = 0f;
        foreach (string line in lines)
        {
            int w = _font.GetTotalPixelWidth(line);
            int visualW = w - _font.Tracking;
            if (w > _totalPixelWidth)
                _totalPixelWidth = w;
            if (visualW > maxVisualWidth)
                maxVisualWidth = visualW;
        }

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            int lineWidth = _font.GetTotalPixelWidth(line);
            int visualLineWidth = lineWidth - _font.Tracking;
            float refWidth = _maxWrapWidth > 0 ? _maxWrapWidth : maxVisualWidth;
            float lineStartX = _alignment switch
            {
                Alignment.Center => (refWidth - visualLineWidth) / 2f,
                Alignment.Right => refWidth - visualLineWidth,
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

                Image glyph = GetOrCreateGlyph(_activeGlyphCount);
                glyph.sprite = sprite;
                glyph.SetNativeSize();

                Vector2 spritePivot = new Vector2(
                    sprite.pivot.x / sprite.rect.width,
                    sprite.pivot.y / sprite.rect.height
                );
                glyph.rectTransform.pivot = spritePivot;

                glyph.color = _color;
                glyph.raycastTarget = false;
                glyph.preserveAspect = true;
                glyph.gameObject.SetActive(true);

                float y = -(float)(lineIndex * _lineHeight);
                glyph.rectTransform.anchoredPosition = new Vector2(lineStartX + currentPixelX, y);

                currentPixelX += advance;
                _activeGlyphCount++;
            }
        }

        UpdateMaterial();
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        UpdateMaterial();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        _isPointerDown = false;
        UpdateMaterial();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        UpdateMaterial();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        UpdateMaterial();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        UpdateMaterial();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        _isPointerDown = false;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        Material target;

        if (_isPointerDown && _pressedMaterial != null)
            target = _pressedMaterial;
        else if (_isSelected && _selectedMaterial != null)
            target = _selectedMaterial;
        else if (_isPointerOver && _hoveredMaterial != null)
            target = _hoveredMaterial;
        else
            target = _normalMaterial ?? _material;

        for (int i = 0; i < _activeGlyphCount; i++)
            transform.GetChild(i).GetComponent<Image>().material = target;
    }

    private Image GetOrCreateGlyph(int index)
    {
        while (index >= transform.childCount)
        {
            GameObject go = new GameObject("Glyph");
            go.transform.SetParent(transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = Vector2.zero;

            go.AddComponent<Image>();
        }
        return transform.GetChild(index).GetComponent<Image>();
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
        _activeGlyphCount = 0;
    }

    private void ApplyColorToActiveGlyphs()
    {
        for (int i = 0; i < _activeGlyphCount; i++)
            transform.GetChild(i).GetComponent<Image>().color = _color;
    }

    private void ApplyMaterialToActiveGlyphs()
    {
        for (int i = 0; i < _activeGlyphCount; i++)
            transform.GetChild(i).GetComponent<Image>().material = _material;
    }
}
