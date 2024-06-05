using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    Transform _player;

    
    private void Awake() {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        PlayerCondition.Instance.Sleep();
        LevelChanger.ChangeLevel("Abandoned Shed", _player.transform.position);
        return true;
    }
}
