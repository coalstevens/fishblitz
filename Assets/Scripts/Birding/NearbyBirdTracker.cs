using System.Collections.Generic;
using UnityEngine;

public class NearbyBirdTracker : MonoBehaviour
{
    [SerializeField] private Bird _thisBird;
    [SerializeField] private int _nearbyBirdsCount = 0;
    private HashSet<Bird> _nearbyBirds = new();
    public IReadOnlyCollection<Bird> NearbyBirds => _nearbyBirds;
    private Collider2D _viewRange;

    private void Start()
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

        InitializeNearbyBirds();
    }

    private void InitializeNearbyBirds()
    {
        var _overlappingColliders = new List<Collider2D>();
        _viewRange.Overlap(new ContactFilter2D().NoFilter(), _overlappingColliders);

        foreach (var _collider in _overlappingColliders)
            if (_collider.TryGetComponent<Bird>(out var _bird) && _bird != _thisBird)
                _nearbyBirds.Add(_bird);
        _nearbyBirdsCount = _nearbyBirds.Count;
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
        if (other.TryGetComponent<Bird>(out var _bird) && _bird != _thisBird)
        {
            _nearbyBirds.Add(_bird);
            _nearbyBirdsCount = _nearbyBirds.Count;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Bird>(out var _bird))
        {
            _nearbyBirds.Remove(_bird);
            _nearbyBirdsCount = _nearbyBirds.Count;
        }
    }

    void OnBirdDestroyed(Bird bird)
    {
        if (bird == null) return;
        _nearbyBirds.Remove(bird);
        _nearbyBirds.RemoveWhere(b => b == null); // Cleanup
        _nearbyBirdsCount = _nearbyBirds.Count;
    }
}
