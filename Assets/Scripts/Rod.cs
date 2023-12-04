using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rod : MonoBehaviour, ITool
{
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;
    private PlayerMovementController _playerMovementController;
    private Coroutine _changeStateRoutine;
    private FishBar _fishBar;    
    // Start is called before the first frame update
    void Start()
    {
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
    }
    public void UseTool(TileData tileData, Vector3 cursorLocation) {
        //if fishing stop fishing
        if (_playerMovementController.CurrState.Value == State.Fishing)
        {
            _playerMovementController.CurrState.Value = State.Idle;
            StopCoroutine(_changeStateRoutine);
            return;
        }

        if(tileData.gameObject == null) {
            return;
        }
 
        bool canFish = tileData.gameObject.GetComponent<RodPlacement>() != null;
        
        if (canFish) {
            _playerMovementController.CurrState.Value = State.Fishing;
            _changeStateRoutine = StartCoroutine(ChangeStateRoutine());
        }
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
}
