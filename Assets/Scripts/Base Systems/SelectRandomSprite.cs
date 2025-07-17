using UnityEngine;
using System.Collections.Generic;


public class SelectRandomSprite : MonoBehaviour
{
    [System.Serializable]
    public struct WeightedSprite
    {
        public Sprite sprite;
        public float weight;
    }
    [SerializeField] private List<WeightedSprite> sprites = new();

    private void Start()
    {
        if (sprites == null || sprites.Count == 0) return;

        float totalWeight = 0f;
        foreach (var ws in sprites)
            totalWeight += Mathf.Max(0, ws.weight);

        if (totalWeight <= 0f) return;

        // Use the GameObject's transform position as the seed
        Vector3 pos = transform.position;
        int positionSeed = pos.GetHashCode();
        var rnd = new System.Random(positionSeed);

        float r = (float)(rnd.NextDouble() * totalWeight);
        float accum = 0f;
        Sprite selected = null;
        foreach (var ws in sprites)
        {
            accum += Mathf.Max(0, ws.weight);
            if (r <= accum)
            {
                selected = ws.sprite;
                break;
            }
        }
        if (selected == null) selected = sprites[0].sprite;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = selected;
        }
    }
}
