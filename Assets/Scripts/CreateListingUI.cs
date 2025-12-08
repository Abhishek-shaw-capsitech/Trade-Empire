using UnityEngine;
using UnityEngine.UI;
using TMPro;  

public class CreateListingUI : MonoBehaviour
{
    public TMP_Dropdown itemDropdown; 
    public TMP_InputField quantityInput, priceInput;
    public GameObject popup;

    public void ShowPopup()
    {
        popup.SetActive(true);
    }

    public void HidePopup()
    {
        popup.SetActive(false);
    }

    public void OnCreateListing()
    {
        string item = itemDropdown.options[itemDropdown.value].text.ToLower();
        if (int.TryParse(quantityInput.text, out int qty) && int.TryParse(priceInput.text, out int prc) && qty > 0 && prc > 0)
        {
            MarketplaceManager.Instance.CreateListing(item, qty, prc);
            HidePopup();
            quantityInput.text = ""; priceInput.text = "";
        }
        else
        {
            Debug.LogError("Invalid quantity or price");
        }
    }
}