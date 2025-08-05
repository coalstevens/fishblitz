using UnityEngine;

public class UniqueMaterialInstance : MonoBehaviour
{
    [SerializeField] private Material _sourceMaterial;
    private void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.material = new Material(_sourceMaterial);

        Texture2D texture = renderer.sprite.texture;
        renderer.material.SetTexture("_MainTex", texture);
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Destroy(GetComponent<SpriteRenderer>().material);
        }
    }
}
