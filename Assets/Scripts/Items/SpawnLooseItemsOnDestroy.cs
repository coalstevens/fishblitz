using UnityEngine;

/// <summary>
/// This function excludes when the object is destroyed by scene exit.
/// </summary>
public class SpawnLooseItemsOnDestroy : MonoBehaviour
{
    [SerializeField] bool _spawnOnDestroy = true;
    [SerializeField] private SpawnItems.ItemSpawnData[] _itemsToSpawn;
    [SerializeField] private Collider2D _spawnArea;

    [Header("Object Spawn Velocity Settings")]
    [SerializeField] float _speed = 1;
    [SerializeField] float _drag = 1;

    private void OnDestroy()
    {
        if (!_spawnOnDestroy)
            return;
        if (gameObject.scene.isLoaded)
            SpawnItems.SpawnItemsFromCollider(_spawnArea, _itemsToSpawn, _speed, _drag);
    }
}
