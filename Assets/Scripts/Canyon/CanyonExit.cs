using UnityEngine;

public class CanyonExit : MonoBehaviour
{
    [SerializeField] private string _targetBiome;

    public string ExitId => gameObject.name;
    public string TargetBiome => _targetBiome;

    public CanyonSpawn GetSpawn(string label)
    {
        foreach (Transform child in transform)
            if (child.TryGetComponent<CanyonSpawn>(out var spawn) && spawn.Label == label)
                return spawn;
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _targetBiome switch
        {
            "Canyon" => new Color(0.8f, 0.5f, 0.2f),
            "Forest" => new Color(0.2f, 0.8f, 0.3f),
            "Cave" => new Color(0.4f, 0.3f, 0.7f),
            _ => Color.white
        };
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
