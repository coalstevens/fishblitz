using System.Collections.Generic;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public Vector3 SceneSpawnPosition = new Vector3(0,0);
    public class BirdingLogEntry {
        public string Name;
        public List<GameClock.Seasons> CaughtSeasons = new();
        public List<GameClock.DayPeriods> CaughtDayPeriods = new();
    }

    public class BirdingLog {
        public int NumberOfCaughtBirds;
        public List<BirdingLogEntry> CaughtBirds = new();
    }
    
    public BirdingLog PlayerBirdingLog = new();
}
