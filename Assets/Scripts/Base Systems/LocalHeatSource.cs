using System.Collections.Generic;
using UnityEngine;

public class LocalHeatSource : MonoBehaviour, IHeatSource {
    [SerializeField] private Collider2D _heatCollider;
    private Temperature _temperature;
    public Temperature Temperature {
        get => _temperature;
        set {
            if (value == _temperature) 
                return;
            else {
                _temperature = value;
                AddSourceToOverlappedHeatSensitives();
            }
        }
    }

    private void Start() {
        if (_heatCollider == null) {
            Debug.LogError("This LocalHeatSource is missing a heat collider");
        }
    }

    private void OnDisable() {
        RemoveSourceFromOverlappedHeatSensitives();
    }

    private void OnEnable() {
        AddSourceToOverlappedHeatSensitives();
    }

    private void AddSourceToOverlappedHeatSensitives() {
        List<Collider2D> _colliders = new List<Collider2D>();
        _heatCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _colliders);
        foreach(var _collider in _colliders) {
            if (_collider.gameObject.TryGetComponent<HeatSensitive>(out var _heatSensitive))
                _heatSensitive.AddHeatSource(this);
        }
    }

    private void RemoveSourceFromOverlappedHeatSensitives() {
        List<Collider2D> _colliders = new List<Collider2D>();
        _heatCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _colliders);
        foreach(var _collider in _colliders)
            if (_collider.gameObject.TryGetComponent<HeatSensitive>(out var _heatSensitive))
                _heatSensitive.RemoveHeatSource(this);
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.TryGetComponent<HeatSensitive>(out var _heatSensitive))
            _heatSensitive.AddHeatSource(this);
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.TryGetComponent<HeatSensitive>(out var _heatSensitive))
            _heatSensitive.RemoveHeatSource(this);
    }
}
