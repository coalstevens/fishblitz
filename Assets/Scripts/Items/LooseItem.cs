using UnityEngine;

public class LooseItem : MonoBehaviour
{
    [SerializeField] public Inventory.ItemStack Item;
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
