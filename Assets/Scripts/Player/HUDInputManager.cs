using System.Collections;
using UnityEngine;
public class HUDInputManager : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;

    private void OnItemSelect1()
    {
        SetActiveSlot(0);
    }
    private void OnItemSelect2()
    {
        SetActiveSlot(1);
    }
    private void OnItemSelect3()
    {
        SetActiveSlot(2);
    }
    private void OnItemSelect4()
    {
        SetActiveSlot(3);
    }
    private void OnItemSelect5()
    {
        SetActiveSlot(4);
    }
    private void OnItemSelect6()
    {
        SetActiveSlot(5);
    }
    private void OnItemSelect7()
    {
        SetActiveSlot(6);
    }
    private void OnItemSelect8()
    {
        SetActiveSlot(7);
    }
    private void OnItemSelect9()
    {
        SetActiveSlot(8);
    }
    private void OnItemSelect10()
    {
        SetActiveSlot(9);
    }

    private void OnMoveItemCursorRight()
    {
        int _current = _inventory.ActiveItemSlot.Value;
        _current++;
        _inventory.ActiveItemSlot.Value = _current % 10;
    }

    private void OnMoveItemCursorLeft()
    {
        int _current = _inventory.ActiveItemSlot.Value;
        _current--;
        if (_current < 0)
            _current = 9;
        _inventory.ActiveItemSlot.Value = _current;
    }

    public void SetActiveSlot(int slotIndex)
    {
        _inventory.ActiveItemSlot.Value = slotIndex;
    }
}
