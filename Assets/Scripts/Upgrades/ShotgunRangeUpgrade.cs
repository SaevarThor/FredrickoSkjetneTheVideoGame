using UnityEngine;

public class ShotgunRangeUpgrade : Upgrade
{
    public override string UpgradeName {get; set;} = "Extra Pellets" ;
    public override string UpgradeDescription {get; set;}

    private float additionalRange;

    public ShotgunRangeUpgrade(float amount)
    {
        additionalRange = amount; 
        UpgradeDescription = $"Shotgun Range increase by {additionalRange} meters" ;
    }

    public override void ApplyUpgrade()
    {
       var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
       shotgunShooter.AddRange(additionalRange); 
       
    }
}
