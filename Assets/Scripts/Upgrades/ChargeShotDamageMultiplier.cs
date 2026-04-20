


using UnityEngine;

public class ChargeShotDamageMultiplier: Upgrade
{
    public override string UpgradeName { get; set; } = "Charge Shot damage multiplier";
    public override string UpgradeDescription { get; set; }

    private float _multiplier;

    public ChargeShotDamageMultiplier(float multiplier)
    {
                _multiplier = multiplier;
                UpgradeDescription = $"{multiplier}x multiplier";
    }

    public override void ApplyUpgrade()
    {
        var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        shotgunShooter.ApplyChargeShotMultiplier(_multiplier);

    }

}
