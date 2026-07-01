using System.Collections.Generic;
using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _pages;
    private int _currentPageIndex;

    private void Start()
    {
        foreach (var page in _pages)
            page.SetActive(false);

        SwitchToPage(1);
    }

    public void SwitchToPage(int pageIndex)
    {
        if (pageIndex == _currentPageIndex || pageIndex < 0 || pageIndex >= _pages.Count)
            return;

        _pages[_currentPageIndex].SetActive(false);
        _currentPageIndex = pageIndex;
        _pages[_currentPageIndex].SetActive(true);
    }
}
