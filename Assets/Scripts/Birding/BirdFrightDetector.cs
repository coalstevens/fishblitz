using NUnit.Framework;
using UnityEngine;

public class BirdFrightDetector : MonoBehaviour
{
    private Collider2D _playerCollider;
    private BirdBrain _bird;

    private void Awake()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Assert.IsTrue(playerObjects.Length == 1, "More than one (or zero) player objects found in scene");
        _playerCollider = playerObjects[0].GetComponent<Collider2D>();
        _bird = transform.GetComponentInParent<BirdBrain>();
        Assert.IsNotNull(_bird, "Birdbrain not found in parent");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other != _playerCollider)
            return;
        Debug.Log("Player collider detected.");
        if
        (
            _bird.BirdState is not BirdBrain.ShelteredState &&
            _bird.BirdState is not BirdBrain.FleeingState &&
            _bird.BirdState is not BirdBrain.HighFlyingState &&
            _bird.BirdState is not BirdBrain.HighLandingState
        )
        {
            Debug.Log("Bird was frightened");
            _bird.FrightenBird();
        }
    }
}
