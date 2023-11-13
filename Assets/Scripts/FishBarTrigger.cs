using UnityEngine;

public class FishBarTrigger : MonoBehaviour
{
    [SerializeField] private Sprite _fulfilledSprite;
    [SerializeField] private Sprite _unfulfilledSprite;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SendMessageUpwards("PassedFishBarTrigger", this);
    }

    public void SetSprite(bool fulfilled)
    {
        _spriteRenderer.sprite = fulfilled ? _fulfilledSprite : _unfulfilledSprite;
    }

    public Collider2D GetCollider()
    {
        return _collider;
    }
}
