using UnityEngine;
using UnityEngine.UI;

public class Axe : MonoBehaviour, ITool, IInventoryItem
{
    private PlayerMovementController _playerMovementController;
    private const string ITEM_HAME = "Axe";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 1;
    [SerializeField] protected AudioClip _chopSFX;
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

    bool ITool.UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is IUseableWithAxe _worldObject)
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Axing;
            _worldObject.OnUseAxe();
            return true;
        }
        return false;
    }

    void ITool.SwingTool() {
        _playerMovementController.PlayerState.Value = PlayerStates.Axing;
    }

    bool ITool.UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false; // does nothing
    }

    public void PlayToolHitSound()
    {   
        AudioManager.Instance.PlaySFX(_chopSFX, 0.4f);
    }
}
