public interface ISceneSaveable : ISaveable
{
    string PrefabId { get; }
    string PersistentID { get; set; }

    string ISaveable.SaveableId => PrefabId;
}
