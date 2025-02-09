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
    [SerializeField] private AbandonedShedData _shedData;
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
    Bounds _perchBounds;
    private Collider2D _repairCollider; // also the vines collider
    private SpriteRenderer _renderer;
    private int _vineChopCount = 0;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _repairCollider = GetComponent<Collider2D>();
        _perchBounds = _perch.bounds;
        _renderer.sprite = _repairStates[_shedData.RepairProgress].Sprite;
        
        if (_shedData.AreVinesDestroyed)
            _vines.gameObject.SetActive(false);
        else
            _vines.GetComponent<SpriteRenderer>().sortingOrder = _renderer.sortingOrder + 1;
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

        if (!_shedData.AreVinesDestroyed) return true;
        bool _repairsComplete = _shedData.RepairProgress >= _repairStates.Count;
        if (_repairsComplete) return false;

        // check for hammer
        RepairState _nextState = _repairStates[_shedData.RepairProgress + 1];
        if (!_playerInventory.IsPlayerHoldingItem("Hammer"))
        {
            PlayerDialogueController.Instance.PostMessage($"I could fix the {_nextState.RepairName} if I had a hammer");
            return true;
        }

        // check for material
        if (!_playerInventory.TryRemoveItem(_nextState.ItemType.ItemName, _nextState.Quantity))
        {
            PlayerDialogueController.Instance.PostMessage($"I need {_nextState.Quantity} {_nextState.ItemType.ItemName} to fix the {_nextState.RepairName}");
            return true;
        }

        // fix the thing
        _shedData.RepairProgress++;
        NarratorSpeechController.Instance.PostMessage($"The {_repairStates[_shedData.RepairProgress].RepairName} has been repaired.");
        _renderer.sprite = _repairStates[_shedData.RepairProgress].Sprite;
        _shedData.NamesOfRepaired.Add(_repairStates[_shedData.RepairProgress].RepairName);
        return true;
    }

    public void OnUseAxe()
    {
        if (_shedData.AreVinesDestroyed) return;

        _vineChopCount++;
        ShakeVines();

        if (_vineChopCount >= _vineChopsToDestroy)
        {
            _shedData.AreVinesDestroyed = true;
            SpawnItems.SpawnItemsFromCollider(_repairCollider, _vineDestroySpawnItem, _spawnItemQuantity, SpawnItems.LaunchDirection.DOWN);
            _vines.gameObject.SetActive(false);
        }
    }

    private void ShakeVines()
    {
        if (_vines.gameObject.activeInHierarchy)
            _vines.DOShakePosition(_chopShakeDuration, new Vector3(_chopShakeStrength, 0, 0), _chopShakeVibrato, _chopShakeRandomness);
    }
}
