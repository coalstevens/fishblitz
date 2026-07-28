using UnityEngine;

public class TheWayNorthExit : MonoBehaviour
{
    public enum ForkDirection { None, Left, Right }

    [SerializeField] private ForkDirection _forkDirection;

    public string ExitId => gameObject.name;
    public ForkDirection ForkDir => _forkDirection;

    public TheWayNorthSpawn GetSpawn(string label)
    {
        foreach (Transform child in transform)
            if (child.TryGetComponent<TheWayNorthSpawn>(out var spawn) && spawn.Label == label)
                return spawn;
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
