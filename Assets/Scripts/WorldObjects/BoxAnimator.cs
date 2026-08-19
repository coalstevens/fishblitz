using UnityEngine;

public class BoxAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _transitionSpeed = 2f;

    private BoxLidCollider _lidCollider;
    private float _transitionProgress;
    private bool _isComplete;
    private bool _alertCleared;

    public float GetClipLength(string clipName) => _animator.GetClipLength(clipName);

    private void Awake()
    {
        _transitionProgress = 0;
        _lidCollider = GetComponentInChildren<BoxLidCollider>();
    }

    private void Update()
    {
        if (_isComplete || !_alertCleared) return;

        float target = _lidCollider.IsPlayerInside ? 1f : 0f;
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

    public void ResetState()
    {
        _isComplete = false;
        ResetToClosed();
    }
}
