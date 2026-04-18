using UnityEngine;

public class FireRateUpgrade : Upgrade
{
    [SerializeField] private ShotgunShooter shotgunShooter;
    [SerializeField] private float fireRateMultiplier = 0.2f;

    public override string UpgradeName { get; set; } = "Fire Rate";
    public override string UpgradeDescription { get; set; }

    public FireRateUpgrade(float fireRateMultiplier)
    {
        UpgradeDescription = $"Increases your fire rate by {fireRateMultiplier * 100}%";
        this.fireRateMultiplier = fireRateMultiplier;
    }

    public override void ApplyUpgrade()
    {
        shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        shotgunShooter.ApplyFireRateUpgrade(fireRateMultiplier);
    }
}
