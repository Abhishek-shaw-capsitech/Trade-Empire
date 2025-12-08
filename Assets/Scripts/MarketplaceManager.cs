using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class MarketplaceManager : MonoBehaviour
{
    public static MarketplaceManager Instance;
    private DatabaseReference marketRef;
    private const string DB_URL = "https://trade-empire-game-default-rtdb.firebaseio.com/";

    public List<Listing> availableListings = new List<Listing>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }


    }

    void Start()
    {
        var db = FirebaseDatabase.GetInstance(DB_URL);
        marketRef = db.RootReference.Child("marketplace");
        ListenToMarketplace();
    }

    private void ListenToMarketplace()
    {
        marketRef.OrderByChild("status").EqualTo("available").ValueChanged += HandleMarketValueChanged;
    }


    private void HandleMarketValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Marketplace listener error: " + args.DatabaseError.Message);
            return;
        }

        var snapshot = args.Snapshot;

        availableListings.Clear();

        if (snapshot == null || snapshot.Value == null)
        {
            UpdateMarketplaceUI();
            return;
        }

        foreach (var child in snapshot.Children)
        {
            var json = child.GetRawJsonValue();
            var listing = JsonUtility.FromJson<Listing>(json);

            listing.key = child.Key;

            if (listing.status == "available")
            {
                availableListings.Add(listing);
            }
        }

        UpdateMarketplaceUI();
    }


    private void UpdateMarketplaceUI()
    {
        Debug.Log("Market updated: " + availableListings.Count + " listings");

        if (MarketplaceUI.Instance != null)
        {
            MarketplaceUI.Instance.RefreshListings();
        }
    }

    public async void CreateListing(string itemName, int quantity, int price)
    {
        if (marketRef == null)
        {
            var db = FirebaseDatabase.GetInstance(DB_URL);
            marketRef = db.RootReference.Child("marketplace");
        }

        if (!ProfileManager.Instance.HasResource(itemName, quantity))
        {
            Debug.LogError("Not enough " + itemName);
            return;
        }

        ProfileManager.Instance.RemoveResource(itemName, quantity);

        var listing = new Listing
        {
            itemName = itemName,
            sellerId = ProfileManager.Instance.userId,
            quantity = quantity,
            price = price,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            status = "available",
            buyerId = null
        };

        string key = marketRef.Push().Key;
        await marketRef.Child(key).SetRawJsonValueAsync(JsonUtility.ToJson(listing));
        Debug.Log("Listing created: " + itemName);
    }

    public async Task PurchaseListing(Listing listing)
    {
        if (listing.status != "available")
        {
            Debug.LogError("Cannot purchase: listing not available");
            return;
        }

        if (ProfileManager.Instance.currentPlayer.coins < listing.price)
        {
            Debug.LogError("Cannot purchase: insufficient coins");
            return;
        }

        if (string.IsNullOrEmpty(listing.key))
        {
            Debug.LogError("Listing has no Firebase key set!");
            return;
        }

        var db = FirebaseDatabase.GetInstance(DB_URL);
        var listingRef = db.RootReference.Child("marketplace").Child(listing.key);

        bool listingSold = false;

        var txnTask = listingRef.RunTransaction(mutable =>
        {
            if (mutable.Value == null)
            {
                return TransactionResult.Abort();
            }

            var dict = mutable.Value as Dictionary<string, object>;
            if (dict == null)
            {
                return TransactionResult.Abort();
            }

            object statusObj;
            if (!dict.TryGetValue("status", out statusObj))
            {
                return TransactionResult.Abort();
            }

            var currentStatus = statusObj.ToString();
            if (currentStatus != "available")
            {
                return TransactionResult.Abort();
            }

            dict["status"] = "sold";
            dict["buyerId"] = ProfileManager.Instance.userId;

            mutable.Value = dict;

            listingSold = true;

            return TransactionResult.Success(mutable);
        });

        await txnTask;

        if (txnTask.Exception != null)
        {
            Debug.LogError("Purchase transaction error: " + txnTask.Exception);
            return;
        }

        if (!listingSold)
        {
            Debug.LogError("Purchase failed: listing already sold or removed");
            return;
        }

        ProfileManager.Instance.AddCoins(-listing.price);
        ProfileManager.Instance.AddResource(listing.itemName, listing.quantity);
        ProfileManager.Instance.SaveProfile();

        var sellerCoinsRef = db.RootReference
            .Child("players")
            .Child(listing.sellerId)
            .Child("coins");

        var sellerTxn = sellerCoinsRef.RunTransaction(mutable =>
        {
            long currentCoins = 0;

            if (mutable.Value != null)
            {
                try
                {
                    currentCoins = Convert.ToInt64(mutable.Value);
                }
                catch
                {
                    currentCoins = 0;
                }
            }

            mutable.Value = currentCoins + listing.price;
            return TransactionResult.Success(mutable);
        });

        await sellerTxn;


        if (sellerTxn.Exception != null)
        {
            Debug.LogError("Failed to credit seller coins: " + sellerTxn.Exception);
            return;
        }

        var pm = ProfileManager.Instance;
        if (pm != null && pm.currentPlayer != null)
        {
            pm.currentPlayer.tradeCount++;

            pm.UnlockAchievement("First Purchase");

            if (pm.currentPlayer.tradeCount >= 5)
            {
                pm.UnlockAchievement("Trader Level 1");
            }

            pm.SaveProfile();
        }

        var sellerProfileRef = db.RootReference
            .Child("players")
            .Child(listing.sellerId);

        var sellerSnapshot = await sellerProfileRef.GetValueAsync();
        if (sellerSnapshot.Exists)
        {
            var sellerData = JsonUtility.FromJson<PlayerData>(sellerSnapshot.GetRawJsonValue());
            if (sellerData != null)
            {
                sellerData.tradeCount++;

                if (sellerData.achievements == null)
                {
                    sellerData.achievements = new string[0];
                }

                var achList = new System.Collections.Generic.List<string>(sellerData.achievements);

                if (!achList.Contains("First Sale"))
                {
                    achList.Add("First Sale");
                }

                if (sellerData.tradeCount >= 5 && !achList.Contains("Trader Level 1"))
                {
                    achList.Add("Trader Level 1");
                }

                sellerData.achievements = achList.ToArray();

                await sellerProfileRef.SetRawJsonValueAsync(JsonUtility.ToJson(sellerData));
            }
        }


        Debug.Log("Purchase successful! Waiting for listener to refresh UI.");



    }
}





