using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pixel Font", menuName = "Pixel Font")]
public class PixelFont : ScriptableObject
{
    [Serializable]
    public struct CharSpritePair
    {
        public char Character;
        public Sprite Sprite;
    }

    [SerializeField] private CharSpritePair[] _characters;
    [SerializeField] private int _spaceWidth = 4;
    [SerializeField] private int _tracking;

    public int Tracking => _tracking;

    public float PixelsPerUnit
    {
        get
        {
            if (_characters != null)
            {
                for (int i = 0; i < _characters.Length; i++)
                {
                    if (_characters[i].Sprite != null)
                        return _characters[i].Sprite.pixelsPerUnit;
                }
            }
            return 32f;
        }
    }

    public bool TryGetGlyph(char c, out Sprite sprite, out int pixelWidth)
    {
        if (c == ' ')
        {
            sprite = null;
            pixelWidth = _spaceWidth + _tracking;
            return true;
        }

        if (_characters != null)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i].Character == c && _characters[i].Sprite != null)
                {
                    sprite = _characters[i].Sprite;
                    pixelWidth = Mathf.RoundToInt(sprite.rect.width) + _tracking;
                    return true;
                }
            }
        }
        sprite = null;
        pixelWidth = 0;
        return false;
    }

    public int GetTotalPixelWidth(string text)
    {
        int total = 0;
        if (_characters != null)
        {
            foreach (char c in text)
            {
                if (c == ' ')
                {
                    total += _spaceWidth + _tracking;
                    continue;
                }

                for (int i = 0; i < _characters.Length; i++)
                {
                    if (_characters[i].Character == c && _characters[i].Sprite != null)
                    {
                        total += Mathf.RoundToInt(_characters[i].Sprite.rect.width) + _tracking;
                        break;
                    }
                }
            }
        }
        return total;
    }
}
