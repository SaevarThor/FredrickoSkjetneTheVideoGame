using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<LevelEntry> LevelEntries = new List<LevelEntry>();


    public LevelEntry GetLevelEntry(int levelIndex)
    {
        return LevelEntries.FirstOrDefault(entry => entry.LevelIndex == levelIndex);
    }

    public void AddLevelEntry(LevelEntry entry)
    {
        var oldEntry = LevelEntries.FirstOrDefault(e => e.LevelIndex == entry.LevelIndex);
        if (oldEntry != null)
        {
            oldEntry.TimeTaken = entry.TimeTaken;
            oldEntry.EnemiesKilled = entry.EnemiesKilled;
            oldEntry.Score = entry.Score;
            return;
        }

        LevelEntries.Add(entry);
    }

}


public class LevelEntry
{
    public int LevelIndex;
    public float TimeTaken;
    public int EnemiesKilled; 
    public float Score; 

}
