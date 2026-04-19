using UnityEngine;

public class LargerMagUpgrade : Upgrade
{
    public override string UpgradeName { get; set; } = "Larger Magazine";
    public override string UpgradeDescription { get; set; }

    public float MagSizeUpgrade = 1; 

    public LargerMagUpgrade (float amount)
    {
        UpgradeDescription = $"{MagSizeUpgrade} more shots in a mag"; 
        MagSizeUpgrade = amount; 
    }

    public override void ApplyUpgrade()
    {
        
    }
}