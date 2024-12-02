using UnityEngine;

public class BirdOutliner : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _defaultMaterial; // Material to use when no bird is inside
    [SerializeField] private Material _outlineMaterial;    // Material to use when a bird enters
    private void Awake()
    {
        if (_defaultMaterial == null || _outlineMaterial == null)
            Debug.LogError("Materials not set. Please assign materials in the Inspector.");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        BirdBrain _bird = other.GetComponent<BirdBrain>();
        if (_bird != null)
        {
            SpriteRenderer _renderer = _bird.transform.GetComponentInChildren<SpriteRenderer>();
            if (_renderer != null)
            {
                _renderer.material = _outlineMaterial;
            }
            else {
                Debug.Log("Renderer not found");
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.GetComponent<BirdBrain>() != null)
            other.GetComponentInChildren<SpriteRenderer>().material = _defaultMaterial;
    }

}