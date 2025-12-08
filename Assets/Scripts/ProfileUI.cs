using UnityEngine;
using TMPro;

public class ProfileUI : MonoBehaviour
{
    public GameObject profilePanel;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI tradeCountText;
    public TextMeshProUGUI achievementsText;
    public void ShowProfile()
    {
        profilePanel.SetActive(true);

        var p = ProfileManager.Instance.currentPlayer;
        if (p == null) return;

        usernameText.text    = "User: " + p.username;
        coinsText.text       = "Coins: " + p.coins;
        tradeCountText.text  = "Trades: " + p.tradeCount;      
        if (p.achievements == null || p.achievements.Length == 0)
            achievementsText.text = "Achievements: None yet";
        else
            achievementsText.text = "Achievements: " + string.Join(", ", p.achievements);
    }

    public void HideProfile()
    {
        profilePanel.SetActive(false);
    }
}
