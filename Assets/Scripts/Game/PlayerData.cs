using UnityEngine;
using System.Collections.Generic;

public class PlayerData
{
    public int playerIndex;
    public string playerName;
    public List<CardData> handCards = new List<CardData>();

    public PlayerData(int playerIndex, string playerName)
    {
        this.playerIndex = playerIndex;
        this.playerName = playerName;
    }

    public int CardCount
    {
        get { return handCards.Count; }
    }
}
