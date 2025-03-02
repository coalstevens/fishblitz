using System.Collections.Generic;
using UnityEngine;

public class ShedGerard : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    private List<string> _insideMessages = new()
    {
        "That was some storm hey?",
        "Not sure how I ended up here...",
        "Thanks again for the fish!",
    };

    private List<string> _outsideMessages = new() {
        "It's really nice out here.",
        "Maybe I should go for a walk.",
        "I wonder what's out there."
    };

    private List<string> _generalMessages = new() {
        "I'm just gonna rest around here for a bit."
    };

    [SerializeField] private Gerard _gerard;
    [SerializeField] private bool _isInside = true;
    private CharacterDialogueController _dialogueController;
    private int _insideMessageIndex = 0;
    private int _outsideMessageIndex = 0;
    private Vector2 _isOutsideGameHours = new Vector2(9, 19);

    private void Awake()
    {
        if (_gerard.State == Gerard.States.UnconsciousBeach || _gerard.State == Gerard.States.UnconsciousShed)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_gerard.State != Gerard.States.AwakeShed)
            Destroy(gameObject);

        if (_isInside)
        {
            if (GameClock.Instance.GameHour >= _isOutsideGameHours.x && GameClock.Instance.GameHour <= _isOutsideGameHours.y)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        else
        {
            if (GameClock.Instance.GameHour < _isOutsideGameHours.x && GameClock.Instance.GameHour > _isOutsideGameHours.y)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }

        _dialogueController = GetComponentInChildren<CharacterDialogueController>();
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_isInside)
            PostInsideMessage();
        else
            PostOutsideMessage();
        return true;
    }

    private void PostInsideMessage()
    {
        if (_insideMessageIndex < _insideMessages.Count)
        {
            _dialogueController.PostMessage(_insideMessages[_insideMessageIndex]);
            _insideMessageIndex++;
        }
        else
        {
            _dialogueController.PostMessage(_generalMessages[Random.Range(0, _generalMessages.Count)]);
        }
    }

    private void PostOutsideMessage()
    {
        if (_outsideMessageIndex < _outsideMessages.Count)
        {
            _dialogueController.PostMessage(_outsideMessages[_outsideMessageIndex]);
            _outsideMessageIndex++;
            if (_outsideMessageIndex >= 2)
                _gerard.ReadyForNextState = true;
        }
        else
        {
            _dialogueController.PostMessage(_generalMessages[Random.Range(0, _generalMessages.Count)]);
        }
    }
}
