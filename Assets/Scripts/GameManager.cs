
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public int Score = 0; 

    public int CommonChestsOwned = 0;
    public int RareChestsOwned = 0;
    public int EpicChestsOwned = 0;

    public List<LevelEntry> LevelEntries = new List<LevelEntry>();

    public List<Upgrade> CommonUpgrades = new List<Upgrade>();
    public List<Upgrade> RareUpgrades = new List<Upgrade>();
    public List<Upgrade> EpicUpgrades = new List<Upgrade>();


    public List <Upgrade> ActiveUpgrades = new List<Upgrade>();

    public void Start()
    {
        GeneraterCommonUpgrades();
        GenerateRareUpgrades();
        GenerateEpicUpgrades();
    }

    private void GeneraterCommonUpgrades()
    {
            CommonUpgrades.Add(new FireRateUpgrade(0.1f));
            CommonUpgrades.Add(new FireRateUpgrade(0.2f));
            CommonUpgrades.Add(new ReloadSpeedUpgrade(0.1f));
            CommonUpgrades.Add(new ReloadSpeedUpgrade(0.2f));
            CommonUpgrades.Add(new LargerMagUpgrade(1));
            CommonUpgrades.Add(new DamageBonusUpgrade(0.1f)); 
            CommonUpgrades.Add(new FasterPlayerMovementUpgrade(0.05f)); 
    }

    private void GenerateRareUpgrades()
    {
        RareUpgrades.Add(new FireRateUpgrade(0.3f));
        RareUpgrades.Add(new FireRateUpgrade(0.4f));
        RareUpgrades.Add(new ReloadSpeedUpgrade(0.4f));
        RareUpgrades.Add(new ReloadSpeedUpgrade(0.5f));
        RareUpgrades.Add(new FasterPlayerMovementUpgrade(0.1f));
        RareUpgrades.Add(new FasterPlayerMovementUpgrade(0.2f));
        RareUpgrades.Add(new LargerMagUpgrade(3));
        RareUpgrades.Add(new DamageBonusUpgrade(0.3f));
    }
    
    private void GenerateEpicUpgrades()
    {
        EpicUpgrades.Add(new FireRateUpgrade(0.6f));
        EpicUpgrades.Add(new FireRateUpgrade(0.7f));
        EpicUpgrades.Add(new ReloadSpeedUpgrade(0.7f));
        EpicUpgrades.Add(new ReloadSpeedUpgrade(0.8f));
        EpicUpgrades.Add(new FasterPlayerMovementUpgrade(0.3f));
        EpicUpgrades.Add(new FasterPlayerMovementUpgrade(0.4f));
        EpicUpgrades.Add(new LargerMagUpgrade(8)); 
        EpicUpgrades.Add(new DamageBonusUpgrade(0.5f)); 
    }

    [ContextMenu("Test Upgrade")]
    public void TestUpgrade()
    {
        var up = new LargerMagUpgrade(8); 

        up.ApplyUpgrade();
    }

    public Upgrade GetCommonUpgrade()
    {
        if (CommonUpgrades.Count == 0) return null;

        Upgrade upgrade = CommonUpgrades[Random.Range(0, CommonUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        CommonChestsOwned--; 
        return upgrade;
    }

    public Upgrade GetRareUpgrade()
    {
        if (RareUpgrades.Count == 0) return null;

        Upgrade upgrade = RareUpgrades[Random.Range(0, RareUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        RareChestsOwned--;
        return upgrade;
    }

    public Upgrade GetEpicUpgrade()
    {
        if (EpicUpgrades.Count == 0) return null;

        Upgrade upgrade = EpicUpgrades[Random.Range(0, EpicUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        EpicChestsOwned--;
        return upgrade;
    }

    public void ApplyActiveUpgrades()
    {
        foreach (Upgrade upgrade in ActiveUpgrades)
        {
            upgrade.ApplyUpgrade();
        }
    }

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
