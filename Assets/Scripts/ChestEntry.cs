using UnityEngine;

public class ChestEntry : MonoBehaviour
{
    public enum ChestType
    {
        Common, 
        Rare, 
        Epic
    }

    public ChestType chestType; 
    public UnlockPanel unlockPanel;
    public ChestOpener opener;


    public void OpenChest()
    {
        switch(chestType)
        {
            case ChestType.Common:
            var upgrade = ReferenceManager.Instance.gameManager.GetCommonUpgrade();
            unlockPanel.ShowPanel(upgrade); 
                break;
            case ChestType.Rare:
            var upgrade2 = ReferenceManager.Instance.gameManager.GetRareUpgrade();
            unlockPanel.ShowPanel(upgrade2); 
                break;
            case ChestType.Epic:
            var upgrade3 = ReferenceManager.Instance.gameManager.GetEpicUpgrade();
            unlockPanel.ShowPanel(upgrade3); 
                break;
        }

        opener.ResetChestUI();
    } 

}
