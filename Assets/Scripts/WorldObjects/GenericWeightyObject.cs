using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class GenericWeightyObject : MonoBehaviour, IWeighty
{
    [SerializeField] WeightyObjectType _weightyObjectType;
    [SerializeField] private string _identifier = "Log";
    private string _persistentID;
    private PlayerCarry _playerCarry;
    public WeightyObjectType WeightyObject => _weightyObjectType;

    private void Start()
    {
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);
        _playerCarry = _player.GetComponent<PlayerCarry>();
        Assert.IsNotNull(_playerCarry);
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        if (_playerCarry.TryPickUpWeightyObject(this))
        {
            StartCoroutine(DelayedDestroy(0.06f * 4)); // duration is half the pick up animation
            return true;
        }
        return false;
    }

    void Awake()
    {
        Assert.IsNotNull(_weightyObjectType);
    }

    public string PrefabId => _identifier;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        return null;
    }

    public void RestoreState(string json)
    {
        // no extended state to restore
    }

    public void ResetState() { }
}
