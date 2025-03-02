using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.U2D.IK;

public interface IGiftAble
{
    public bool TryGiveGift(Inventory.ItemType gift);
}
public class UnconsciousGerard : MonoBehaviour, PlayerInteractionManager.IInteractable, IGiftAble
{
    private List<string> _initialMessages = new()
    {
        "Ugh...",
        "Can't move...",
        "So hungry...",
        "Fish...",
        "Food.. need food...",
        "So weak...",
        "Please... fish...",
        "Is this the end..?"
    };

    private List<string> _giftMessages = new() {
        "This isn't what I need...",
        "Wait... fish?! Yes!"
    };

    private List<string> _fedMessages = new() {
        "Thank you...",
        "I feel better...",
        "I should rest now..."
    };

    private List<string> _sleepMessages = new() {
        "He's asleep."
    };

    [SerializeField] private List<ScriptableObject> _acceptableFood = new();
    [SerializeField] private Gerard _gerard;
    [SerializeField] private bool _atShed = false;
    private CharacterDialogueController _dialogueController;
    private int _fedIndexMessage = 0;
    private bool _isAsleep = false;

    private void Awake()
    {
        // Unconscious Gerard starts at the beach on the first day of summer and moves to the shed
        if (_atShed)
        {
            if (_gerard.State == Gerard.States.UnconsciousBeach)
            {
                gameObject.SetActive(false);
                return;
            }
            if (_gerard.State != Gerard.States.UnconsciousShed)
                Destroy(gameObject);
            gameObject.SetActive(true);
        }
        else
        {
            if (_gerard.State != Gerard.States.UnconsciousBeach)
                Destroy(gameObject);
            if (GameClock.Instance.GameYear == 1 && GameClock.Instance.GameSeason == GameClock.Seasons.Spring)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        _dialogueController = GetComponentInChildren<CharacterDialogueController>();
    }

    public bool TryGiveGift(Inventory.ItemType gift)
    {
        if (_isAsleep || _gerard.ReadyForNextState)
        {
            PostAfterFedMessage();
            return false;
        }

        if (_acceptableFood.Contains(gift))
        {
            _dialogueController.PostMessage(_giftMessages[1]);
            _gerard.ReadyForNextState = true;
            return true;
        }

        _dialogueController.PostMessage(_giftMessages[0]);
        return false;
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_isAsleep || _gerard.ReadyForNextState)
        {
            PostAfterFedMessage();
            return true;
        }
        _dialogueController.PostMessage(_initialMessages[Random.Range(0, _initialMessages.Count)]);
        return true;
    }

    private void PostAfterFedMessage()
    {
        if (_isAsleep)
        {
            NarratorSpeechController.Instance.PostMessage(_sleepMessages[0]);
            return;
        }

        if (_gerard.ReadyForNextState)
        {
            _dialogueController.PostMessage(_fedMessages[Random.Range(0, _fedMessages.Count)]);
            _fedIndexMessage++;
            if (_fedIndexMessage >= _fedMessages.Count)
                _isAsleep = true;
        }
    }
}
