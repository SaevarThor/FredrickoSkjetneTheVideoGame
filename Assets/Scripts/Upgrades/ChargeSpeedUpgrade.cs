using UnityEngine;
using System.Collections;


public class ChargeSpeedUpgrade: Upgrade
{

    public override string UpgradeName { get; set; } = "Charge Upgrade";
    public override string UpgradeDescription { get; set; } = "Increases your charge up by 10%";


    private float chargeIncrease = 0.1f;
    public ChargeSpeedUpgrade(float _chargeIncrease)
    {
        UpgradeDescription = $"Increases your damage by {_chargeIncrease * 100}%";
        this.chargeIncrease = _chargeIncrease;
    }

    public override void ApplyUpgrade()
    {
        var shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        shotgunShooter.ApplyChargeUpgrade(chargeIncrease);
    }
}
