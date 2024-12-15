using UnityEngine;
using UnityEngine.UI;

public class Binoculars : MonoBehaviour, Inventory.IItem, PlayerInteractionManager.ITool
{
    private const string ITEM_NAME = "Binoculars";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 99;
    public int StackCapacity {get {return STACK_CAPACITY;}}
    public string ItemName { get {return ITEM_NAME;} }

    private BirdingGame _birdingGame;
    private PlayerMovementController _playerMovementController;

    private void Start() {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _birdingGame = GameObject.FindWithTag("Player").GetComponentInChildren<BirdingGame>(true);
    }

    public Sprite ItemSprite { 
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }

    public int Quantity { 
        get => _quantity;
        set => _quantity = value;
    }

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false;
    }

    public void SwingTool()
    {
        Debug.Log("Binoculars swung");
        if (_playerMovementController.PlayerState.Value == PlayerStates.Birding) {
            Debug.Log("Binoculars returned");
            return;
        }

        _playerMovementController.PlayerState.Value = PlayerStates.Birding;
        _birdingGame.Play();
    }

    public void PlayToolHitSound()
    {
        // no sound
    }
}
