public interface IHeatSensitive {
    public Temperature LocalTemperature {get;set;}
    //void OnTemperatureChange(IHeatSource heatSource, Temperature newTemp);
}
public interface IHeatSource {
    public Temperature SourceTemperature{get;set;}
   // void TriggerTemperatureChange(Temperature newTemp);
}