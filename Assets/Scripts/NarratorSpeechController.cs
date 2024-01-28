using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NarratorSpeechController : MonoBehaviour
{
    private static NarratorSpeechController _instance;
    public static NarratorSpeechController Instance
    {
        get
        {
            // If the instance doesn't exist, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<NarratorSpeechController>();

                // If it still doesn't exist, create a new GameObject with the singleton script attached
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("NarratorSpeechControllerError");
                    _instance = singletonObject.AddComponent<NarratorSpeechController>();
                }
            }

            return _instance;
        }
    }
    private List<TextMeshProUGUI> _messages = new();
    private List<float> _messageStartTimes = new();
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;
    [SerializeField] private float _bottomPadding = 1.5f;
    [SerializeField] private float _sidePadding = 0.5f;
    [SerializeField] private float _lineSpacing = 0.2f;
    private float _postTime;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensures that only one instance of the singleton exists
        }
    }
    void Start() {
        _postTime = Time.time + UnityEngine.Random.Range(0, 5); 
    }
    
    void Update()
    {
        CheckMessageLifeSpans();
    }

    public void PostMessage(string message) {
        // move existing messages up, if any
        for (int i = 0; i < _messages.Count; i++) {
            var _pos = _messages[i].rectTransform.position;
            _pos.y += _lineSpacing;
            _messages[i].rectTransform.position = _pos;
        }
        // create a new message object
        var _newMessagePosition = this.transform.position;
        _newMessagePosition.y += _bottomPadding;
        _newMessagePosition.x += _sidePadding;
        var _newMessageObj = Instantiate(_messagePrefab, _newMessagePosition, Quaternion.identity, this.transform);
        var _newMessage = _newMessageObj.GetComponent<TextMeshProUGUI>();
        _newMessage.text = message;
        _messages.Add(_newMessage);
        _messageStartTimes.Add(Time.time);
    }

    private void CheckMessageLifeSpans() {
        for (int i = 0; i < _messages.Count; i++) {
            if (Time.time - _messageStartTimes[i] < _messageDurationSecs) {
                continue;
            }
            //fade message
            if (_messages[i].alpha > _fadeRateAlphaPerFrame) {
                _messages[i].alpha -=  _fadeRateAlphaPerFrame;  
                continue;
            }
            //destroy message once faded
            else {
                var temp = _messages[i];
                _messages.RemoveAt(i);
                _messageStartTimes.RemoveAt(i);
                Destroy(temp.transform.gameObject);
            }
        }
    }
}
