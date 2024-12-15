using UnityEngine;
using UnityEngine.UI;
public static class SpawnItems
{
    [System.Serializable]
    public class ItemSpawnData
    {
        public ItemSpawnData(string identifier, int minQuantity, int maxQuantity)
        {
            this.identifier = identifier;
            this.minQuantity = minQuantity;
            this.maxQuantity = maxQuantity;
        }
        public string identifier;
        public int minQuantity;
        public int maxQuantity;
    }

    public static void SpawnItemsFromCollider(Collider2D collider, ItemSpawnData[] itemsToSpawn, float launchSpeed, float launchDrag)
    {
        foreach (var _item in itemsToSpawn)
        {
            GameObject _spawnItem = Resources.Load<GameObject>("Items/" + _item.identifier);
            if (_spawnItem == null)
            {
                Debug.LogError("The spawn item doesn't exist.");
                continue;
            }
            Sprite _itemImage = _spawnItem.GetComponent<Image>().sprite;

            foreach (var _spawnPosition in GetRandomPositionsWithinCollider(collider, Random.Range(_item.minQuantity, _item.maxQuantity)))
            {
                GameObject _spawnedItem = InstantiateLooseItem(new Inventory.ItemData(_item.identifier, 1), _itemImage, _spawnPosition);
                SetLaunchSpeed(_spawnedItem, launchSpeed, launchDrag);
            }
        }
    }

    public static void SpawnLooseItems(string itemName, Vector3[] spawnPositions, bool playBounceAnimation = true, float launchSpeed = 1, float launchDrag = 1)
    {
        GameObject _spawnItem = Resources.Load<GameObject>("Items/" + itemName);
        if (_spawnItem == null)
        {
            Debug.LogError("The spawn item doesn't exist.");
            return;
        }
        Sprite _itemImage = _spawnItem.GetComponent<Image>().sprite;

        foreach (var _spawnPosition in spawnPositions)
        {
            GameObject _spawnedItem = InstantiateLooseItem(new Inventory.ItemData(itemName, 1), _itemImage, _spawnPosition);
            if (!playBounceAnimation)
                _spawnedItem.GetComponent<Animator>().Play("Idle"); // skips bounce animation
            SetLaunchSpeed(_spawnedItem, launchSpeed, launchDrag);
        }
    }

    private static GameObject InstantiateLooseItem(Inventory.ItemData item, Sprite itemSprite, Vector3 spawnPosition)
    {
        // Spawn a generic loose item
        GameObject _spawnedItem = Object.Instantiate(Resources.Load<GameObject>("Items/LooseItem"),
                                                     spawnPosition,
                                                     Quaternion.identity,
                                                     GameObject.FindGameObjectWithTag("LooseItems").transform);

        // Apply item sprite to loose item
        _spawnedItem.GetComponentInChildren<SpriteRenderer>().sprite = itemSprite;

        // Transfer item info
        _spawnedItem.GetComponent<LooseItem>().Item = item;
        return _spawnedItem;
    }

    private static Vector3[] GetRandomPositionsWithinCollider(Collider2D collider, int quantity)
    {
        // Get bounds, collider has to be enabled
        Bounds _bounds;
        if (collider.enabled)
            _bounds = collider.bounds;
        else
        {
            collider.enabled = true;
            _bounds = collider.bounds;
            collider.enabled = false;
        }

        // Find positions
        Vector3[] _positions = new Vector3[quantity];
        for (int i = 0; i < quantity; i++)
        {
            _positions[i] = new Vector3(Random.Range(_bounds.min.x, _bounds.max.x),
                                        Random.Range(_bounds.min.y, _bounds.max.y),
                                        0);
        }
        return _positions;
    }

    private static void SetLaunchSpeed(GameObject _launchedObject, float launchSpeed, float launchDrag)
    {
        Rigidbody2D _rb = _launchedObject.GetComponent<Rigidbody2D>();
        _rb.drag = launchDrag;
        _rb.velocity = GenerateRandomDirection() * launchSpeed;
    }

    private static Vector3 GenerateRandomDirection()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        randomDirection.Normalize();

        return randomDirection;
    }
}