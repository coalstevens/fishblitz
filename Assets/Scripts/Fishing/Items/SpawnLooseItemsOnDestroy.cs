using UnityEngine;

/// <summary>
/// This function excludes when the object is destroyed by scene exit.
/// </summary>
public class SpawnLooseItemsOnDisable : MonoBehaviour
{
    [SerializeField] bool _spawnOnDisable = true;
    [SerializeField] private SpawnItems.SpawnItemData[] _itemsToSpawn;
    [SerializeField] private Collider2D _spawnArea;

    [Header("Object Spawn Velocity Settings")]
    [SerializeField] float _speed = 1;
    [SerializeField] float _drag = 1;

    private void OnDisable()
    {
        if (!_spawnOnDisable)
            return;
        if (gameObject.scene.isLoaded)
        {
            _spawnArea.enabled = true;
            SpawnItems.SpawnItemsFromCollider(_spawnArea, _itemsToSpawn, SpawnItems.LaunchDirection.ANY, _speed, _drag);
        }
    }
}
