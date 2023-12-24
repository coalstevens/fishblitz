using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpeechController : MonoBehaviour
{
    private TextMeshPro _textBox;
    [SerializeField] private float _messageDurationSecs = 5f;
    private float _postedTime;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;
    public float alpha; 

    void Start()
    {
        _textBox = GetComponent<TextMeshPro>();
        _textBox.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        alpha = _textBox.alpha;
        if (_textBox.text == "") {
            return;
        }
        if (Time.time - _postedTime < _messageDurationSecs) {
            return;
        }

        //fade message
        if (_textBox.alpha > _fadeRateAlphaPerFrame) {
            _textBox.alpha -=  _fadeRateAlphaPerFrame;  
        }
        else {
            _textBox.alpha = 0f;
            _textBox.text = "";
        }
    }

    public void PostMessage(string message) {
        _textBox.alpha = 1f;
        _textBox.text = message;
        _postedTime = Time.time;
    }
}
