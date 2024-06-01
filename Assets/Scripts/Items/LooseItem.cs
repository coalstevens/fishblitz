using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooseItem : MonoBehaviour
{
    [SerializeField] public ItemData Item;
    [SerializeField] public bool IsMagnetic = false;

    Animator _animator;

    private void Start() {
        _animator = GetComponent<Animator>();
    }
    void Update() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            IsMagnetic = true;
        }
    }
}
