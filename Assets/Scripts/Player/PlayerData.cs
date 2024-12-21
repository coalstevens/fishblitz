using System.Collections.Generic;
using UnityEngine;

public static class PlayerData 
{
    public static Vector3 SceneSpawnPosition = new Vector3(0,0);
    public class BirdCapturePeriod {
        public List<GameClock.Seasons> CaughtSeasons = new();
        public List<GameClock.DayPeriods> CaughtDayPeriods = new();
        public BirdCapturePeriod(string birdName, List<GameClock.Seasons> caughtSeasons, List<GameClock.DayPeriods> caughtPeriods) {
            CaughtSeasons = caughtSeasons;
            CaughtDayPeriods = caughtPeriods;
        }
    }

    public class BirdingLog
    {
        public int NumberOfCaughtBirds;
        public Dictionary<string, BirdCapturePeriod> CaughtBirds = new(); 
    }

    public static BirdingLog PlayerBirdingLog = new();

    /// <summary>
    /// Adds a bird to the log and returns true for first-time captures.
    /// </summary>
    public static bool AddToBirdingLog(Bird caughtBird)
    {
        PlayerBirdingLog.NumberOfCaughtBirds++;

        // Check if the bird already exists in the log
        if (PlayerBirdingLog.CaughtBirds.TryGetValue(caughtBird.BirdName, out BirdCapturePeriod existingEntry))
        {
            if (!existingEntry.CaughtSeasons.Contains(caughtBird.SeasonSpawned))
                existingEntry.CaughtSeasons.Add(caughtBird.SeasonSpawned);

            if (!existingEntry.CaughtDayPeriods.Contains(caughtBird.PeriodSpawned))
                existingEntry.CaughtDayPeriods.Add(caughtBird.PeriodSpawned);

            return false; 
        }

        // Add a new bird entry for the first capture
        PlayerBirdingLog.CaughtBirds[caughtBird.BirdName] = new BirdCapturePeriod(
            caughtBird.BirdName,
            new List<GameClock.Seasons> { caughtBird.SeasonSpawned },
            new List<GameClock.DayPeriods> { caughtBird.PeriodSpawned }
        );

        Debug.Log($"Caught a {caughtBird.BirdName}.");

        return true; 
    }
}
