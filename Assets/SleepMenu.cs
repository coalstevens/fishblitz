using UnityEngine;

public class SleepMenu : MonoBehaviour
{
    Animator _animator;
    public const float FADE_DURATION_SEC = 1f;
    private void Awake() {
        _animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable() {
        _animator.Play("FadeIn");
    }

    public void FadeOut() {
        _animator.Play("FadeOut");
    }
}
