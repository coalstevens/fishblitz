using UnityEngine;

public class DialogueController : MonoBehaviour
{
    private PixelTextRenderer _textBox;
    private Transform _followTransform;
    private float _postedTime;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;

    private void Start()
    {
        _textBox = GetComponentInChildren<PixelTextRenderer>();

        if (_textBox == null)
        {
            Debug.LogError("PixelTextRenderer component is missing from CharacterDialogueController.");
            return;
        }

        if (transform.parent != null)
            _followTransform = transform.parent;
        else
            Debug.LogWarning("CharacterDialogueController has no parent transform.");
    }

    private void Update()
    {
        if (_textBox == null || string.IsNullOrEmpty(_textBox.Text))
            return;

        // Hold message for the duration
        if (Time.time - _postedTime < _messageDurationSecs)
            return;

        // Fade message
        Color textColor = _textBox.Color;
        if (textColor.a > _fadeRateAlphaPerFrame)
        {
            textColor.a -= _fadeRateAlphaPerFrame;
            _textBox.Color = textColor;
        }
        else
        {
            _textBox.Color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            _textBox.Text = "";
        }
    }

    public void PostMessage(string message)
    {
        if (_textBox == null)
        {
            Debug.LogError("Cannot post message: PixelTextRenderer is not assigned.");
            return;
        }

        _textBox.Text = message;
        _textBox.Color = new Color(_textBox.Color.r, _textBox.Color.g, _textBox.Color.b, 1f);
        _postedTime = Time.time;
    }
}
