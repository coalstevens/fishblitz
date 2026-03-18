using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCaptureLog", menuName = "CaptureLog")]
public class CaptureLog : ScriptableObject 
{
    [System.Serializable]
    public class CaptureEntry
    {
        public string name;
        public int yearCaught;
        public GameClock.Seasons seasonCaught;
        public int dayCaught;
        public int hourCaught;
        public int minuteCaught;

        public CaptureEntry(string name, int year, GameClock.Seasons season, int day, int hour, int minute)
        {
            this.name = name;
            yearCaught = year;
            seasonCaught = season;
            dayCaught = day;
            hourCaught = hour;
            minuteCaught = minute;
        }
    }

    public List<CaptureEntry> CaptureTable = new();

    /// <summary>
    /// Adds a new row to the capture table for every capture.
    /// Returns true if this is the first time this name was caught.
    /// </summary>
    public bool AddToLog(string name)
    {
        var entry = new CaptureEntry(
            name,
            GameClock.Instance.GameYear,
            GameClock.Instance.GameSeason,
            GameClock.Instance.GameDay,
            GameClock.Instance.GameHour,
            GameClock.Instance.GameMinute
        );

        CaptureTable.Add(entry);

        bool isFirstCapture = CaptureTable.FindIndex(e => e.name == name) == CaptureTable.Count - 1;

        Debug.Log($"Caught a {name} at {entry.seasonCaught} {entry.dayCaught} {entry.hourCaught}:{entry.minuteCaught} (Year {entry.yearCaught})");

        return isFirstCapture;
    }

    /// <summary>
    /// Returns the total number of captures.
    /// </summary>
    public int GetTotalCaptures()
    {
        return CaptureTable.Count;
    }

    /// <summary>
    /// Returns the total number of unique names captured.
    /// </summary>
    public int GetUniqueCaptureCount()
    {
        var uniqueNames = new HashSet<string>();
        foreach (var entry in CaptureTable)
        {
            uniqueNames.Add(entry.name);
        }
        return uniqueNames.Count;
    }

    /// <summary>
    /// Returns the number of times a certain name has been caught.
    /// </summary>
    public int GetCaptureCountForName(string name)
    {
        int count = 0;
        foreach (var entry in CaptureTable)
            if (entry.name == name)
                count++;
        return count;
    }

    /// <summary>
    /// Returns the number of times a certain name has been caught in the current week.
    /// </summary>
    public int GetCaptureCountForNameThisWeek(string name)
    {
        int count = 0;
        int currentSeason = (int)GameClock.Instance.GameSeason;
        int currentWeek = (GameClock.Instance.GameDay - 1) / 5;

        foreach (var entry in CaptureTable)
        {
            if (entry.name == name &&
                (int)entry.seasonCaught == currentSeason &&
                ((entry.dayCaught - 1) / 5) == currentWeek)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Returns a 4x3 bool table [season, week] indicating if this name was caught in each season/week.
    /// Seasons: 0=Spring, 1=Summer, 2=Fall, 3=Winter
    /// Weeks: 0=days 1-5, 1=days 6-10, 2=days 11-15
    /// </summary>
    public bool[,] GetSeasonWeekTable(string name)
    {
        bool[,] table = new bool[4, 3];

        foreach (var entry in CaptureTable)
        {
            if (entry.name != name)
                continue;

            int seasonIdx = (int)entry.seasonCaught;
            int weekIdx = (entry.dayCaught - 1) / 5;
            if (seasonIdx >= 0 && seasonIdx < 4 && weekIdx >= 0 && weekIdx < 3)
                table[seasonIdx, weekIdx] = true;
        }

        return table;
    }

    /// <summary>
    /// Returns true if the given name has been caught (exists in the log).
    /// </summary>
    public bool HasBeenCaught(string name)
    {
        foreach (var entry in CaptureTable)
        {
            if (entry.name == name)
                return true;
        }
        return false;
    }
}
