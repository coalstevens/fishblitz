using UnityEngine;

public class BeachedBoat : MonoBehaviour
{

    [SerializeField] private Sprite _boat;
    [SerializeField] private Sprite _damagedBoat;
    [SerializeField] private Sprite _damagedBoatWithHammer;
    private SpriteRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = _damagedBoatWithHammer;
    }

    public void RemoveHammer()
    {
        _renderer.sprite = _damagedBoat;
    }
}
