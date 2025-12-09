using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using System;

public class ProfileManager : MonoBehaviour
{
    public PlayerData currentPlayer = new PlayerData();
    public string userId;
    private DatabaseReference dbRef;

    public static ProfileManager Instance;
    public event Action OnInventoryChanged;
    private const string DB_URL = "https://trade-empire-game-default-rtdb.firebaseio.com/";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void Initialize(string uid)
    {
        userId = uid;

        if (dbRef != null)
        {
            dbRef.ValueChanged -= HandleProfileChanged;
        }

        dbRef = FirebaseDatabase.GetInstance(DB_URL)
            .RootReference
            .Child("players")
            .Child(uid);

        dbRef.ValueChanged += HandleProfileChanged;

        LoadProfile();
    }

    private async void LoadProfile()
    {
        var snapshot = await dbRef.GetValueAsync();
        if (snapshot.Exists)
        {
            currentPlayer = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
        }
        else
        {
            currentPlayer.username = "Default";
            currentPlayer.coins = 100;
            currentPlayer.tradeCount = 0;
            currentPlayer.achievements = new string[0];
            currentPlayer.inventory = new PlayerData.Inventory();
        }

        SaveProfile();

        RaiseInventoryChanged();

        Debug.Log("Profile loaded: " + currentPlayer.username + " has " + currentPlayer.coins + " coins");
    }

    private void HandleProfileChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Profile listener error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists)
        {
            Debug.LogWarning("Profile snapshot missing for user: " + userId);
            return;
        }

        currentPlayer = JsonUtility.FromJson<PlayerData>(args.Snapshot.GetRawJsonValue());

        Debug.Log("Profile changed from Firebase. Coins=" + currentPlayer.coins + ", trades=" + currentPlayer.tradeCount);

        RaiseInventoryChanged();
    }

    private void OnDestroy()
    {
        if (dbRef != null)
        {
            dbRef.ValueChanged -= HandleProfileChanged;
        }
    }

    public async void SaveProfile()
    {
        if (dbRef != null)
        {
            await dbRef.SetRawJsonValueAsync(JsonUtility.ToJson(currentPlayer));
        }
    }

    public void AddCoins(int amount)
    {
        currentPlayer.coins += amount;
        SaveProfile();
        RaiseInventoryChanged();
    }

    public void AddResource(string resource, int amount)
    {
        switch (resource)
        {
            case "wood": currentPlayer.inventory.wood += amount; break;
            case "stone": currentPlayer.inventory.stone += amount; break;
            case "gold": currentPlayer.inventory.gold += amount; break;
        }
        SaveProfile();
        RaiseInventoryChanged();
    }

    public bool HasResource(string resource, int amount)
    {
        switch (resource)
        {
            case "wood": return currentPlayer.inventory.wood >= amount;
            case "stone": return currentPlayer.inventory.stone >= amount;
            case "gold": return currentPlayer.inventory.gold >= amount;
            default: return false;
        }
    }

    public void RemoveResource(string resource, int amount)
    {
        switch (resource)
        {
            case "wood": currentPlayer.inventory.wood -= amount; break;
            case "stone": currentPlayer.inventory.stone -= amount; break;
            case "gold": currentPlayer.inventory.gold -= amount; break;
        }
        SaveProfile();
        RaiseInventoryChanged();
    }

    private void RaiseInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public void UnlockAchievement(string ach)
    {
        if (currentPlayer == null) return;

        if (currentPlayer.achievements == null)
            currentPlayer.achievements = new string[0];

        if (Array.IndexOf(currentPlayer.achievements, ach) != -1)
            return;

        var list = new System.Collections.Generic.List<string>(currentPlayer.achievements);
        list.Add(ach);
        currentPlayer.achievements = list.ToArray();

        SaveProfile();
        Debug.Log("Unlocked achievement: " + ach);
    }
}
