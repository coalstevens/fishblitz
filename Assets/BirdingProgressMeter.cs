using System;
using System.Collections.Generic;
using UnityEngine;

public class BirdingProgressMeter : MonoBehaviour
{
    [SerializeField] private float MeterTimeoutSeconds = 5f;
    [SerializeField] private Color inProgressColor;
    [SerializeField] private Color capturedColor;
    private SpriteRenderer _progressBar;
    private SpriteRenderer _frame;
    private Bird _bird;
    private bool _timedOut = true;
    private float _timeoutElapsed = 0;
    private List<Action> _unsubscribeHooks = new();

    private void Awake()
    {
        _bird = transform.parent.GetComponent<Bird>();
        _progressBar = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _frame = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _unsubscribeHooks.Add(_bird.BeamHoveredElapsedSeconds.OnChange(_ => UpdateProgressMeter()));
        _unsubscribeHooks.Add(_bird.Caught.OnChange(curr => OnCapture(curr)));
        _progressBar.color = inProgressColor;
    }

    private void OnCapture(bool curr)
    {
        if (curr)
            _progressBar.color = capturedColor;
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void UpdateProgressMeter()
    {
        if (_timedOut)
        {
            _timedOut = false;
            SetMeterVisibility(true);
        }
        _timeoutElapsed = 0;

        float newWidth = Mathf.Lerp(0, 1, (float)_bird.BeamHoveredElapsedSeconds.Value / _bird.TimetoCatchSeconds);
        _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
    }

    private void SetMeterVisibility(bool visible)
    {
        _progressBar.enabled = visible;
        _frame.enabled = visible;
    }

    void Update()
    {
        AdjustAgainstParentScale();
        
        if (_timedOut) return;
        _timeoutElapsed += Time.deltaTime;

        if (_timeoutElapsed >= MeterTimeoutSeconds)
        {
            _timedOut = true;
            SetMeterVisibility(false);
        }
    }
    
    private void AdjustAgainstParentScale() 
    {
        transform.localScale = new Vector3
        (
            1f / transform.parent.lossyScale.x,
            1f / transform.parent.lossyScale.y,
            1f / transform.parent.lossyScale.z
        );
    }
}
