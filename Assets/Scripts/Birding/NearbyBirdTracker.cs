using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class NearbyBirdTracker : MonoBehaviour
{
    [SerializeField] private int _nearbyBirdsCount = 0;
    private BirdBrain _thisBird;
    private HashSet<BirdBrain> _nearbyBirds = new();
    public IReadOnlyCollection<BirdBrain> NearbyBirds => _nearbyBirds;
    private Collider2D _viewRange;

    private void Start()
    {
        _thisBird = GetComponentInParent<BirdBrain>();
        _viewRange = GetComponent<Collider2D>();
        Assert.IsNotNull(_viewRange, "NearbyBirdTracker requires a Collider2D component!");

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
            if (_collider.TryGetComponent<BirdBrain>(out var _bird) && _bird != _thisBird)
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
        if (other.TryGetComponent<BirdBrain>(out var _bird) && _bird != _thisBird)
        {
            _nearbyBirds.Add(_bird);
            _nearbyBirdsCount = _nearbyBirds.Count;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<BirdBrain>(out var _bird))
        {
            _nearbyBirds.Remove(_bird);
            _nearbyBirdsCount = _nearbyBirds.Count;
        }
    }

    void OnBirdDestroyed(BirdBrain bird)
    {
        if (bird == null) return;
        _nearbyBirds.Remove(bird);
        _nearbyBirds.RemoveWhere(b => b == null); // Cleanup
        _nearbyBirdsCount = _nearbyBirds.Count;
    }
}
