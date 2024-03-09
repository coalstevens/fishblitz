public class OutsideTemperature : Singleton<OutsideTemperature> {

    private GlobalHeatSource _outsideHeatSource;
    public Temperature Temperature {
        get => _outsideHeatSource.Temperature;
        set => _outsideHeatSource.Temperature = value;
    }
    private void Start() {
        _outsideHeatSource = GetComponent<GlobalHeatSource>();
        _outsideHeatSource.Temperature = Temperature.Cold;
    }
}
