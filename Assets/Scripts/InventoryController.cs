using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Reactive<int> _rods = new Reactive<int>(1);
    private Reactive<int> _money = new Reactive<int>(0);
    private Reactive<bool> _hasKey = new Reactive<bool>(false);

    [Header("UI hookups")]
    [SerializeField] private TextMeshProUGUI _rodUiCount;
    [SerializeField] private TextMeshProUGUI _moneyUiCount;

    private void Start()
    {
        UpdateRodUi(_rods.Get());
        UpdateMoneyUi(_money.Get());
        HookUpUi();
    }

    private void HookUpUi()
    {
        _rods.OnChange((prev, curr) => UpdateRodUi(curr));
        _money.OnChange((prev, curr) => UpdateMoneyUi(curr));
    }

    private void UpdateRodUi(int to)
    {
        _rodUiCount.text = $"rods: {to}";
    }

    private void UpdateMoneyUi(int to)
    {
        _moneyUiCount.text = $"money: {to}";
    }
}