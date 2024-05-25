using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Axe : MonoBehaviour, ITool, IInventoryItem
{
    private PlayerMovementController _playerMovementController;
    private const string ITEM_HAME = "Axe";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 1;
    public string ItemName { get { return ITEM_HAME; } }
    public int StackCapacity { get { return STACK_CAPACITY; } }
    public Sprite ItemSprite
    {
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }
    public int Quantity
    {
        get => _quantity;
        set => _quantity = value;
    }
    
    private void Start() {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
    }

    public void UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is IUseableWithAxe _worldObject)
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Axing;
            _worldObject.OnUseAxe();
        }
    }

    public void UseToolOnTile(IInteractableTile interactableTile, Vector3Int cursorLocation)
    {
        // do nothing
    }
}
