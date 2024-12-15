using System;
using System.Collections;
using DG.Tweening;
using ReactiveUnity;
using UnityEngine;

public abstract class FallenTree : MonoBehaviour, PlayerInteractionManager.IInteractable, Axe.IUseableWithAxe
{
    [SerializeField] protected bool _fallsEast;

    [Header("Sprites")]
    [SerializeField] protected Sprite _idle;
    [SerializeField] protected Sprite _falling;

    [Header("Shake Properties")]
    [SerializeField] float _shakeDuration = 0.5f;
    [SerializeField] float _shakeStrength = 0.05f;
    [SerializeField] int _shakeVibrato = 10;
    [SerializeField] float _shakeRandomness = 90f;

    public enum FallenTreeStates { Idle, Falling };
    protected Reactive<FallenTreeStates> _state = new Reactive<FallenTreeStates>(FallenTreeStates.Falling);
    private const int _HITS_TO_DESTROY = 5;
    private int _hitCount = 0;

    Animator _animator;
    Action _unsubscribeCB;
    SpriteRenderer _spriteRenderer;
    Collider2D _collider;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

    }

    public void PlayFallingAnimation()
    {
        Shake();
        string _animationToPlay = _fallsEast ? "E_Falling" : "W_Falling";
        _animator.Play(_animationToPlay);
        _collider.enabled = false; // collider disabled while falling
        _spriteRenderer.sprite = _falling;
        _animator.enabled = true;
        StartCoroutine(WaitForFallingAnimationToEnd());
    }

    private void OnEnable()
    {
        _unsubscribeCB = _state.OnChange((curr, prev) => OnStateChange());
    }
    private void OnDisable()
    {
        _unsubscribeCB();
    }

    private void OnStateChange()
    {
        switch (_state.Value)
        {
            case FallenTreeStates.Idle:
                _spriteRenderer.sprite = _idle;
                break;
            default:
                Debug.LogWarning("FallenLarch state machine defaulted");
                break;
        }
    }

    private IEnumerator WaitForFallingAnimationToEnd()
    {
        // minus 0.01f so no flash of start of animation
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length - 0.01f);
        StopAnimation();
        _state.Value = FallenTreeStates.Idle;
    }

    protected void StopAnimation()
    {
        _animator.StopPlayback();
        _animator.enabled = false;
        _collider.enabled = true;

        // Sprite is animated locally within the child object.
        // This resets any values changed during the animation.
        // The parent is in the target position. 
        _spriteRenderer.transform.localPosition = Vector3.zero;
        _spriteRenderer.transform.localRotation = Quaternion.identity;

        // Requires enabling because it was disabled to make the falling animation look good.  
        GetComponentInChildren<StaticSpriteSorting>().enabled = true;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        return false; // does nothing
    }

    public void OnUseAxe()
    {
        if (_hitCount < _HITS_TO_DESTROY - 1)
        {
            _hitCount++;
            Shake();
            return;
        }
        Destroy(gameObject);
    }

    public void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }
}

