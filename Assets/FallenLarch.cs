using System;
using System.Collections;
using ReactiveUnity;
using UnityEngine;

public class FallenLarch : MonoBehaviour
{
    [SerializeField] private Sprite _idle;
    [SerializeField] private Sprite _falling;
    Animator _animator;
    private enum States { Idle, Falling };
    Reactive<States> _state = new Reactive<States>(States.Falling);
    Action _unsubscribe;
    SpriteRenderer _spriteRenderer;
    Collider2D _collider;
    void Start()
    {
        // References
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        // FallenLarch spawns in falling animation
        _collider.enabled = false; // collider disabled while falling
        _spriteRenderer.sprite = _falling;
        _animator.enabled = true;
        StartCoroutine(WaitForFallingAnimationToEnd());
    }

    void OnEnable()
    {
        _unsubscribe = _state.OnChange((curr, prev) => OnStateChange());
    }
    void OnDisable()
    {
        _unsubscribe();
    }

    void OnStateChange()
    {
        switch (_state.Value)
        {
            case States.Idle:
                _spriteRenderer.sprite = _idle;
                break;
            default:
                Debug.LogWarning("FallenLarch state machine defaulted");
                break;
        }
    }

    private IEnumerator WaitForFallingAnimationToEnd()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
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

        _state.Value = States.Idle;
    }
}
