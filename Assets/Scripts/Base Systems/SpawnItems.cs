using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public static class SpawnItems
{
    private const float DEFAULT_LAUNCH_SPEED = 3f;
    private const float DEFAULT_LAUNCH_DRAG = 3f;

    public enum LaunchDirection { UP, DOWN, LEFT, RIGHT, ANY };

    [System.Serializable]
    public class SpawnItemData
    {
        public SpawnItemData(Inventory.ItemType item, int minQuantity, int maxQuantity)
        {
            this.ItemType = item;
            this.MinQuantity = minQuantity;
            this.MaxQuantity = maxQuantity;
        }
        public Inventory.ItemType ItemType;
        public int MinQuantity;
        public int MaxQuantity;
    }

    public static void SpawnItemsFromCollider(Collider2D collider, Inventory.ItemType itemType, int quantity, LaunchDirection launchDirection = LaunchDirection.ANY)
    {
        SpawnItemData[] _spawnItems = { new SpawnItemData(itemType, quantity, quantity) };
        SpawnItemsFromCollider(collider, _spawnItems, launchDirection);
    }

    public static void SpawnItemsFromCollider(Collider2D collider, SpawnItemData[] itemsToSpawn, LaunchDirection launchDirection = LaunchDirection.ANY, float launchSpeed = DEFAULT_LAUNCH_SPEED, float launchDrag = DEFAULT_LAUNCH_DRAG)
    {
        foreach (var _item in itemsToSpawn)
        {
            Inventory.ItemType _spawnItem = FetchItem(_item.ItemType.ItemName);

            foreach (var _spawnPosition in GetRandomPositionsWithinCollider(collider, UnityEngine.Random.Range(_item.MinQuantity, _item.MaxQuantity)))
            {
                GameObject _spawnedItem = InstantiateLooseItem
                (
                    new Inventory.ItemData(_spawnItem, 1),
                    _spawnItem.ItemSprite,
                    _spawnPosition
                );
                SetLaunchSpeed(_spawnedItem, launchDirection, launchSpeed, launchDrag);
            }
        }
    }

    public static void SpawnLooseItems(Inventory.ItemType itemType, Vector3[] spawnPositions, bool playBounceAnimation = true, LaunchDirection launchDirection = LaunchDirection.ANY, float launchSpeed = DEFAULT_LAUNCH_SPEED, float launchDrag = DEFAULT_LAUNCH_DRAG)
    {
        Inventory.ItemType _spawnItem = FetchItem(itemType.ItemName);

        foreach (var _spawnPosition in spawnPositions)
        {
            GameObject _spawnedItem = InstantiateLooseItem
            (
                new Inventory.ItemData(_spawnItem, 1),
                _spawnItem.ItemSprite,
                _spawnPosition
            );
            if (!playBounceAnimation)
                _spawnedItem.GetComponent<Animator>().Play("Idle"); // skips bounce animation
            SetLaunchSpeed(_spawnedItem, launchDirection, launchSpeed, launchDrag);
        }
    }

    private static Inventory.ItemType FetchItem(string itemName)
    {
        Inventory.ItemType _spawnItem = Resources.Load<Inventory.ItemType>($"Items/{itemName}");
        if (_spawnItem == null)
            Debug.LogError("The spawn item doesn't exist.");
        return _spawnItem;
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
            _positions[i] = new Vector3(UnityEngine.Random.Range(_bounds.min.x, _bounds.max.x),
                                        UnityEngine.Random.Range(_bounds.min.y, _bounds.max.y),
                                        0);
        }
        return _positions;
    }

    private static void SetLaunchSpeed(GameObject _launchedObject, LaunchDirection launchDirection, float launchSpeed, float launchDrag)
    {
        Rigidbody2D _rb = _launchedObject.GetComponent<Rigidbody2D>();
        _rb.drag = launchDrag;
        _rb.velocity = GenerateLaunchDirection(launchDirection) * launchSpeed;
    }

    private static Vector3 GenerateLaunchDirection(LaunchDirection direction)
    {
        float randomAngle = direction switch {
            LaunchDirection.UP => UnityEngine.Random.Range(Mathf.PI / 4f, Mathf.PI * 3f/4f),
            LaunchDirection.RIGHT => UnityEngine.Random.Range(-Mathf.PI / 4f, Mathf.PI / 4f),
            LaunchDirection.LEFT => UnityEngine.Random.Range(Mathf.PI * 3f / 4f, Mathf.PI * 5f/ 4f),
            LaunchDirection.DOWN => UnityEngine.Random.Range(Mathf.PI * 5f/ 4f, Mathf.PI * 7f/ 4f), 
            LaunchDirection.ANY => UnityEngine.Random.Range(0f, Mathf.PI * 2f),
            _ => UnityEngine.Random.Range(0f, Mathf.PI * 2f),
        };

        Vector3 randomDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        randomDirection.Normalize();

        return randomDirection;
    }
}