using UnityEngine;

public class BoxAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _transitionSpeed = 2f;
    [SerializeField] private float _overlapRadius = 1.5f;
    [SerializeField] private Vector2 _overlapOffset = new Vector2(0f, 0.5f);
    [SerializeField] private LayerMask _playerLayer;

    private float _transitionProgress;
    private bool _isComplete;
    private bool _alertCleared;

    public float GetClipLength(string clipName) => _animator.GetClipLength(clipName);

    private void Awake()
    {
        _transitionProgress = 0;
    }

    private void Update()
    {
        if (_isComplete || !_alertCleared) 
            return;

        bool isPlayerInside = IsPlayerInLidRange();

        float target = isPlayerInside ? 1f : 0f;
        _transitionProgress = Mathf.MoveTowards(_transitionProgress, target, _transitionSpeed * Time.deltaTime);

        if (_transitionProgress >= 0.99f)
        {
            _transitionProgress = 1f;
            _animator?.Play("Open");
        }
        else if (_transitionProgress <= 0.01f)
        {
            _transitionProgress = 0f;
            _animator?.Play("Closed");
        }
        else
        {
            _animator?.Play("Opening", 0, _transitionProgress);
        }
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
        _transitionProgress = 0f;
        _animator.Play("Closed");
    }
}
