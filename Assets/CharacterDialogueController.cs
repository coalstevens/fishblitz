using TMPro;
using UnityEngine;

public class CharacterDialogueController : MonoBehaviour
{
    private TextMeshProUGUI _textBox;
    private Transform _followTransform;
    private float _postedTime;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;

    private void Start()
    {
        _textBox = GetComponentInChildren<TextMeshProUGUI>();

        if (_textBox == null)
        {
            Debug.LogError("TextMeshProUGUI component is missing from CharacterDialogueController.");
            return;
        }

        if (transform.parent != null)
            _followTransform = transform.parent;
        else
            Debug.LogWarning("CharacterDialogueController has no parent transform.");
    }

    private void Update()
    {
        if (_textBox == null || string.IsNullOrEmpty(_textBox.text))
            return;

        // Hold message for the duration
        if (Time.time - _postedTime < _messageDurationSecs)
            return;

        // Fade message
        Color textColor = _textBox.color;
        if (textColor.a > _fadeRateAlphaPerFrame)
        {
            textColor.a -= _fadeRateAlphaPerFrame;
            _textBox.color = textColor;
        }
        else
        {
            _textBox.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            _textBox.text = "";
        }
    }

    public void PostMessage(string message)
    {
        if (_textBox == null)
        {
            Debug.LogError("Cannot post message: TextMeshProUGUI is not assigned.");
            return;
        }

        _textBox.text = message;
        _textBox.color = new Color(_textBox.color.r, _textBox.color.g, _textBox.color.b, 1f);
        _postedTime = Time.time;
    }
}
