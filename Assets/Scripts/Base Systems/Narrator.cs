using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Note: Narrator messages run on unscaledTime (unaffected by gamepause)

public class Narrator : MonoBehaviour
{
    private List<TextMeshProUGUI> _postedMessages = new();
    private Queue<string> _messageQueue = new();
    private List<float> _messageStartTimes = new();
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;
    [SerializeField] private float _bottomPadding = 1.5f;
    [SerializeField] private float _sidePadding = 0.5f;
    [SerializeField] private float _lineSpacing = 0.2f;
    [SerializeField] private float _postMessageDelaySeconds = 2f;
    private float _postMessageBuffer = 0f;
    private Transform _narratorMessageContainer;
    private static Narrator _instance;
    public static Narrator Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("The Narrator is not loaded into this scene");
                return null;
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        _narratorMessageContainer = GameObject.FindGameObjectWithTag("NarratorMessageContainer").transform;
    }

    private void OnEnable()
    {
        SceneSaveLoadManager.FirstVisitToScene += PrintFirstVisitToSceneMessage;
    }

    private void OnDisable()
    {
        SceneSaveLoadManager.FirstVisitToScene -= PrintFirstVisitToSceneMessage;
    }

    void Update()
    {
        CheckMessageLifeSpans();

        // Print next message if the post message delay is expired
        if (_postMessageBuffer > _postMessageDelaySeconds)
        {
            if (_messageQueue.TryDequeue(out var _message))
            {
                _postMessageBuffer = 0;
                PrintMessage(_message);
            }
            return;
        }
        _postMessageBuffer += Time.unscaledDeltaTime;
    }

    public void PostMessage(string message)
    {
        _messageQueue.Enqueue(message);
    }

    private void PrintMessage(string message)
    {
        // move existing messages up, if any
        for (int i = 0; i < _postedMessages.Count; i++)
        {
            var _pos = _postedMessages[i].rectTransform.position;
            _pos.y += _lineSpacing;
            _postedMessages[i].rectTransform.position = _pos;
        }
        // create and position new message object
        var _newMessagePosition = _narratorMessageContainer.position;
        _newMessagePosition.y += _bottomPadding;
        _newMessagePosition.x += _sidePadding;
        var _newMessageObj = Instantiate(_messagePrefab, _newMessagePosition, Quaternion.identity, _narratorMessageContainer);
        var _newMessage = _newMessageObj.GetComponent<TextMeshProUGUI>();
        _newMessage.text = message;

        // log details
        _postedMessages.Add(_newMessage);
        _messageStartTimes.Add(Time.unscaledTime);
    }

    private void CheckMessageLifeSpans()
    {
        for (int i = 0; i < _postedMessages.Count; i++)
        {
            if (Time.unscaledTime - _messageStartTimes[i] < _messageDurationSecs)
            {
                continue;
            }
            //fade message
            if (_postedMessages[i].alpha > _fadeRateAlphaPerFrame)
            {
                _postedMessages[i].alpha -= _fadeRateAlphaPerFrame;
                continue;
            }
            //destroy message once faded
            else
            {
                var temp = _postedMessages[i];
                _postedMessages.RemoveAt(i);
                _messageStartTimes.RemoveAt(i);
                Destroy(temp.transform.gameObject);
            }
        }
    }

    public bool AreMessagesClear()
    {
        return _postedMessages.Count == 0 && _messageQueue.Count == 0;
    }

    private void PrintFirstVisitToSceneMessage(string sceneName)
    {
        switch (sceneName)
        {
            case "Outside":
                StartCoroutine(PostMessageAfterDelay("i need to find shelter", 30f, false));
                break;
            case "Abandoned Shed":
                StartCoroutine(PostMessageAfterDelay("guess i'm sleeping in this wreck", 2f, false));
                break;
            default:
                break;
        }
    }

    private IEnumerator PostMessageAfterDelay(string message, float delay, bool narrator)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (narrator)
            PostMessage(message);
        else 
            PlayerDialogue.Instance.PostMessage(message);
    }
}
