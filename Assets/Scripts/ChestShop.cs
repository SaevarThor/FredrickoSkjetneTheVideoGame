using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChestShop : MonoBehaviour
{
    public TMP_Text CurrencyText;

    private GameManager gameManager;

    [SerializeField] private int commonChestPrice = 100; 
    [SerializeField] private int rareChestPrice = 200; 
    [SerializeField] private int epicChestPrice = 400; 

    [SerializeField] private Button commonButton;
    [SerializeField] private Button rareButton; 
    [SerializeField] private Button epicButton; 



    public void Start()
    {
        gameManager = ReferenceManager.Instance.gameManager; 
        UpdateUI();

        commonButton.onClick.AddListener(BuyCommonChest);
        rareButton.onClick.AddListener(BuyRareChest);
        epicButton.onClick.AddListener(BuyEpicChest);

    }

    public void UpdateUI()
    {
        var score = gameManager.Score; 
        CurrencyText.text = $"Your Current Money is {score}"; 

        commonButton.interactable = score > commonChestPrice; 
        rareButton.interactable = score > rareChestPrice; 
        epicButton.interactable = score > epicChestPrice; 

    }

    public void BuyCommonChest()
    {
        gameManager.CommonChestsOwned++; 
        gameManager.Score -= commonChestPrice; 

        UpdateUI();
    }

    public void BuyRareChest()
    {
        gameManager.RareChestsOwned++; 
        gameManager.Score -= rareChestPrice; 

        UpdateUI();
    }

    public void BuyEpicChest()
    {
        gameManager.EpicChestsOwned++; 
        gameManager.Score -= epicChestPrice; 

        UpdateUI();
    }
}
