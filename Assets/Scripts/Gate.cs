using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gate : MonoBehaviour
{
    [SerializeField] private InputActionReference _inputActionReference;
    [SerializeField] private Collider2D gateCollider;
    private Inventory _inventory;
    private Animator _animator;
    private bool inRange = false;
    // Start is called before the first frame update

    private void Awake()
    {
        _inventory = GameObject.FindWithTag("InventoryContainer").GetComponent<Inventory>();
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        inRange = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (!inRange)
        {
            return;
        }
        if (_inputActionReference.action.WasPressedThisFrame())
        {
            if (_inventory.RemoveItem("Key", 1)) {
                openGate();
            }
        }
    }

    private void openGate()
    {
        Invoke(nameof(RemoveCollider), 1);
        _animator.Play("GateOpening");
    }

    private void RemoveCollider()
    {
        gateCollider.enabled = false;
    }
}
