using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [System.Serializable]
    private class JournalPage
    {
        public Sprite FrameSprite;
        public CaptureLog Log;
        public Transform LeftPage;
        public Transform RightPage;
    }

    [Header("General")]
    [SerializeField] private Image _frame;
    [SerializeField] private List<JournalPage> _journalPages = new();
    [SerializeField] private Logger _logger = new();

    [Header("Notebook")]
    [SerializeField] private PixelCanvasTextRenderer _noteBookTitle;
    [SerializeField] private PixelCanvasTextRenderer _tagText;
    [SerializeField] private Transform _dateDotsContainer;
    [SerializeField] private Image _notebookImage;
    [SerializeField] private PixelCanvasTextRenderer _counterText;

    private int _currentJournalPageIndex;
    private Transform _selectedEntry;
    private List<List<Transform>> _pageEntries = new();

    private void OnEnable()
    {
        _pageEntries.Clear();
        foreach (var page in _journalPages)
            _pageEntries.Add(CollectJournalPageEntries(page));

        LoadJournalPage(_currentJournalPageIndex);
        WireEntryButtons();
    }

    private void OnDisable()
    {
    }

    public void SwitchToJournalPage(int index)
    {
        if (index < 0 || index >= _journalPages.Count || index == _currentJournalPageIndex)
            return;

        _currentJournalPageIndex = index;
        LoadJournalPage(index);
        WireEntryButtons();
        _logger.Info($"Journal switched to page {index}");
    }

    public void OnEntrySelected(Transform entry)
    {
        _selectedEntry = entry;
        UpdateNotebookDisplay(_journalPages[_currentJournalPageIndex]);
    }

    private void LoadJournalPage(int pageIndex)
    {
        for (int i = 0; i < _journalPages.Count; i++)
        {
            bool active = i == pageIndex;
            _journalPages[i].LeftPage.parent.gameObject.SetActive(active);
            // _journalPages[i].LeftPage.gameObject.SetActive(active);
            // _journalPages[i].RightPage.gameObject.SetActive(active);
        }

        var page = _journalPages[pageIndex];
        _frame.sprite = page.FrameSprite;
        _counterText.Text = $"{page.Log.GetUniqueCaptureCount()} / {_pageEntries[pageIndex].Count}";

        foreach (Transform child in page.LeftPage)
        {
            bool isNameInLog = page.Log.HasBeenCaught(child.gameObject.name);
            child.GetChild(0).gameObject.SetActive(!isNameInLog);
            child.GetChild(1).gameObject.SetActive(isNameInLog);
        }

        foreach (Transform child in page.RightPage)
        {
            bool isNameInLog = page.Log.HasBeenCaught(child.gameObject.name);
            child.GetChild(0).gameObject.SetActive(!isNameInLog);
            child.GetChild(1).gameObject.SetActive(isNameInLog);
        }
    }

    private void Start()
    {
        var entries = _pageEntries[_currentJournalPageIndex];
        if (entries.Count > 0)
            OnEntrySelected(entries[0]);
    }

    private void WireEntryButtons()
    {
        foreach (Transform entry in _pageEntries[_currentJournalPageIndex])
        {
            Button btn = entry.GetComponent<Button>();
            if (btn == null)
            {
                btn = entry.gameObject.AddComponent<Button>();
                btn.targetGraphic = entry.GetComponentInChildren<Image>();
            }

            JournalEntry je = entry.GetComponent<JournalEntry>();
            if (je == null)
                je = entry.gameObject.AddComponent<JournalEntry>();
            je.Journal = this;

            btn.onClick.RemoveAllListeners();
            Transform captured = entry;
            btn.onClick.AddListener(() => OnEntrySelected(captured));
        }
    }

    private void UpdateNotebookDisplay(JournalPage page)
    {
        if (_selectedEntry == null)
            return;

        string titleText = _selectedEntry.name;
        bool hasBeenCaught = page.Log.HasBeenCaught(titleText);
        titleText = hasBeenCaught ? titleText : "???";
        int caughtThisWeek = page.Log.GetCaptureCountForNameThisWeek(titleText);
        int caughtAllTime = page.Log.GetCaptureCountForName(titleText);

        _notebookImage.sprite = _selectedEntry.GetChild(1).GetComponent<NotebookSprite>().sprite;
        _notebookImage.enabled = hasBeenCaught;

        _noteBookTitle.Text = titleText;
        _tagText.Text = $"{caughtThisWeek} tagged this week\n{caughtAllTime} tagged all time";
        UpdateDateDots(page, titleText);
    }

    private void UpdateDateDots(JournalPage page, string name)
    {
        bool[,] seasonWeekTable = page.Log.GetSeasonWeekTable(name);

        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
                _dateDotsContainer.GetChild(i * 3 + j).GetComponent<Image>().enabled = seasonWeekTable[i, j];
    }

    private List<Transform> CollectJournalPageEntries(JournalPage page)
    {
        List<Transform> allEntries = new();
        foreach (Transform child in page.LeftPage)
            if (child.gameObject.activeSelf)
                allEntries.Add(child);
        foreach (Transform child in page.RightPage)
            if (child.gameObject.activeSelf)
                allEntries.Add(child);
        return allEntries;
    }
}
