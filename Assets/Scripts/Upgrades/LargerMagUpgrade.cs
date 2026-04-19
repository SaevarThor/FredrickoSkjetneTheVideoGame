using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class LargerMagUpgrade : Upgrade
{
    public override string UpgradeName { get; set; } = "Larger Magazine";
    public override string UpgradeDescription { get; set; }

    public ShotgunShooter _shotgunShooter;
    public float MagSizeUpgrade = 1; 

    public LargerMagUpgrade (float amount)
    {
        MagSizeUpgrade = amount; 
        UpgradeDescription = $"{MagSizeUpgrade} more shots in a mag"; 
    }

    public override void ApplyUpgrade()
    {
        _shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        Debug.Log("Adding mag size upgrade"); 
        _shotgunShooter.ApplyMagSizeUpgrade(MagSizeUpgrade); 
    }
}