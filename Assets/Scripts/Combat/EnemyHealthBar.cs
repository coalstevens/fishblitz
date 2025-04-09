using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private float _meterTimeoutSeconds = 5f;
    [SerializeField] private Color _healthBarColor;
    private EnemyHealth _enemyHealth;
    private SpriteRenderer _progressBar;
    private SpriteRenderer _frame;
    private bool _timedOut = true;
    private Coroutine _timeoutCoroutine;
    private float _timeoutElapsed = 0;
    private List<Action> _unsubscribeCBs = new();

    private void Awake()
    {
        _enemyHealth = transform.parent.GetComponent<EnemyHealth>();  
        _progressBar = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _frame = GetComponent<SpriteRenderer>();
        _progressBar.color = _healthBarColor;

        Assert.IsNotNull(_enemyHealth);
        Assert.IsNotNull(_progressBar);
        Assert.IsNotNull(_frame);
    }

    private void OnEnable()
    {
        _unsubscribeCBs.Add(_enemyHealth.CurrentHealth.OnChange(curr => UpdateHealthBar(curr)));
    }

    private void OnDisable()
    {
        foreach (var cb in _unsubscribeCBs) 
            cb();
        _unsubscribeCBs.Clear();
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (_timedOut)
        {
            _timedOut = false;
            _timeoutCoroutine = StartCoroutine(HandleTimeout());
            SetMeterVisibility(true);
        }

        _timeoutElapsed = 0;

        float newWidth = Mathf.Clamp01(currentHealth / _enemyHealth.MaxHealth);
        _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
    }

    private void SetMeterVisibility(bool visible)
    {
        _progressBar.enabled = visible;
        _frame.enabled = visible;
    }

    private IEnumerator HandleTimeout()
    {
        while (_timeoutElapsed < _meterTimeoutSeconds)
        {
            _timeoutElapsed += Time.deltaTime;
            yield return null;
        }

        _timedOut = true;
        SetMeterVisibility(false);
        _timeoutCoroutine = null;
    }
}
