public class SpruceSaveData : WorldObjectSaveData {
    public TreeStates TreeState;
}
public class Spruce : TreePlant, ISaveable<SpruceSaveData> {
    private const string IDENTIFIER = "Spruce";

    public void Load(SpruceSaveData saveData)
    {
        _treeState.Value = saveData.TreeState;
    }

    public SpruceSaveData Save()
    {
        return new SpruceSaveData {
            Identifier = IDENTIFIER,
            Position = new SimpleVector3(transform.position),
            TreeState = _treeState.Value
        };
    }
}
