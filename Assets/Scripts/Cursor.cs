using System;
using UnityEngine;
using ReactiveUnity;

public class Cursor : MonoBehaviour
{
  [SerializeField] private Grid _grid;
  [SerializeField] public Transform _renderedTransform;
  [SerializeField] private Direction _activeDirection;
  [SerializeField] private SpriteRenderer _spriteRenderer;
  private PlayerMovementController _playerMovementController;
  private SpriteRenderer _playerSpriteRenderer;

  private void Start()
  {
    _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
    _playerSpriteRenderer = GameObject.FindWithTag("Player").GetComponent<SpriteRenderer>();
    _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
    _playerMovementController.Fishing.OnChange((prev, curr) => OnFishingChange(curr));
  }

  private void OnFishingChange(bool curr)
  {
    bool maybeTrue = _playerMovementController.FacingDir.Value == _activeDirection;
    _spriteRenderer.enabled = curr ? false : maybeTrue;
  }

  private void OnDirectionChange(Direction curr)
  {
    if (curr == _activeDirection)
    {
      _spriteRenderer.enabled = true;
    }
    else
    {
      _spriteRenderer.enabled = false;
    }
  }

  private void Update()
  {
    _renderedTransform.position = _grid.WorldToCell(transform.position);
    _renderedTransform.GetComponent<SpriteRenderer>().sortingOrder = _playerSpriteRenderer.sortingOrder;
  }
}