using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NarratorSpeechController : MonoBehaviour
{
    private List<TextMeshProUGUI> _messages = new();
    private List<float> _messageStartTimes = new();
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;
    [SerializeField] private float _bottomPadding = 1.5f;
    [SerializeField] private float _sidePadding = 0.5f;
    [SerializeField] private float _lineSpacing = 5;
    private float _postTime;
    private int _messageNum = 0;
    void Start() {
        _postTime = Time.time + UnityEngine.Random.Range(0, 5); 
    }
    // Update is called once per frame
    void Update()
    {
        Tester();
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

    private void Tester(){
        if (Time.time > _postTime) {
            PostMessage("Hello I am the narrator " + _messageNum.ToString());
            _messageNum++;
            _postTime = Time.time + UnityEngine.Random.Range(1, 3);
        }
    }
}
