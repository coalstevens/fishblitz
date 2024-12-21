using OysterUtils;
using UnityEngine;

public class LevelChanger : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    [SerializeField] bool OnInteract = false; 
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _spawnLocation;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!OnInteract && other == GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>()) {
            ChangeLevel (_toScene, _spawnLocation);
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (OnInteract) {
            ChangeLevel (_toScene, _spawnLocation);
            return true;
        }
        return false;
    }

    public static void ChangeLevel(string sceneName, Vector3 spawnLocation) {
        PlayerData.SceneSpawnPosition = spawnLocation;
        SmoothSceneManager.LoadScene(sceneName);
    }
}
