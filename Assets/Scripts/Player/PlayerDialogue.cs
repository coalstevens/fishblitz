using UnityEngine;

public class PlayerDialogue : MonoBehaviour
{
    public static PlayerDialogue Instance;
    private DialogueController _characterDialogueController;
    
    private void Awake()
    {
        Instance = this;
        _characterDialogueController = GetComponent<DialogueController>();
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

