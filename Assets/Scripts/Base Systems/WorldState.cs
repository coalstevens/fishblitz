using ReactiveUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldState", menuName = "Base Systems/World State")]
public class WorldState : ScriptableObject
{
    public enum WaterStates { Puddles, Flood, Full, Shallow, PostFlood };
    public enum RainStates { HeavyRain, NoRain };
    public enum WindStates { Windy, Breezy, NoWind};
    public static Reactive<WaterStates> WaterState = new Reactive<WaterStates>(WaterStates.Shallow);
    public static Reactive<RainStates> RainState = new Reactive<RainStates>(RainStates.NoRain);
    public static Reactive<WindStates> WindState = new Reactive<WindStates>(WindStates.NoWind);
    public static Temperature OutsideTemperature = Temperature.Freezing;
}
