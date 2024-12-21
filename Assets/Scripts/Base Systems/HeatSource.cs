using UnityEngine;

// Enum must go from cold->hot
// Some logic relies on the enum numeric value
public enum Temperature {Freezing, Cold, Neutral, Warm, Hot};

public interface IHeatSource {
    public Temperature Temperature { get; set; }
}

