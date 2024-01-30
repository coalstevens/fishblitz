using UnityEngine;

public class HeatSourceManager : MonoBehaviour {
    [SerializeField] float _heatSourceRadius = 5;
    private Temperature _localTemperature = (Temperature) 0;

    /// <summary>
    /// Setting the temperature enables the heatsource
    /// </summary>
    public Temperature LocalTemperature {
        get => _localTemperature;
        set {
            if (value == _localTemperature) 
                return;
            else {
                _localTemperature = value;
                RaiseTemperatureChangeEvent(value);
            }
        }
    }

    public void DisableHeatSource() {
        HeatEventSystem.Instance.RaiseHeatSourceRemovedEvent(this.gameObject);
    }

    private void OnDestroy() {
        HeatEventSystem.Instance.RaiseHeatSourceRemovedEvent(this.gameObject);
    }

    private void RaiseTemperatureChangeEvent(Temperature newTemp) {
        HeatEventSystem.Instance.TriggerHeatSourceChangeEventWithinRadius(this.gameObject, newTemp, _heatSourceRadius);
    }
}
