using UnityEngine;

public class CheckForCore : MonoBehaviour
{
    private void Awake()
    {
        if (CoreManager.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Core");
            if (prefab != null)
                Instantiate(prefab);
            else
                Debug.LogError("CheckForCore: Could not load Core from Resources");
        }
        Destroy(gameObject);
    }
}
