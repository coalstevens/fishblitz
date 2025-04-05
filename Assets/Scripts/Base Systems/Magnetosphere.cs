using System.Collections.Generic;
using UnityEngine;


public class Magnetosphere : MonoBehaviour
{
    [SerializeField] private Collider2D _collectCollider;
    [SerializeField] private float _strength = 1;
    [SerializeField] private Inventory _inventory;
    private Collider2D _playerMagnetosphere;
    private List<LooseItem> _looseItemsInRange = new();
    private List<LooseItem> _itemsToDestroy = new();

    void Start()
    {
        _playerMagnetosphere = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        //TODO: Case where player only has inventory space for some of the quantity of item, not all

        foreach (LooseItem _looseItem in _looseItemsInRange)
        {
            if (_looseItem.IsMagnetic && 
                _inventory.HasEnoughInventorySpace(_looseItem.Item.Item.ItemLabel, _looseItem.Item.Quantity))
            {
                ApplyMagneticForce(_looseItem);
                if (CollectItem(_looseItem))
                    _itemsToDestroy.Add(_looseItem);
            }
        }

        // Destroying LooseItems modifies _looseItemsInRange via OnTriggerExit2D()
        // so they must be destroyed outside the previous loop
        foreach (LooseItem _looseItem in _itemsToDestroy)
            Destroy(_looseItem.gameObject);
        _itemsToDestroy.Clear();
    }

    private bool CollectItem(LooseItem looseItem)
    {
        // Check if _magnetic item intersects collect collider
        if (!_collectCollider.bounds.Intersects(looseItem.GetComponent<Collider2D>().bounds))
            return false;

        // Try to add item to inventory. This should never fail as HasEnoughInventorySpace was already checked
        if (!_inventory.TryAddItem(looseItem.Item.Item, looseItem.Item.Quantity))
            return false;
        
        return true;
    }

    private void ApplyMagneticForce(LooseItem looseItem)
    {
        // Coulombs law: Fm = k(q1q2/r^2), 
        // therefore     Fm ~= strength / r^2 
        var _rb = looseItem.GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogError("This magnetic doesn't have a rigidbody.");

        // Calculate force
        float _radius = Vector3.Distance(looseItem.transform.position, _playerMagnetosphere.transform.position);
        float _magnitude = _strength / (_radius * _radius);
        Vector3 _direction = Vector3.Normalize(_playerMagnetosphere.transform.position - looseItem.transform.position);
        Vector3 _Fmagnet = _magnitude * _direction;

        _rb.AddForce(_Fmagnet, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<LooseItem>(out var _magnetic))
            _looseItemsInRange.Add(_magnetic);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent<LooseItem>(out var _magnetic))
            _looseItemsInRange.Remove(_magnetic);
    }
}

