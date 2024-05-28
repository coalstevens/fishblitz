using UnityEngine;
using UnityEngine.UI;

public class SpawnLooseItemsOnDestroy : MonoBehaviour
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
            GameObject _spawnItem = Resources.Load<GameObject>("Items/" + _item.identifier);
            if (_spawnItem == null)
            {
                Debug.LogError("The spawn item doesn't exist.");
                continue;
            }
            Sprite _itemImage = _spawnItem.GetComponent<Image>().sprite;

            foreach (var _spawnPosition in GetRandomPositionsWithinCollider(_item.quantity))
            {
                GameObject _spawnedItem = SpawnLooseItem(new SpawnItemData(_item.identifier, 1), _itemImage, _spawnPosition);
                SetLaunchSpeed(_spawnedItem);
            }
        }
    }

    private GameObject SpawnLooseItem(SpawnItemData item, Sprite itemSprite, Vector3 spawnPosition) {
        // Spawn a generic loose item
        GameObject _spawnedItem = Instantiate(Resources.Load<GameObject>("Items/LooseItem"),
                                                spawnPosition,
                                                Quaternion.identity,
                                                _sceneItemSpawnContainer);
        
        // Apply item sprite to loose item
        _spawnedItem.GetComponentInChildren<SpriteRenderer>().sprite = itemSprite;

        // Transfer item info
        _spawnedItem.GetComponent<LooseItem>().Item = item;
        return _spawnedItem;
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
