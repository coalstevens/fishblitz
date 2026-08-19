using UnityEngine;

public class BoxLidCollider : MonoBehaviour
{
    public bool IsPlayerInside { get; private set; }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
            IsPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
            IsPlayerInside = false;
    }
}
