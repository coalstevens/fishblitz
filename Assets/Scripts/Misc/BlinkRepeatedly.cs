using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkRepeatedly : MonoBehaviour
{
    [SerializeField] private float _invisibleInterval = 0.2f;
    [SerializeField] private float _visibleInterval = 0.6f;
    [SerializeField] private TextMeshProUGUI _text;

    private string _originalText;

    private IEnumerator Start()
    {
        _originalText = _text.text;

        while (true)
        {
            yield return new WaitForSeconds(_visibleInterval);
            _text.text = "";
            yield return new WaitForSeconds(_invisibleInterval);
            _text.text = _originalText;
        }
    }
}
