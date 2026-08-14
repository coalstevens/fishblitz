using UnityEngine;

[RequireComponent(typeof(PixelTextRenderer))]
public class DialogueController : MonoBehaviour
{
    private PixelTextRenderer text;
    private Transform _followTransform;
    private float _postedTime;
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;

    private void Start()
    {
        text = GetComponent<PixelTextRenderer>();

        if (text == null)
        {
            Debug.LogError("PixelTextRenderer component is missing from DiaglogueController.");
            return;
        }

        if (transform.parent != null)
            _followTransform = transform.parent;
        else
            Debug.LogWarning("CharacterDialogueController has no parent transform.");
    }

    private void Update()
    {
        if (text == null || string.IsNullOrEmpty(text.Text))
            return;

        // Hold message for the duration
        if (Time.time - _postedTime < _messageDurationSecs)
            return;

        // Fade message
        Color textColor = text.Color;
        if (textColor.a > _fadeRateAlphaPerFrame)
        {
            textColor.a -= _fadeRateAlphaPerFrame;
            text.Color = textColor;
        }
        else
        {
            text.Color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            text.Text = "";
        }
    }

    public void PostMessage(string message)
    {
        if (text == null)
        {
            Debug.LogError("Cannot post message: PixelTextRenderer is not assigned.");
            return;
        }

        text.Text = message;
        text.Color = new Color(text.Color.r, text.Color.g, text.Color.b, 1f);
        _postedTime = Time.time;
    }
}
