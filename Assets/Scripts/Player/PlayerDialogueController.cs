using UnityEngine;

public class PlayerDialogueController : MonoBehaviour
{
    public static PlayerDialogueController Instance;
    private CharacterDialogueController _characterDialogueController;
    
    private void Awake()
    {
        Instance = this;
        _characterDialogueController = GetComponent<CharacterDialogueController>();
    }

    public void PostMessage(string message) 
    {
        if (_characterDialogueController == null)
        {
            Debug.LogError("CharacterDialogueController is not assigned!");
            return;
        }
        _characterDialogueController.PostMessage(message);
    }
}

