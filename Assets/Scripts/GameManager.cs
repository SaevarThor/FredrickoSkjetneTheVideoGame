
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

    public float GameTime; 


    public List <Upgrade> ActiveUpgrades = new List<Upgrade>();

    public void Start()
    {
        GeneraterCommonUpgrades();
        GenerateRareUpgrades();
        GenerateEpicUpgrades();
    }

    private void GeneraterCommonUpgrades()
    {
        CommonUpgrades.Add(new FireRateUpgrade(0.05f));
        CommonUpgrades.Add(new ReloadSpeedUpgrade(0.05f));
        CommonUpgrades.Add(new ReloadSpeedUpgrade(0.075f));
        CommonUpgrades.Add(new LargerMagUpgrade(1));
        CommonUpgrades.Add(new DamageBonusUpgrade(0.1f)); 
        CommonUpgrades.Add(new ShotgunRangeUpgrade(2)); 
        CommonUpgrades.Add(new ShotgunSpreadUpgrade(0.05f));
        CommonUpgrades.Add(new ChargeSpeedUpgrade(0.1f));
    }

    private void GenerateRareUpgrades()
    {
        RareUpgrades.Add(new FireRateUpgrade(0.1f));
        RareUpgrades.Add(new FireRateUpgrade(0.125f));
        RareUpgrades.Add(new ReloadSpeedUpgrade(0.1f));
        RareUpgrades.Add(new ReloadSpeedUpgrade(0.15f));
        RareUpgrades.Add(new FasterPlayerMovementUpgrade(0.05f));
        RareUpgrades.Add(new FasterPlayerMovementUpgrade(0.1f));
        RareUpgrades.Add(new LargerMagUpgrade(3));
        RareUpgrades.Add(new DamageBonusUpgrade(0.3f));
        RareUpgrades.Add(new ExtraPalletUpgrade(1));
        RareUpgrades.Add(new ShotgunRangeUpgrade(5)); 
        RareUpgrades.Add(new ShotgunSpreadUpgrade(0.1f));
        RareUpgrades.Add(new ChargeSpeedUpgrade(0.2f));
        RareUpgrades.Add(new ChargeShotDamageMultiplier(1.5f));
    }
    
    private void GenerateEpicUpgrades()
    {
        EpicUpgrades.Add(new FireRateUpgrade(0.15f));
        EpicUpgrades.Add(new FireRateUpgrade(0.2f));
        EpicUpgrades.Add(new ReloadSpeedUpgrade(0.2f));
        EpicUpgrades.Add(new ReloadSpeedUpgrade(0.25f));
        EpicUpgrades.Add(new FasterPlayerMovementUpgrade(0.2f));
        EpicUpgrades.Add(new FasterPlayerMovementUpgrade(0.25f));
        EpicUpgrades.Add(new LargerMagUpgrade(8)); 
        EpicUpgrades.Add(new DamageBonusUpgrade(0.5f)); 
        EpicUpgrades.Add(new ExtraPalletUpgrade(2));
        EpicUpgrades.Add(new ShotgunRangeUpgrade(10)); 
        EpicUpgrades.Add(new ShotgunSpreadUpgrade(0.2f));
        EpicUpgrades.Add(new ChargeSpeedUpgrade(0.3f));
        RareUpgrades.Add(new ChargeShotDamageMultiplier(3f));
    }

    [ContextMenu("Test Upgrade")]
    public void TestUpgrade()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach(var item in EpicUpgrades)
            {
                item.ApplyUpgrade();
            }
        }
    }

    public Upgrade GetCommonUpgrade(bool takeChest = false)
    {
        if (CommonUpgrades.Count == 0) return null;

        Upgrade upgrade = CommonUpgrades[Random.Range(0, CommonUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        if (takeChest)
            CommonChestsOwned--; 
        return upgrade;
    }

    public Upgrade GetRareUpgrade(bool takeChest = false)
    {
        if (RareUpgrades.Count == 0) return null;

        Upgrade upgrade = RareUpgrades[Random.Range(0, RareUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        if (takeChest)
            RareChestsOwned--;
        return upgrade;
    }

    public Upgrade GetEpicUpgrade(bool takeChest = false)
    {
        if (EpicUpgrades.Count == 0) return null;

        Upgrade upgrade = EpicUpgrades[Random.Range(0, EpicUpgrades.Count)];
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade();
        if (takeChest)
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


    void Update()
    {
        GameTime += Time.deltaTime;
    }

}


public class LevelEntry
{
    public int LevelIndex;
    public float TimeTaken;
    public int EnemiesKilled; 
    public float Score; 

}
