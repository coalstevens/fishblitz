using System.Collections.Generic;
using UnityEngine;

public class NearbyBirdTracker : MonoBehaviour
{
    private List<Bird> _nearbyBirds = new List<Bird>();
    public IReadOnlyCollection<Bird> NearbyBirds => _nearbyBirds;
    private Collider2D _viewRange;

    private void Awake()
    {
        _viewRange = GetComponent<Collider2D>();
        if (_viewRange == null)
        {
            Debug.LogError("NearbyBirdTracker requires a Collider2D component!");
            return;
        }

        if (!_viewRange.isTrigger)
        {
            Debug.LogWarning("NearbyBirdTracker collider should be set as a trigger. Adjusting now.");
            _viewRange.isTrigger = true;
        }

        var _overlappingColliders = new List<Collider2D>();
        _viewRange.OverlapCollider(new ContactFilter2D().NoFilter(), _overlappingColliders);

        foreach (var _collider in _overlappingColliders)
            if (_collider.TryGetComponent<Bird>(out var _nearbyBird) && !_nearbyBirds.Contains(_nearbyBird))
                _nearbyBirds.Add(_nearbyBird);

        if (TryGetComponent<Bird>(out var _thisBird) && _nearbyBirds.Contains(_thisBird))
            _nearbyBirds.Remove(_thisBird);
    }

    private void OnEnable()
    {
        BirdBrain.BirdDestroyed += OnBirdDestroyed;
    }

    private void OnDisable()
    {
        BirdBrain.BirdDestroyed -= OnBirdDestroyed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Bird>(out var _bird))
            if (!_nearbyBirds.Contains(_bird))
                _nearbyBirds.Add(_bird);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Bird>(out var _bird))
            if (_nearbyBirds.Contains(_bird))
                _nearbyBirds.Remove(_bird);
    }

    void OnBirdDestroyed(Bird bird)
    {
        if (_nearbyBirds.Contains(bird))
            _nearbyBirds.Remove(bird);
        //_nearbyBirds.RemoveAll(transform => transform == null);
    }
}
