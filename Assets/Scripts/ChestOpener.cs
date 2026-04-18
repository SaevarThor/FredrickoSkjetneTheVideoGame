using System.Collections.Generic;
using UnityEngine;

public class ChestOpener : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private GameObject chestEntryCommon, chestEntryEpic, chestEntryRare; 

    [SerializeField] private Transform chestEntryParent;

    private List<GameObject> activeChests = new List<GameObject>();

    [SerializeField] private GameObject NoChests; 
    public UnlockPanel unlockPanel; 

    private void OnEnable()
    {
        PopulateUI();
    }

    private void OnDisable()
    {
        ResetUI();
    }


    public void ResetChestUI()
    {
        ResetUI();
        PopulateUI();
    }


    private void PopulateUI()
    {
        if (gameManager == null) gameManager = ReferenceManager.Instance.gameManager;

        AddEntry(gameManager.CommonChestsOwned, chestEntryCommon); 
        AddEntry(gameManager.RareChestsOwned, chestEntryRare);
        AddEntry(gameManager.EpicChestsOwned, chestEntryEpic); 

        if (activeChests.Count == 0)
        {
            NoChests.SetActive(true);
        }
    }


    private void ResetUI()
    {
        for (int i = 0; i < activeChests.Count; i++)
        {
            Destroy(activeChests[i]);
        }

        activeChests.Clear();
        NoChests.SetActive(false); 
    }

    private void AddEntry(int amount, GameObject entry)
    {
        for (int i = 0; i < amount; i++)
        {
           GameObject g = Instantiate(entry, chestEntryParent); 
           g.GetComponent<ChestEntry>().unlockPanel = unlockPanel;
           g.GetComponent<ChestEntry>().opener = this; 
           activeChests.Add(g);
        }
    }


}
