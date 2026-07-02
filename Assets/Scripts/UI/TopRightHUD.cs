using UnityEngine;
using UnityEngine.UI;

public class TopRightHUD : MonoBehaviour
{
    [SerializeField] private PixelCanvasTextRenderer _clockText;
    [SerializeField] private PixelCanvasTextRenderer _dateText;

    [SerializeField] private Sprite _springFrame;
    [SerializeField] private Sprite _summerFrame;
    [SerializeField] private Sprite _fallFrame;
    [SerializeField] private Sprite _winterFrame;
    private Image _frame;

    void OnEnable()
    {
        _frame = GetComponent<Image>();
        GameClock.Instance.OnGameMinuteTick += UpdateClockTextEveryFiveMinutes;
        GameClock.Instance.OnGameHourTick += UpdateClockText;
        GameClock.Instance.OnGameDayTick += UpdateDateText;
        GameClock.Instance.OnGameSeasonTick += UpdateSeasonFrame; 
        
        UpdateClockText();
        UpdateDateText();
        UpdateSeasonFrame();
    }

    void OnDisable()
    {
        GameClock.Instance.OnGameMinuteTick -= UpdateClockTextEveryFiveMinutes;
        GameClock.Instance.OnGameHourTick -= UpdateClockText;
        GameClock.Instance.OnGameDayTick -= UpdateDateText;
        GameClock.Instance.OnGameSeasonTick -= UpdateSeasonFrame; 
    }

    void UpdateSeasonFrame()
    {
        _frame.sprite = GameClock.Instance.GameSeason switch
        {
            GameClock.Seasons.Spring => _springFrame,
            GameClock.Seasons.Summer => _summerFrame,
            GameClock.Seasons.Fall => _fallFrame,
            GameClock.Seasons.Winter => _winterFrame,
            _ => null
        };
    }

    void UpdateClockTextEveryFiveMinutes()
    {
        if (GameClock.Instance.GameMinute % 5 == 0) 
            UpdateClockText();
    }

    void UpdateClockText()
    {
        // 24h clock
        _clockText.Text = GameClock.Instance.GameHour.ToString() + ":";
        _clockText.Text += GameClock.Instance.GameMinute < 10 ? "0" : "";
        _clockText.Text += GameClock.Instance.GameMinute.ToString();
    }

    void UpdateDateText()
    {
        int _gameDay = GameClock.Instance.GameDay;

        // "1st" thru "15th"  
        _dateText.Text = $"{_gameDay}";
        _dateText.Text += _gameDay switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

}
