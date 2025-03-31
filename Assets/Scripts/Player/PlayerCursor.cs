using System;
using UnityEngine;
using ReactiveUnity;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// Handles the Player world cursor.
/// Only active in scenes with a grid.
/// There are 4 cursor gameObjects, one for each cardinal direction.
/// Only the cursor matching the player facing direction should be active.
/// </summary>
public class PlayerCursor : MonoBehaviour
{
    [SerializeField] public Transform _renderedTransform;
    [SerializeField] private FacingDirection _cursorActiveDirection;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SortingGroup _playerSpriteSortingGroup;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private bool _cursorVisible = false;
    public Collider2D Collider
    {
        get => _collider;
    }
    private PlayerMovementController _playerMovementController;
    private Grid _grid;
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange((prev, curr) => OnDirectionChange(curr)));
        _unsubscribeHooks.Add(_playerMovementController.PlayerState.OnChange((prev, curr) => TryHideCursor(curr)));

        OnDirectionChange(_playerMovementController.FacingDirection.Value);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _grid = GameObject.FindFirstObjectByType<Grid>();
    }

    private void TryHideCursor(PlayerMovementController.PlayerStates playerState)
    {
        // If player is in an Acting State, hide the cursor
        if (playerState != PlayerMovementController.PlayerStates.Idle && playerState != PlayerMovementController.PlayerStates.Walking)
        {
            _spriteRenderer.enabled = false;
            return;
        }

        bool playerFacingCursor = _playerMovementController.FacingDirection.Value == _cursorActiveDirection;
        if (_cursorVisible)
            _spriteRenderer.enabled = playerFacingCursor;
    }

    private void OnDirectionChange(FacingDirection currentDirection)
    {
        if (_cursorVisible)
            _spriteRenderer.enabled = currentDirection == _cursorActiveDirection;
        else
            _spriteRenderer.enabled = false;
        _collider.enabled = currentDirection == _cursorActiveDirection;
    }

    private void Update()
    {
        if (_grid != null)
        {
            _renderedTransform.position = _grid.WorldToCell(transform.position);
            _renderedTransform.GetComponent<SpriteRenderer>().sortingOrder = _playerSpriteSortingGroup.sortingOrder;
        }
    }
}