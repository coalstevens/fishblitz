using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// the roof is huge so gonna let infinite birds land on it.
// hopefully thats not an issue
public class AbandonedShed : MonoBehaviour, BirdBrain.IPerchableHighElevation, PlayerInteractionManager.IInteractable, Axe.IUseableWithAxe
{
    [Serializable]
    private class RepairState
    {
        public Sprite Sprite;
        public Inventory.ItemType ItemType;
        public int Quantity;
        public string RepairName;
    }

    [Header("General")]
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] Collider2D _perch;
    [SerializeField] private Transform _vines;
    [SerializeField] List<RepairState> _repairStates = new();
    [SerializeField] private int _vineChopsToDestroy = 5;
    [SerializeField] private Inventory.ItemType _vineDestroySpawnItem;
    [SerializeField] private int _spawnItemQuantity = 3;
    
    [Header("Vine Chop Shake Properties")]
    [SerializeField] protected float _chopShakeDuration = 0.5f;
    [SerializeField] protected float _chopShakeStrength = 0.05f;
    [SerializeField] protected int _chopShakeVibrato = 10;
    [SerializeField] protected float _chopShakeRandomness = 90f;
    private int _repairProgress = 0;
    Bounds _perchBounds;
    private Collider2D _repairCollider; // also the vines collider
    private SpriteRenderer _renderer;
    private bool _areVinesDestroyed = false;
    private int _vineChopCount = 0;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _repairCollider = GetComponent<Collider2D>();
        _perchBounds = _perch.bounds;
    }

    public Vector2 GetPositionTarget()
    {
        // Return random point in collider
        Vector2 randomPoint;
        do
        {
            float x = UnityEngine.Random.Range(_perchBounds.min.x, _perchBounds.max.x);
            float y = UnityEngine.Random.Range(_perchBounds.min.y, _perchBounds.max.y);
            randomPoint = new Vector2(x, y);
        } while (!_perch.OverlapPoint(randomPoint));

        return randomPoint;
    }

    public int GetSortingOrder()
    {
        return _renderer.sortingOrder;
    }

    public bool IsThereSpace()
    {
        return true;
    }

    public void OnBirdEntry(BirdBrain bird)
    {
        // do nothing
    }

    public void OnBirdExit(BirdBrain bird)
    {
        // do nothing
    }

    public void ReserveSpace(BirdBrain bird)
    {
        // do nothing
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        // Check if vines gone
        if (!_areVinesDestroyed) {
            return true;
        }

        // All repairs done
        if (_repairProgress >= _repairStates.Count)
        {
            return false;
        } 

        // Check for hammer
        RepairState _nextState = _repairStates[_repairProgress];

        if (!_playerInventory.IsPlayerHoldingItem("Hammer"))
        {
            PlayerDialogueController.Instance.PostMessage($"I could fix the {_nextState.RepairName} if I had a hammer");
            return true;
        }

        // Player doesn't have enough material
        if (!_playerInventory.TryRemoveItem(_nextState.ItemType.ItemName, _nextState.Quantity))
        {
            PlayerDialogueController.Instance.PostMessage($"I need {_repairStates[_repairProgress].Quantity} {_repairStates[_repairProgress].ItemType.ItemName} to fix the {_repairStates[_repairProgress].RepairName}");
            return true;
        }

        Debug.Log("Fixing the thing"); 
        // Fix the thing
        _renderer.sprite = _repairStates[_repairProgress].Sprite;
        _repairProgress++;
        return true;
    }

    public void OnUseAxe()
    {
        if (_areVinesDestroyed)
            return;

        _vineChopCount++;
        ShakeVines();

        if (_vineChopCount >= _vineChopsToDestroy)
        {
            _areVinesDestroyed = true;
            SpawnItems.SpawnItemsFromCollider(_repairCollider, _vineDestroySpawnItem, _spawnItemQuantity, SpawnItems.LaunchDirection.DOWN);
            _vines.gameObject.SetActive(false);
        } 
    }

    protected void ShakeVines()
    {
        _vines.DOShakePosition(_chopShakeDuration, new Vector3(_chopShakeStrength, 0, 0), _chopShakeVibrato, _chopShakeRandomness);
    }
}
