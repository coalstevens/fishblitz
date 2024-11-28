using System.Collections.Generic;
using UnityEngine;

public class NearbyBirdTracker : MonoBehaviour
{
    private HashSet<Transform> _nearbyBirds = new HashSet<Transform>();
    public IReadOnlyCollection<Transform> NearbyBirds => _nearbyBirds;
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

        var overlappingColliders = new List<Collider2D>();
        _viewRange.OverlapCollider(new ContactFilter2D().NoFilter(), overlappingColliders);

        foreach (var collider in overlappingColliders)
            if (collider.TryGetComponent<Transform>(out var _bird) && !_nearbyBirds.Contains(_bird))
                _nearbyBirds.Add(_bird);
        
        if (_nearbyBirds.Contains(transform))
            _nearbyBirds.Remove(transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Transform>(out var _bird))
            if (!_nearbyBirds.Contains(_bird))
                _nearbyBirds.Add(_bird);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Transform>(out var _bird))
            if (_nearbyBirds.Contains(_bird))
                _nearbyBirds.Remove(_bird);
    }
}
