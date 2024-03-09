using UnityEngine;
using UnityEngine.SceneManagement;

// Enum must go from cold->hot
// Some logic relies on the enum numeric value
public enum Temperature {Freezing, Cold, Neutral, Warm, Hot};

public abstract class HeatSource : MonoBehaviour {
    [SerializeField] protected Temperature _temperature = (Temperature) 0;
    public virtual Temperature Temperature {
        get => _temperature;
        set => _temperature = value;
    }
}
