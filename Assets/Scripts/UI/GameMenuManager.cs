using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    public interface IGameMenuPage
    {
        /// <returns> Returns false on move into the Menu Tabs</returns>
        public bool MoveCursor(Vector2 inputDirection);
        public void Select();
        public void EnableCursor();
        public void DisableCursor();
        public void LoadPage();
        public void UnloadPage();
    }

    [Serializable]
    private class MenuPage
    {
        public Transform TabCursor;
        public Transform PageTransform;
        private IGameMenuPage _page;
        public IGameMenuPage Page
        {
            get
            {
                if (_page == null)
                    _page = PageTransform.GetComponent<IGameMenuPage>();
                return _page;
            }
        }
    }

    [Header("Menu Pages")]
    [SerializeField] private List<MenuPage> _pages = new();
    private Reactive<bool> _areMenuTabsActive = new Reactive<bool>(true);
    private Reactive<int> _currentPageIndex = new Reactive<int>(1);
    private Reactive<int> _cursorTabIndex = new Reactive<int>(1);
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _unsubscribeHooks.Add(_cursorTabIndex.OnChange((prev, curr) => UpdateTabCursor(prev, curr)));
        _unsubscribeHooks.Add(_currentPageIndex.OnChange((prev, curr) => HandlePageChange(prev, curr)));
        _unsubscribeHooks.Add(_areMenuTabsActive.OnChange(curr => SetTabCursorActive(curr)));

        _pages[_currentPageIndex.Value].Page.LoadPage();
        UpdateTabCursor(0, _currentPageIndex.Value);
    }

    private void OnDisable()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook();
        _unsubscribeHooks.Clear();
    }

    // Player input method
    public void OnSelect()
    {
        if (_areMenuTabsActive.Value && _cursorTabIndex.Value != _currentPageIndex.Value)
            _currentPageIndex.Value = _cursorTabIndex.Value;
        else
            _pages[_currentPageIndex.Value].Page.Select();
    }

    // Player input method
    public void OnMoveCursor(InputValue value)
    {
        if (_areMenuTabsActive.Value)
            MoveTabCursor(value.Get<Vector2>());
        else
            if (!_pages[_currentPageIndex.Value].Page.MoveCursor(value.Get<Vector2>()))
            _areMenuTabsActive.Value = true;
    }

    private void SetTabCursorActive(bool curr)
    {

        _pages[_cursorTabIndex.Value].TabCursor.gameObject.SetActive(curr);
        if (curr)
        {
            _pages[_currentPageIndex.Value].Page.DisableCursor();
            _cursorTabIndex.Value = _currentPageIndex.Value;
        }
        else
        {
            _pages[_currentPageIndex.Value].Page.EnableCursor();
        }
    }

    private void HandlePageChange(int previousPage, int currentPage)
    {
        _pages[previousPage].Page.UnloadPage();
        _pages[currentPage].Page.LoadPage();
    }

    private void UpdateTabCursor(int previousTabIndex, int currentTabIndex)
    {
        if (!_areMenuTabsActive.Value) return;
        _pages[previousTabIndex].TabCursor.gameObject.SetActive(false); 
        _pages[currentTabIndex].TabCursor.gameObject.SetActive(true);
    }

    private void MoveTabCursor(Vector2 inputDirection)
    {
        if ((int)inputDirection.x == 1 && _cursorTabIndex.Value + 1 < _pages.Count)
            _cursorTabIndex.Value++;
        else if ((int)inputDirection.x == -1 && _cursorTabIndex.Value - 1 >= 0)
            _cursorTabIndex.Value--;
        else if ((int)inputDirection.y == -1)
            _areMenuTabsActive.Value = false;
    }
}
