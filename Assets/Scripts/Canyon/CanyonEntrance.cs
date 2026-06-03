using UnityEngine;

public class CanyonEntrance : MonoBehaviour
{
    public string EntranceId => gameObject.name;

    public CanyonSpawn GetSpawn(string label)
    {
        foreach (Transform child in transform)
            if (child.TryGetComponent<CanyonSpawn>(out var spawn) && spawn.Label == label)
                return spawn;
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f);
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
