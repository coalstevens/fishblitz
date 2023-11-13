using UnityEngine;
using UnityEngine.InputSystem;

public class Store : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;

    private Inventory _inventory;
    private Reactive<bool> _inRange = new Reactive<bool>(false);

    private void Awake()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _inRange.Set(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _inRange.Set(false);
    }

    private void Update()
    {
        if (!_inRange.Get())
        {
            return;
        }

        if (!_action.action.WasPressedThisFrame())
        {
            return;
        }

        MakeTrade();
    }

    private void MakeTrade()
    {
        if (_inventory.Money <= 0)
        {
            return;
        }

        _inventory.Money -= 1;
        _inventory.Rods += 1;
    }
}
