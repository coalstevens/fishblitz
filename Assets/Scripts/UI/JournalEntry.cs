using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalEntry : MonoBehaviour, ISelectHandler
{
    [HideInInspector] public Journal Journal;

    public void OnSelect(BaseEventData eventData)
    {
        Journal?.OnEntrySelected(transform);
    }

    private void Awake()
    {
        if (GetComponent<Button>() == null)
        {
            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = GetComponentInChildren<Image>();
        }
    }
}
