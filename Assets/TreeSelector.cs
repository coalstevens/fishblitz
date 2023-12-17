using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class TreeSelector : MonoBehaviour
{
    [SerializeField] Sprite _summerAdult;
    [SerializeField] Sprite _fallAdult;
    [SerializeField] Sprite _deadAdult;
    [SerializeField] Sprite _stump;
    private SpriteRenderer _spriteRenderer;

    private enum TreeType {SummerAdult, FallAdult, DeadAdult, Stump};
    [SerializeField] private TreeType _defaultSprite; 

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetSprite(_defaultSprite);
    }

    void SetSprite(TreeType treeType) {
        switch (treeType) {
            case TreeType.SummerAdult:
                _spriteRenderer.sprite = _summerAdult;
                break;
            case TreeType.FallAdult:
                _spriteRenderer.sprite = _fallAdult;
                break;
            case TreeType.DeadAdult:
                _spriteRenderer.sprite = _deadAdult;
                break;
            case TreeType.Stump:
                _spriteRenderer.sprite = _stump;
                break;
        }
    }
}
