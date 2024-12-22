using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRod", menuName = "Items/Rod")]
public class Rod : Inventory.ItemType, PlayerInteractionManager.ITool
{
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;
    private Coroutine _changeStateRoutine;
    private FishBar _fishBar;

    void Awake()
    {
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
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
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Fishing) {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
            //StopCoroutine(_changeStateRoutine);
            return true;
        }

        // if cursor is on water, start fishing
        if (tilemapLayerName == "Water") {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Fishing;
            //_changeStateRoutine = StartCoroutine(WaitForFishToBite());
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
