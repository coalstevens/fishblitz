using UnityEngine;

public class DevMenu : MonoBehaviour
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private ToggleMenuInput _toggleMenuInput;

    public void Respawn()
    {
        CloseMenu();
        var health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            health.RespawnAtDeathPosition();
    }

    public void ResetGame()
    {
        CloseMenu();
        GameReset.ResetPlayerState(_playerInventory);
        GameReset.ResetClock();

        var health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
            return;

        PlayerSceneData.PendingSpawnPosition = health.RespawnPosition;
        PlayerSceneData.HasPendingSpawn = true;
        LevelChanger.ChangeLevel(health.RespawnScene);
        GameReset.ClearAllSaveFiles();
    }

    private void CloseMenu()
    {
        GameClock.Instance?.ResumeGame();
        if (_toggleMenuInput != null)
            _toggleMenuInput.CloseMenu();
        else
            gameObject.SetActive(false);
    }
}
