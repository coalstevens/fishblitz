using UnityEngine;

public class Cursor : MonoBehaviour
{
  [SerializeField] private Grid _grid;
  [SerializeField] public Transform _renderedTransform;
  [SerializeField] private Direction _activeDirection;
  [SerializeField] private PlayerMovementController _playerMovementController;
  [SerializeField] private SpriteRenderer _spriteRenderer;

  private void Start()
  {
    _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
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
  }
}