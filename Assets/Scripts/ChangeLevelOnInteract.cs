using UnityEngine;
using OysterUtils;

public class ChangeLevelOnInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;

    public Collider2D ObjCollider {
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null) {
                return _collider;
            }
            else {
                Debug.LogError("ChangeLevelOnInteract does not have a collider component");
                return null;
            }
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        PlayerData.Instance.SceneSpawnPosition = _sceneSpawnLocation;
        SmoothSceneManager.LoadScene(_toScene);
        return true;
    }
}
