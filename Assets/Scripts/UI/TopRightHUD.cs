using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopRightHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _clockText;
    [SerializeField] private TextMeshProUGUI _dateText;

    [SerializeField] private Sprite _springFrame;
    [SerializeField] private Sprite _summerFrame;
    [SerializeField] private Sprite _fallFrame;
    [SerializeField] private Sprite _winterFrame;
    private Image _frame;

    void Start()
    {
        _frame = GetComponent<Image>();
        GameClock.Instance.GameMinute.OnChange(curr => UpdateClockTextEveryFiveMinutes(curr));
        GameClock.Instance.GameHour.OnChange(_ => UpdateClockText());
        GameClock.Instance.GameDay.OnChange(_ => UpdateDateText());
        GameClock.Instance.GameSeason.OnChange(_ => UpdateSeasonFrame());

        UpdateClockText();
        UpdateDateText();
        UpdateSeasonFrame();
    }

    void UpdateSeasonFrame()
    {
        _frame.sprite = GameClock.Instance.GameSeason.Value switch
        {
            GameClock.Seasons.Spring => _springFrame,
            GameClock.Seasons.EndOfSpring => _springFrame,
            GameClock.Seasons.Summer => _summerFrame,
            GameClock.Seasons.EndOfSummer => _summerFrame,
            GameClock.Seasons.Fall => _fallFrame,
            GameClock.Seasons.EndOfFall => _fallFrame,
            GameClock.Seasons.Winter => _winterFrame,
            GameClock.Seasons.EndOfWinter => _winterFrame,
            _ => null
        };
    }

    void UpdateClockTextEveryFiveMinutes(int minute)
    {
        if (minute % 5 == 0) 
            UpdateClockText();
    }

    void UpdateClockText()
    {
        // 24h clock
        _clockText.text = GameClock.Instance.GameHour.Value.ToString() + ":";
        _clockText.text += GameClock.Instance.GameMinute.Value < 10 ? "0" : ""; // add a leading zero for <10 min
        _clockText.text += GameClock.Instance.GameMinute.Value.ToString();
    }

    void UpdateDateText()
    {
        int _gameDay = GameClock.Instance.GameDay.Value;

        // "1st" thru "15th" 
        _dateText.text = $"The {_gameDay}";
        _dateText.text += _gameDay switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

}
