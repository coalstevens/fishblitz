using UnityEngine;

public class SpawnItemsOnDestroy : MonoBehaviour
{
    [SerializeField] bool _spawnOnDestroy = true;
    [SerializeField] private SpawnItemData[] _itemsToSpawn;
    [SerializeField] private Collider2D _spawnArea;

    [Header("Object Spawn Velocity Settings")]
    [SerializeField] float _speed = 1;
    [SerializeField] float _drag = 1;

    Transform _sceneItemSpawnContainer;
    Bounds _bounds;
    private void Awake()
    {
        // Collider has to be enabled to get the bounds
        if(_spawnArea.enabled)
            _bounds = _spawnArea.bounds;
        else 
        {
            _spawnArea.enabled = true;
            _bounds = _spawnArea.bounds;
            _spawnArea.enabled = false;
        }
        _sceneItemSpawnContainer = GameObject.FindGameObjectWithTag("LooseItems").transform;
    }
    private void OnDestroy()
    {
        if (!_spawnOnDestroy)
            return;

        SpawnItemsFromCollider();
    }

    public void SpawnItemsFromCollider()
    {
        foreach (var _item in _itemsToSpawn)
        {
            GameObject _spawnItem = Resources.Load<GameObject>("Items/" + _item.identifier + "_Loose");
            if (_spawnItem == null)
            {
                Debug.LogError("The spawn item could not be found");
                continue;
            }

            foreach (var _spawnPosition in GetRandomPositionsWithinCollider(_item.quantity))
            {
                GameObject _spawnedItem = Instantiate(_spawnItem,
                                                     _spawnPosition,
                                                     Quaternion.identity,
                                                     _sceneItemSpawnContainer);
                SetLaunchSpeed(_spawnedItem);
            }
        }
    }

    private void OnDisable() {
    }
    Vector3[] GetRandomPositionsWithinCollider(int quantity)
    {
        //var _bounds = _spawnArea.bounds;
        Debug.Log("Bounds: " + _bounds.min.x + ", " + _bounds.max.x + ", " + _bounds.min.y + ", " + _bounds.max.y);
        Vector3[] _positions = new Vector3[quantity];
        for (int i = 0; i < quantity; i++)
        {
            _positions[i] = new Vector3(Random.Range(_bounds.min.x, _bounds.max.x), 
                                        Random.Range(_bounds.min.y, _bounds.max.y), 
                                        0);
            Debug.Log("Position: " + _positions[i].x + ", " + _positions[i].y);
        }
        return _positions;
    }

    private void SetLaunchSpeed(GameObject _launchedObject)
    {
        Rigidbody2D _rb = _launchedObject.GetComponent<Rigidbody2D>();
        _rb.drag = _drag;
        _rb.velocity = GenerateRandomDirection() * _speed;
    }

    public Vector3 GenerateRandomDirection() {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        randomDirection.Normalize();

        return randomDirection;
    }
}
