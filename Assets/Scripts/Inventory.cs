using TMPro;
using UnityEngine;
using ReactiveUnity;

public class Inventory : MonoBehaviour
{
    private Reactive<int> _mountedRods = new Reactive<int>(1);
    private Reactive<int> _rods = new Reactive<int>(1);
    private Reactive<int> _keys = new Reactive<int>(0);
    private Reactive<int> _money = new Reactive<int>(0);

    [Header("UI hookups")]
    [SerializeField] private TextMeshProUGUI _rodUiCount;
    [SerializeField] private TextMeshProUGUI _moneyUiCount;
    [SerializeField] private TextMeshProUGUI _keyUiCount;

    public int MountedRods
    {
        get
        {
            return _mountedRods.Value;
        }
        set
        {
            _mountedRods.Value = value;
        }
    }

    public int Rods
    {
        get
        {
            return _rods.Value;
        }
        set
        {
            _rods.Value = value;
        }
    }

    public int Keys
    {
        get
        {
            return _keys.Value;
        }

        set
        {
            _keys.Value = value;
        }
    }

    public int Money
    {
        get
        {
            return _money.Value;
        }

        set
        {
            _money.Value = value;
        }
    }

    private void Start()
    {
        UpdateUiCount(_rodUiCount, MountedRods);
        UpdateUiCount(_moneyUiCount, Money);
        UpdateUiCount(_keyUiCount, Keys);
        HookUpUi();
    }

    private void HookUpUi()
    {
        _mountedRods.OnChange((prev, curr) => UpdateUiCount(_rodUiCount, curr));
        _money.OnChange((prev, curr) => UpdateUiCount(_moneyUiCount, curr));
        _keys.OnChange((prev, curr) => UpdateUiCount(_keyUiCount, curr));
    }

    private void UpdateUiCount(TextMeshProUGUI textComponent, int count)
    {
        textComponent.text = count.ToString();
    }
    
}

