using UnityEngine;

public class BoxAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _overlapRadius = 1.5f;
    [SerializeField] private Vector2 _overlapOffset = new Vector2(0f, 0.5f);
    [SerializeField] private LayerMask _playerLayer;

    private bool _isComplete;
    private bool _alertCleared;

    public float GetClipLength(string clipName) => _animator.GetClipLength(clipName);

    private void Update()
    {
        if (_isComplete || !_alertCleared || _animator == null)
            return;

        bool isPlayerInside = IsPlayerInLidRange();

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Opening") || state.IsName("Closing"))
        {
            if (state.normalizedTime >= 1f)
                _animator.Play(state.IsName("Opening") ? "Open" : "Closed");
            return;
        }

        if (state.IsName("Closed") && isPlayerInside)
            _animator.Play("Opening");
        else if (state.IsName("Open") && !isPlayerInside)
            _animator.Play("Closing");
    }

    private bool IsPlayerInLidRange()
    {
        Vector2 center = (Vector2)transform.position + _overlapOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, _overlapRadius, _playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root.CompareTag("Player"))
                return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + _overlapOffset;
        Gizmos.DrawWireSphere(center, _overlapRadius);
    }

    public void SetAlertCleared()
    {
        _alertCleared = true;
    }

    public void PlayWin()
    {
        _isComplete = true;
        _animator.Play("Win");
    }

    public void SetComplete()
    {
        _isComplete = true;
    }

    public void ResetToClosed()
    {
        _animator.Play("Closed");
    }
}
