using UnityEngine;

public class DamageBonusUpgrade : Upgrade
{
   
    public override string UpgradeName { get; set; } = "Faster Player Movement";
    public override string UpgradeDescription { get; set; } = "Increases your movement speed by 20%";


    private float damageIncrease = 0.2f; 
    public DamageBonusUpgrade(float damageIncrease)
    {
        UpgradeDescription = $"Increases your movement speed by {damageIncrease * 100}%";
        this.damageIncrease = damageIncrease;
    }

    public override void ApplyUpgrade()
    {
        var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        shotgunShooter.ApplyDamageBuff(damageIncrease);
    } 
}
