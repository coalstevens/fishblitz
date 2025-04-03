using System;
using UnityEngine;
using UnityEngine.Assertions;

public interface IHealth
{
    public event Action OnHealthUpdate;
    public float CurrentHealth { get ; }
    public float MaxHealth { get ; }
}

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private float _meterTimeoutSeconds = 5f;
    [SerializeField] private Color _healthBarColor;
    private IHealth _enemy;
    private SpriteRenderer _progressBar;
    private SpriteRenderer _frame;
    private bool _timedOut = true;
    private float _timeoutElapsed = 0;

    private void Awake()
    {
        _enemy = transform.parent.GetComponent<IHealth>();
        _progressBar = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _frame = GetComponent<SpriteRenderer>();
        _progressBar.color = _healthBarColor;

        Assert.IsNotNull(_enemy);
        Assert.IsNotNull(_progressBar);
        Assert.IsNotNull(_frame);
    }

    private void OnEnable()
    {
        _enemy.OnHealthUpdate += UpdateHealthBar;
    }

    private void OnDisable()
    {
        _enemy.OnHealthUpdate -= UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        if (_timedOut)
        {
            _timedOut = false;
            SetMeterVisibility(true);
        }
        _timeoutElapsed = 0;

        float newWidth = Mathf.Lerp(0, 1, _enemy.CurrentHealth / _enemy.MaxHealth);
        _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
    }

    private void SetMeterVisibility(bool visible)
    {
        _progressBar.enabled = visible;
        _frame.enabled = visible;
    }

    private void Update()
    {
        if (_timedOut) return;
        _timeoutElapsed += Time.deltaTime;

        if (_timeoutElapsed >= _meterTimeoutSeconds)
        {
            _timedOut = true;
            SetMeterVisibility(false);
        }
    }
}
