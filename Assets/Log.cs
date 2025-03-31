using UnityEngine;
using UnityEngine.Assertions;

public class GenericWeightyObject : MonoBehaviour, IWeighty
{
    [SerializeField] WeightyObjectType _weightyObjectType;
    [SerializeField] private string _identifier = "Log";
    private PlayerCarry _playerCarry;
    public WeightyObjectType WeightyObject => _weightyObjectType;

    private void OnEnable()
    {
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);
        _playerCarry = _player.GetComponent<PlayerCarry>();
        Assert.IsNotNull(_playerCarry);
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerCarry.TryPickUpWeightyObject(this))
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    void Awake()
    {
        Assert.IsNotNull(_weightyObjectType);
    }

    public SaveData Save()
    {
        var _saveData = new SaveData();
        _saveData.AddIdentifier(_identifier);
        _saveData.AddTransformPosition(transform.position);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        // nothing to load
    }
}
