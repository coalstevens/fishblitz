using System.Collections;
using UnityEngine;

public class BlinkRepeatedly : MonoBehaviour
{
    [SerializeField] private float _invisibleInterval = 0.2f;
    [SerializeField] private float _visibleInterval = 0.6f;
    [SerializeField] private PixelTextRenderer _text;

    private string _originalText;

    private IEnumerator Start()
    {
        _originalText = _text.Text;

        while (true)
        {
            yield return new WaitForSeconds(_visibleInterval);
            _text.Text = "";
            yield return new WaitForSeconds(_invisibleInterval);
            _text.Text = _originalText;
        }
    }
}
