using UnityEngine;

public class UpgradePickUp : MonoBehaviour
{
    public enum UpgradeType
    {
        common, 
        rare, 
        epic, 
    }

    public UpgradeType upgradeType; 


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var upgrade = GetUpgrade();

            ReferenceManager.Instance.mManager.UpdateObjective(upgrade.UpgradeName, upgrade.UpgradeDescription);

            Destroy(this.gameObject);
        }
    }


    private Upgrade GetUpgrade()
    {
        var gameManager = ReferenceManager.Instance.gameManager; 


        switch(upgradeType)
        {
            case UpgradeType.common:
                return gameManager.GetCommonUpgrade(); 
            case UpgradeType.rare:
                return gameManager.GetRareUpgrade();
            case UpgradeType.epic:
                return gameManager.GetEpicUpgrade();
                
        }

        return null; 
    }
}
