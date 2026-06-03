using UnityEngine;

public class CanyonSpawn : MonoBehaviour
{
    [SerializeField] private string _label;

    public string Label => _label;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.8f);
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
