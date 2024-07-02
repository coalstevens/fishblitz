using System;
using UnityEngine;
using ReactiveUnity;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Handles the Player world cursor.
/// Only active in scenes with a grid.
/// There are 4 cursor gameObjects, one for each cardinal direction.
/// Only the cursor matching the player facing direction should be active.
/// </summary>
public class Cursor : MonoBehaviour
{
    [SerializeField] public Transform _renderedTransform;
    [SerializeField] private FacingDirections _cursorActiveDirection;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    public Collider2D Collider {
        get => _collider;
    }
    private PlayerMovementController _playerMovementController;
    private Grid _grid;
    private SpriteRenderer _playerSpriteRenderer;
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        // References
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _playerSpriteRenderer = GameObject.FindWithTag("Player").GetComponent<SpriteRenderer>();

        // Subscriptions
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange((prev, curr) => OnDirectionChange(curr)));
        _unsubscribeHooks.Add(_playerMovementController.PlayerState.OnChange((prev,curr) => TryHideCursor(curr)));

        OnDirectionChange(_playerMovementController.FacingDirection.Value);
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _grid = GameObject.FindObjectOfType<Grid>();
    }

    private void TryHideCursor(PlayerStates playerState)
    {
        // If player is in an Acting State, hide the cursor
        if (playerState != PlayerStates.Idle && playerState != PlayerStates.Walking) {
            _spriteRenderer.enabled = false;
            return;
        }
        
        bool playerFacingCursor = _playerMovementController.FacingDirection.Value == _cursorActiveDirection;
        _spriteRenderer.enabled = playerFacingCursor;
    }

    private void OnDirectionChange(FacingDirections currentDirection)
    {
        _spriteRenderer.enabled = currentDirection == _cursorActiveDirection;
        _collider.enabled = currentDirection == _cursorActiveDirection;
    }

    private void Update()
    {
        if (_grid != null) {
            _renderedTransform.position = _grid.WorldToCell(transform.position);
            _renderedTransform.GetComponent<SpriteRenderer>().sortingOrder = _playerSpriteRenderer.sortingOrder;
        }
    }
}