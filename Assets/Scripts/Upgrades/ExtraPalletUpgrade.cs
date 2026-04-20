using UnityEngine;

public class ExtraPalletUpgrade : Upgrade
{
    public override string UpgradeName {get; set;} = "Extra Pellets" ;
    public override string UpgradeDescription {get; set;}

    private int extraPellets;

    public ExtraPalletUpgrade(int amount)
    {
        extraPellets = amount; 
        UpgradeDescription = $"{extraPellets} additional pellets per shot" ;
    }

    public override void ApplyUpgrade()
    {
       var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
       shotgunShooter.AddPellets(extraPellets); 
       
    }
}
