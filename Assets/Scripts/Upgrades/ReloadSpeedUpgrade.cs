using UnityEngine;


public class ReloadSpeedUpgrade : Upgrade
{
    private ShotgunShooter _shotgunShooter;
    private float _reloadSpeedIncrease;

    public override string UpgradeName { get; set; } = "Reload Speed";
    public override string UpgradeDescription { get; set; }

    public ReloadSpeedUpgrade(float reloadSpeedIncrease)
    {
        UpgradeDescription = $"Increases your reload speed by {reloadSpeedIncrease * 100}%";
        _shotgunShooter = GameObject.FindWithTag("Player").GetComponent<ShotgunShooter>();
        _reloadSpeedIncrease = reloadSpeedIncrease;
    }

    public override void ApplyUpgrade()
    {
        _shotgunShooter.ApplyReloadSpeedUpgrade(_reloadSpeedIncrease);
    }
}