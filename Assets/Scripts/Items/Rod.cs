using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Rod : MonoBehaviour, ITool, IInventoryItem
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
    private IEnumerator ChangeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishBar.Play();
            yield break;
        }
    }

    public void UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        // do nothing
    }

    public void UseToolOnTile(IInteractableTile interactableTile, Vector3Int cursorLocation)
    {
        //if fishing stop fishing
        if (_playerMovementController.PlayerState.Value == PlayerStates.Fishing) {
            _playerMovementController.PlayerState.Value = PlayerStates.Idle;
            StopCoroutine(_changeStateRoutine);
            return;
        }

        if (interactableTile is RodPlacement) {
            _playerMovementController.PlayerState.Value = PlayerStates.Fishing;
            _changeStateRoutine = StartCoroutine(ChangeStateRoutine());
        }
    }
}
