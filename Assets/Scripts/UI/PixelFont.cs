using UnityEngine;

[CreateAssetMenu(fileName = "New Pixel Font", menuName = "Pixel Font")]
public class PixelFont : ScriptableObject
{
    [SerializeField] private Sprite[] _glyphSprites = new Sprite[95];

    private const int START_INDEX = 32;

    public float PixelsPerUnit
    {
        get
        {
            if (_glyphSprites.Length == 0 || _glyphSprites[0] == null)
                return 32f;
            return _glyphSprites[0].pixelsPerUnit;
        }
    }

    public bool TryGetGlyph(char c, out Sprite sprite, out int pixelWidth)
    {
        int index = c - START_INDEX;
        if (index < 0 || index >= _glyphSprites.Length || _glyphSprites[index] == null)
        {
            sprite = null;
            pixelWidth = 0;
            return false;
        }
        sprite = _glyphSprites[index];
        pixelWidth = Mathf.RoundToInt(sprite.rect.width);
        return true;
    }

    public int GetTotalPixelWidth(string text)
    {
        int total = 0;
        foreach (char c in text)
        {
            int index = c - START_INDEX;
            if (index >= 0 && index < _glyphSprites.Length && _glyphSprites[index] != null)
                total += Mathf.RoundToInt(_glyphSprites[index].rect.width);
        }
        return total;
    }
}
