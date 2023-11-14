using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Reactive<int> _rods = new Reactive<int>(10);
    private Reactive<int> _keys = new Reactive<int>(1);
    private Reactive<int> _money = new Reactive<int>(10);

    [Header("UI hookups")]
    [SerializeField] private TextMeshProUGUI _rodUiCount;
    [SerializeField] private TextMeshProUGUI _moneyUiCount;
    [SerializeField] private TextMeshProUGUI _keyUiCount;

    public int Rods
    {
        get
        {
            return _rods.Get();
        }
        set
        {
            _rods.Set(value);
        }
    }

    public int Keys
    {
        get
        {
            return _keys.Get();
        }

        set
        {
            _keys.Set(value);
        }
    }

    public int Money
    {
        get
        {
            return _money.Get();
        }

        set
        {
            _money.Set(value);
        }
    }

    private void Start()
    {
        UpdateUiCount(_rodUiCount, Rods);
        UpdateUiCount(_moneyUiCount, Money);
        UpdateUiCount(_keyUiCount, Keys);
        HookUpUi();
    }

    private void HookUpUi()
    {
        _rods.OnChange((prev, curr) => UpdateUiCount(_rodUiCount, curr));
        _money.OnChange((prev, curr) => UpdateUiCount(_moneyUiCount, curr));
        _keys.OnChange((prev, curr) => UpdateUiCount(_keyUiCount, curr));
    }

    private void UpdateUiCount(TextMeshProUGUI textComponent, int count)
    {
        textComponent.text = count.ToString();
    }
}