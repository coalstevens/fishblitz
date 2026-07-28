public interface ISaveableComponent
{
    string ComponentId { get; }
    string CaptureStateAsJson();
    void RestoreStateFromJson(string json);
}
