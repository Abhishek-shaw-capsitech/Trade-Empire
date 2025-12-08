using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using TMPro;
using Firebase;


public class AuthManager : MonoBehaviour
{
    private const string DB_URL = "https://trade-empire-game-default-rtdb.firebaseio.com/";
    public TMP_InputField emailInput, passwordInput;
    public TextMeshProUGUI statusText;
    public GameObject loginPanel;

    private FirebaseAuth auth;
    private string userId;
    private FirebaseDatabase database;
    [SerializeField] private InventoryUI inventoryUI;
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.GetInstance(DB_URL);
        auth.StateChanged += OnAuthStateChanged;
        CheckAuthState();
    }
    void OnDestroy()
    {
        auth.StateChanged -= OnAuthStateChanged;
    }
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            statusText.text = "Logged in as: " + auth.CurrentUser.DisplayName ?? "Guest";
            loginPanel.SetActive(false);

            ProfileManager.Instance.Initialize(userId);
            inventoryUI.ShowInventory();
        }
        else
        {
            userId = null;
            loginPanel.SetActive(true);
        }
    }
    private void CheckAuthState()
    {
        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
        }
    }

    public async void OnGuestLogin()
    {
        statusText.text = "Signing in as guest...";
        Debug.Log("Guest login button pressed");

        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            var user = result.User;

            Debug.Log("Guest login successful, uid = " + user.UserId);

            await CreateProfileIfNew(user.UserId, "Guest" + UnityEngine.Random.Range(1000, 9999));

            ProfileManager.Instance.Initialize(user.UserId);
            inventoryUI.ShowInventory();

            statusText.text = "Guest login successful!";
        }
        catch (Exception ex)
        {
            Debug.LogError("Guest login failed: " + ex);
            statusText.text = "Guest login failed: " + ex.Message;
        }
    }

    public async void OnEmailLogin()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Enter email and password!";
            return;
        }

        statusText.text = "Signing in...";

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            var user = result.User;

            await CreateProfileIfNew(user.UserId, email.Split('@')[0]);

            statusText.text = "Email login successful!";

            ProfileManager.Instance.Initialize(user.UserId);
            inventoryUI.ShowInventory();

        }
        catch (Exception ex)
        {
            statusText.text = "Login failed: " + ex.Message;
            Debug.LogError(ex);
        }
    }

    public async void OnSignup()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || password.Length < 6)
        {
            statusText.text = "Invalid email or password too short!";
            return;
        }

        statusText.text = "Creating account...";

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            var user = result.User;

            await CreateProfileIfNew(user.UserId, email.Split('@')[0]);

            statusText.text = "Signup successful! Logged in.";
            Debug.Log("Signup successful, uid = " + user.UserId);
        }
        catch (Exception ex)
        {
            statusText.text = "Signup failed: " + ex.Message;
            Debug.LogError("Signup failed: " + ex);
        }
    }


    private async Task CreateProfileIfNew(string uid, string username)
{
    var db = database.RootReference.Child("players").Child(uid);
    var snapshot = await db.GetValueAsync();
    if (snapshot.Value == null)
    {
        var profile = new PlayerData
        {
            username = username,
            coins = 100,
            tradeCount = 0,
            achievements = new string[0],
            inventory = new PlayerData.Inventory
            {
                wood = 0,
                stone = 0,
                gold = 0
            }
        };

        string json = JsonUtility.ToJson(profile);
        await db.SetRawJsonValueAsync(json);
    }
}


    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.userId = null;
            ProfileManager.Instance.currentPlayer = new PlayerData();
        }

        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            inventoryUI.inventoryPanel.SetActive(false);
        }

        if (MarketplaceUI.Instance != null && MarketplaceUI.Instance.marketplacePanel != null)
        {
            MarketplaceUI.Instance.marketplacePanel.SetActive(false);
        }

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "Logged out.";
        }

        Debug.Log("User logged out");
    }


}
