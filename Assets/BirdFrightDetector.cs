using UnityEngine;

public class BirdFrightDetector : MonoBehaviour
{
    private Collider2D _playerCollider;
    private BirdBrain _bird;

    private void Awake() {
        _playerCollider = PlayerCondition.Instance.GetComponent<Collider2D>();
        _bird = transform.parent.GetComponent<BirdBrain>();
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other != _playerCollider)
            return;
        Debug.Log("Player collider detected.");
        if (_bird.BirdState is not ShelteredState && _bird.BirdState is not FleeingState) {
            Debug.Log("Bird was frightened");
            _bird.FrightenBird();
        }
    }
}
