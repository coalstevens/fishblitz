using System;
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
        public Sprite TabHighlight;
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
    [SerializeField] private Image _tabCursorRenderer;
    [SerializeField] private List<MenuPage> _pages = new();

    private Reactive<bool> _areMenuTabsActive = new Reactive<bool>(true);
    private Reactive<int> _currentPage = new Reactive<int>(1);
    private Reactive<int> _cursorTab = new Reactive<int>(1);
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _unsubscribeHooks.Add(_cursorTab.OnChange(_ => RefreshTabCursorSprite()));
        _unsubscribeHooks.Add(_currentPage.OnChange((prev, curr) => OnPageChange(prev, curr)));
        _unsubscribeHooks.Add(_areMenuTabsActive.OnChange(curr => TogglePageCursor(curr)));

        _tabCursorRenderer.gameObject.SetActive(true);
        _pages[_currentPage.Value].Page.LoadPage();
        RefreshTabCursorSprite();

        //StartCoroutine(UpdateCursorAfterDelay());
    }
    private void OnDisable()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook();
        _unsubscribeHooks.Clear();
    }

    private void TogglePageCursor(bool curr)
    {
        if (curr)
        {
            _tabCursorRenderer.gameObject.SetActive(true);
            _pages[_currentPage.Value].Page.DisableCursor();
        }
        else
        {
            _tabCursorRenderer.gameObject.SetActive(false);
            _pages[_currentPage.Value].Page.EnableCursor();
        }
    }


    // private IEnumerator UpdateCursorAfterDelay()
    // {
    //     yield return null;
    //     MoveEntryCursor();
    // }

    // Player input method
    public void OnMoveCursor(InputValue value)
    {
        if (_areMenuTabsActive.Value)
            MoveMenuTabCursor(value.Get<Vector2>());
        else
            if(!_pages[_currentPage.Value].Page.MoveCursor(value.Get<Vector2>()))
                _areMenuTabsActive.Value = true;
    }

    // Player input method
    public void OnSelect()
    {
        if (_areMenuTabsActive.Value && _cursorTab.Value != _currentPage.Value)
            _currentPage.Value = _cursorTab.Value;
        else
            _pages[_currentPage.Value].Page.Select();
    }

    private void OnPageChange(int previousPage, int currentPage)
    {
        _pages[previousPage].Page.UnloadPage();
        _pages[currentPage].Page.LoadPage();
    }

    private void RefreshTabCursorSprite()
    {
        if (_areMenuTabsActive.Value)
            _tabCursorRenderer.sprite = _pages[_cursorTab.Value].TabHighlight;
    }

    private void MoveMenuTabCursor(Vector2 inputDirection)
    {
        if ((int)inputDirection.x == 1 && _currentPage.Value + 1 < _pages.Count)
            _cursorTab.Value++;
        else if ((int)inputDirection.x == -1 && _currentPage.Value - 1 >= 0)
            _cursorTab.Value--;
        else if ((int)inputDirection.y == -1)
            _areMenuTabsActive.Value = false;
    }
}
