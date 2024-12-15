using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Rod : MonoBehaviour, PlayerInteractionManager.ITool, Inventory.IItem
{
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;
    private PlayerMovementController _playerMovementController;
    private Coroutine _changeStateRoutine;
    private FishBar _fishBar;
    private const string ITEM_HAME = "Rod";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 1;
    public int StackCapacity {get {return STACK_CAPACITY;}}
    public string ItemName { get {return ITEM_HAME;} }
    public Sprite ItemSprite { 
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }
    public int Quantity { 
        get => _quantity;
        set => _quantity = value;
    } 

    void Start()
    {
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
    }
    private IEnumerator WaitForFishToBite()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishBar.Play();
            yield break;
        }
    }

    bool PlayerInteractionManager.ITool.UseToolOnInteractableTileMap(string tilemapLayerName, UnityEngine.Vector3Int cursorLocation)
    {
        // Debug.Log("Used rod on tilemap");
        // if fishing stop fishing
        if (_playerMovementController.PlayerState.Value == PlayerStates.Fishing) {
            _playerMovementController.PlayerState.Value = PlayerStates.Idle;
            StopCoroutine(_changeStateRoutine);
            return true;
        }

        // if cursor is on water, start fishing
        if (tilemapLayerName == "Water") {
            _playerMovementController.PlayerState.Value = PlayerStates.Fishing;
            _changeStateRoutine = StartCoroutine(WaitForFishToBite());
            return true;
        }
        
        return false;
    }

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        Debug.Log("Used rod on world object");
        return false; // does nothing
    }

    public void SwingTool()
    {
        Debug.Log("Swung fishing rod");
        return; // does nothing. Make a casting animation?
    }

    public void PlayToolHitSound()
    {
        // has no sound
    }
}
