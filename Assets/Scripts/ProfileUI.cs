using UnityEngine;
using TMPro;

public class ProfileUI : MonoBehaviour
{
    public GameObject profilePanel;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI tradeCountText;
    public TextMeshProUGUI achievementsText;

    private void OnEnable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnInventoryChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void ShowProfile()
    {
        profilePanel.SetActive(true);
        RefreshUI();
    }

    public void HideProfile()
    {
        profilePanel.SetActive(false);
    }

    private void RefreshUI()
    {
        var pm = ProfileManager.Instance;
        if (pm == null || pm.currentPlayer == null) return;

        var p = pm.currentPlayer;

        usernameText.text = "User: " + p.username;
        coinsText.text = "Coins: " + p.coins;
        tradeCountText.text = "Trades: " + p.tradeCount;

        if (p.achievements == null || p.achievements.Length == 0)
            achievementsText.text = "Achievements: None yet";
        else
            achievementsText.text = "Achievements: " + string.Join(", ", p.achievements);
    }
}
