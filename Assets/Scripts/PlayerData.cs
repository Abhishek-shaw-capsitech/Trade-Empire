using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string username;
    public int coins;
    public int tradeCount; 
    public string[] achievements;
    public Inventory inventory;

    [Serializable]
    public class Inventory
    {
        public int wood;
        public int stone;
        public int gold;
    }
}