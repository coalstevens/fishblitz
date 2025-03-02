using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCaptureLog", menuName = "CaptureLog")]
public class CaptureLog : ScriptableObject 
{
    public class CapturePeriod
    {
        public List<GameClock.Seasons> CaughtSeasons = new();
        public List<GameClock.DayPeriods> CaughtDayPeriods = new();
        public CapturePeriod(List<GameClock.Seasons> caughtSeasons, List<GameClock.DayPeriods> caughtPeriods)
        {
            CaughtSeasons = caughtSeasons;
            CaughtDayPeriods = caughtPeriods;
        }
    }

    public int NumberCaught = 0;
    public Dictionary<string, CapturePeriod> CaughtCreatures = new();

    /// <summary>
    /// Adds name to the log and returns true for first-time captures.
    /// </summary>
    public bool AddToLog(string name, GameClock.Seasons caughtSeason, GameClock.DayPeriods caughtPeriod)
    {
        NumberCaught++;
        // Check if the bird already exists in the log
        if (CaughtCreatures.TryGetValue(name, out CapturePeriod existingEntry))
        {
            if (!existingEntry.CaughtSeasons.Contains(caughtSeason))
                existingEntry.CaughtSeasons.Add(caughtSeason);

            if (!existingEntry.CaughtDayPeriods.Contains(caughtPeriod))
                existingEntry.CaughtDayPeriods.Add(caughtPeriod);

            return false;
        }

        // Add a new bird entry for the first capture
        CaughtCreatures[name] = new CapturePeriod
        (
            new List<GameClock.Seasons> { caughtSeason },
            new List<GameClock.DayPeriods> { caughtPeriod }
        );

        Debug.Log($"Caught a {name}.");

        return true;
    }
}
