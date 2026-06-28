using System.Collections.Generic;
using UnityEngine;

public class PixelTextRenderer : MonoBehaviour
{
    public enum Alignment { Left, Center, Right }

    [SerializeField] private PixelFont _font;
    [SerializeField] private string _text = "";
    [SerializeField] private Alignment _alignment = Alignment.Left;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private int _sortingOrder = 10;
    [SerializeField] private string _sortingLayerName = "Default";

    private List<SpriteRenderer> _glyphPool = new();
    private int _activeGlyphCount = 0;
    private float _totalWorldWidth = 0f;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value ?? "";
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

    private void OnDestroy()
    {
        for (int i = _glyphPool.Count - 1; i >= 0; i--)
        {
            if (_glyphPool[i] != null)
                Destroy(_glyphPool[i].gameObject);
        }
        _glyphPool.Clear();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            Rebuild();
    }

    public void Rebuild()
    {
        if (_font == null || string.IsNullOrEmpty(_text))
        {
            HideAllGlyphs();
            _totalWorldWidth = 0f;
            return;
        }

        float ppu = _font.PixelsPerUnit;
        float unitsPerPixel = 1f / ppu;

        int totalPixelWidth = _font.GetTotalPixelWidth(_text);
        _totalWorldWidth = totalPixelWidth * unitsPerPixel;

        float startWorldX = _alignment switch
        {
            Alignment.Center => -_totalWorldWidth / 2f,
            Alignment.Right => -_totalWorldWidth,
            _ => 0f
        };

        int currentPixelX = 0;
        _activeGlyphCount = 0;

        for (int i = 0; i < _text.Length; i++)
        {
            char c = _text[i];
            if (!_font.TryGetGlyph(c, out Sprite sprite, out int advance))
                continue;

            SpriteRenderer glyph = GetOrCreateGlyph(_activeGlyphCount);
            glyph.sprite = sprite;
            glyph.color = _color;
            glyph.sortingOrder = _sortingOrder;
            glyph.sortingLayerName = _sortingLayerName;
            glyph.gameObject.SetActive(true);

            float worldX = startWorldX + currentPixelX * unitsPerPixel;
            glyph.transform.localPosition = new Vector3(worldX, 0f, 0f);

            currentPixelX += advance;
            _activeGlyphCount++;
        }

        for (int i = _activeGlyphCount; i < _glyphPool.Count; i++)
            _glyphPool[i].gameObject.SetActive(false);
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
}
