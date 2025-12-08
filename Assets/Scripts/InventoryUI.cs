using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI goldText;

    public Button[] collectButtons;
    public GameObject inventoryPanel;

    public CreateListingUI createListingUI;

      void Start()
    {
        if(ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnInventoryChanged += UpdateUI;
        }
        inventoryPanel.SetActive(false);
      
        string[] resources = { "wood", "stone", "gold" };
        for (int i = 0; i < collectButtons.Length; i++)
        {
            int index = i;
            Button btn = collectButtons[i];
            btn.onClick.AddListener(() => StartCollection(resources[index], btn));
        }

        UpdateUI();
    }


    void OnDestroy()
    {
        if(ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnInventoryChanged -= UpdateUI;
        }
    }

    public void ShowInventory()
    {
        inventoryPanel.SetActive(true);
        UpdateUI();
    }

    private void StartCollection(string resource, Button btn)
    {
        btn.interactable = false;
        StartCoroutine(CollectCoroutine(resource, btn));
    }

    IEnumerator CollectCoroutine(string resource, Button btn)
    {
        yield return new WaitForSeconds(2f);
        ProfileManager.Instance.AddResource(resource, 1);
        UpdateUI();
        btn.interactable = true;
        Debug.Log("Collected 1 " + resource);
    }
  
    void UpdateUI()
    {
        if (ProfileManager.Instance != null && ProfileManager.Instance.currentPlayer != null)
        {
            var p = ProfileManager.Instance.currentPlayer;
            coinsText.text = "Coins: " + p.coins;
            woodText.text = "Wood: " + p.inventory.wood;
            stoneText.text = "Stone: " + p.inventory.stone;
            goldText.text = "Gold: " + p.inventory.gold;
        }
        Debug.Log("ui changed is called");
    }

    public void OnCreateListingButtonClicked()
    {
        createListingUI.ShowPopup();
    }
}