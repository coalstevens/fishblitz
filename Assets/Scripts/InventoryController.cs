using TMPro;
using UnityEngine;

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
            return _mountedRods.Get();
        }
        set
        {
            _mountedRods.Set(value);
        }
    }

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
        UpdateUiCount(_rodUiCount, MountedRods);
        UpdateUiCount(_moneyUiCount, Money);
        UpdateUiCount(_keyUiCount, Keys);
        //TODO add player rod to UI
        HookUpUi();
    }

    private void HookUpUi()
    {
        _mountedRods.OnChange((prev, curr) => UpdateUiCount(_rodUiCount, curr));
        _money.OnChange((prev, curr) => UpdateUiCount(_moneyUiCount, curr));
        _keys.OnChange((prev, curr) => UpdateUiCount(_keyUiCount, curr));
        //TODO add player rod to UI
    }

    private void UpdateUiCount(TextMeshProUGUI textComponent, int count)
    {
        textComponent.text = count.ToString();
    }
}