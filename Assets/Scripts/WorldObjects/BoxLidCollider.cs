using UnityEngine;

public class BoxLidCollider : MonoBehaviour
{
    private Box _box;

    private void Awake()
    {
        _box = GetComponentInParent<Box>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _box.OnPlayerProximityEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _box.OnPlayerProximityExit();
    }
}
