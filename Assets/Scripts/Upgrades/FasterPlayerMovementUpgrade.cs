using UnityEngine;

public class FasterPlayerMovementUpgrade : Upgrade
{
    public override string UpgradeName { get; set; } = "Faster Player Movement";
    public override string UpgradeDescription { get; set; } = "Increases your movement speed by 20%";

    private float speedIncrease = 0.2f; 
    public FasterPlayerMovementUpgrade(float speedIncrease)
    {
        UpgradeDescription = $"Increases your movement speed by {speedIncrease * 100}%";
        this.speedIncrease = speedIncrease;
    }

    public override void ApplyUpgrade()
    {
        PlayerMovement playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        playerMovement.ApplySpeedUpgrade(speedIncrease);
    }
}
