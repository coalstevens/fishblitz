public interface ISaveable
{
    string SaveableId { get; }
    string CaptureState();
    void RestoreState(string json);
    void ResetState();
}
