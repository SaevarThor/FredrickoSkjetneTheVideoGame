using TMPro;
using UnityEngine;

public class UnlockPanel : MonoBehaviour
{
    public GameObject Panel; 

    public TMP_Text Label;
    public TMP_Text Description; 


    public void ShowPanel(Upgrade upgrade)
    {
        Panel.SetActive(true);
        Label.text = upgrade.UpgradeName;
        Description.text = upgrade.UpgradeDescription; 
    }
}
