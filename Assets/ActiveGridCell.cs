using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ActiveGridCell : MonoBehaviour
{
    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Inventory _inventory;
    private Coroutine _changeStateRoutine;

    [Header("Fishing Behavioural Options")]
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;
    private Cursor _activeCursor;
    private FishBar _fishBar;       
    // Start is called before the first frame update
    void Start()
    {
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
    }

    private void OnDirectionChange(Direction curr)
    {
        switch (curr)
        {
            case Direction.Up:
                _activeCursor = _cursorN;
                return;
            case Direction.Right:
                _activeCursor = _cursorE;
                return;
            case Direction.Down:
                _activeCursor = _cursorS;
                return;
            case Direction.Left:
                _activeCursor = _cursorW;
                return;
        }
    }

    public Vector3Int GetActiveCursorLocation()
    {
        return _grid.WorldToCell(_activeCursor.transform.position);
    }

    private void OnInteract() {
        System.Console.WriteLine("Test0");
        // no interrupting cele
        if (_playerMovementController.CurrState.Get() == State.Celebrating)
        {
            return;
        }
        
        // no interrupting catching
        if (_playerMovementController.CurrState.Get() == State.Catching)
        {
            return;
        }

        //if fishing stop fishing
        if (_playerMovementController.CurrState.Get() == State.Fishing)
        {
            _playerMovementController.CurrState.Set(State.Idle);
            StopCoroutine(_changeStateRoutine);
            return;
        }

        TileBase tile = _tilemap.GetTile(GetActiveCursorLocation());
        TileData tileData = new TileData();
        tile.GetTileData(GetActiveCursorLocation(), _tilemap, ref tileData);

        bool canFish = false;
        if (tileData.gameObject != null) 
        {
            canFish = tileData.gameObject.GetComponent<RodPlacement>() != null;
        }

        bool hasRod = _inventory.Rods > 0;
        System.Console.WriteLine("Test1");
        if (hasRod && canFish) {
            System.Console.WriteLine("Test2");
            _playerMovementController.CurrState.Set(State.Fishing);
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
    private void OnFire()
    {
        // no placements when ur fishing
        if (_playerMovementController.CurrState.Get() == State.Fishing || _playerMovementController.CurrState.Get() == State.Catching || _playerMovementController.CurrState.Get() == State.Celebrating)
        {
            return;
        }

        MountedFishingRod mountedRod = GetMountedRod();
        if (mountedRod != null)
        {
            mountedRod.StartFishingGame();
        }
        else
        {
            PlaceRod();
        }
    }

    private void PlaceRod()
    {
        TileBase tile = _tilemap.GetTile(GetActiveCursorLocation());
        TileData tileData = new TileData();
        tile.GetTileData(GetActiveCursorLocation(), _tilemap, ref tileData);

        bool canBePlaced = false;
        if (tileData.gameObject != null) 
        {
            canBePlaced = tileData.gameObject.GetComponent<RodPlacement>() != null;
        }
        
        bool hasRod = _inventory.MountedRods > 0;

        if (canBePlaced && hasRod)
        {
            Instantiate(tileData.gameObject.GetComponent<RodPlacement>().RodToPlace, (Vector3)GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            _inventory.MountedRods -= 1;
        }
    }

    private MountedFishingRod GetMountedRod()
    {
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapBox(GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), results);

        foreach (var result in results)
        {
            var rod = result.GetComponent<MountedFishingRod>();
            if (rod != null)
            {
                return rod;
            }
        }

        return null;
    }
}
