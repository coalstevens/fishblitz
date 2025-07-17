using UnityEngine;

[ExecuteInEditMode]
public class SpriteLocalCoords : MonoBehaviour {

    [SerializeField] private bool usePropertyBlock = true;
    // For URP 2023+ likely want set to false, as sprites support SRP batcher now
    // For Built-in RP / prior URP versions, may be better to keep true

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock mpb;

    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        Sprite sprite = spriteRenderer.sprite;
        Rect rect = sprite.textureRect;
        Vector2 texelSize = sprite.texture.texelSize;
        Vector4 uvRemap = new (
            rect.x * texelSize.x,
            rect.y * texelSize.y,
            rect.width * texelSize.x,
            rect.height * texelSize.y
        );
        
        if (!usePropertyBlock && Application.isPlaying){
            // Use Material Instance (during Play Mode only)
            spriteRenderer.material.SetVector("_UVRemap", uvRemap);
        } else {
            // Use Material Property Block
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetVector("_UVRemap", uvRemap);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }

    void OnDisable() {
        if (!usePropertyBlock && Application.isPlaying){
            // Clean up Material Instance
            Destroy(spriteRenderer.material);
        }
    }
}