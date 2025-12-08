using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MarketplaceUI : MonoBehaviour
{
    public static MarketplaceUI Instance;

    [Header("Root")]
    public GameObject marketplacePanel;

    [Header("Listing UI")]
    public GameObject listingPrefab;
    public Transform contentParent;

    private Dictionary<string, GameObject> listingUIs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        marketplacePanel.SetActive(false);
    }

    public void ShowMarketplace()
    {
        marketplacePanel.SetActive(true);
        RefreshListings();
    }

    public void HideMarketplace()
    {
        marketplacePanel.SetActive(false);
    }

    public void RefreshListings()
    {
        foreach (var go in listingUIs.Values)
        {
            Destroy(go);
        }
        listingUIs.Clear();

        var listings = MarketplaceManager.Instance.availableListings;

        foreach (var listing in listings)
        {
            var inst = GameObject.Instantiate(listingPrefab, contentParent);


            var itemText = inst.transform.Find("ItemText").GetComponent<TMP_Text>();
            itemText.text = $"{listing.itemName} x{listing.quantity} - {listing.price} coins";

            Button buyBtn = inst.GetComponentInChildren<Button>();

            Listing captured = listing;
            buyBtn.onClick.AddListener(async () =>
            {
                await MarketplaceManager.Instance.PurchaseListing(captured);
            });

            listingUIs[listing.key] = inst;
        }
    }
}
