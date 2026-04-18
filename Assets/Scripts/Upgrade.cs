using UnityEngine;
using System; 

[Serializable]
public abstract class Upgrade
{
    public abstract string UpgradeName {get; set;}
    public abstract string UpgradeDescription {get; set;} 

    public abstract void ApplyUpgrade(); 
}
