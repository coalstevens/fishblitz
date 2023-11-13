using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Store : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;
    [SerializeField] private SpriteRenderer _storeSpriteRenderer;
    [SerializeField] private Sprite _outlineSprite;
    [SerializeField] private Sprite _nonOutlineSprite;

    private Inventory _inventory;
    private Reactive<bool> _inRange = new Reactive<bool>(false);

    private void Awake()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _inRange.OnChange((_, inRange) => SetOutline(inRange));
        SetOutline(false);
    }

    private void SetOutline(bool inRange)
    {
        _storeSpriteRenderer.sprite = inRange ? _outlineSprite : _nonOutlineSprite;
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
