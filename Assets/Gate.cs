using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gate : MonoBehaviour
{
    [SerializeField] private InputActionReference _inputActionReference;
    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private Inventory playerInventory;
    private bool inRange = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        inRange = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (!inRange) {
            return;
        }
        if (_inputActionReference.action.WasPressedThisFrame()) {
            if (playerInventory.Keys >= 1) {
                playerInventory.Keys -= 1;
                openGate();
            }
        }
    }

    private void openGate()
    {
        gateCollider.enabled = false;
    }
}
