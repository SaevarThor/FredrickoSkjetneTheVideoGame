using UnityEngine;

public class ShotgunSpreadUpgrade : Upgrade
{
     public override string UpgradeName {get; set;} = "Extra Pellets" ;
    public override string UpgradeDescription {get; set;}

    private float additionalRange;

    public ShotgunSpreadUpgrade(float amount)
    {
        additionalRange = amount; 
        UpgradeDescription = $"Shotgun Spread is decreased by {additionalRange * 100}%" ;
    }

    public override void ApplyUpgrade()
    {
       var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
       shotgunShooter.LowerSpread(additionalRange); 
       
    }
}
