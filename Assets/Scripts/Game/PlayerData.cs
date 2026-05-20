using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int playerIndex;
    public string playerName;
    public List<CardData> handCards = new List<CardData>();
    public bool hasFinished;
    public bool isEliminated;
    public bool isLastPlace;
    public int finishRank;

    public PlayerData()
    {
    }

    public PlayerData(int playerIndex, string playerName)
    {
        this.playerIndex = playerIndex;
        this.playerName = playerName;
    }

    public int CardCount
    {
        get { return handCards.Count; }
    }

    public bool IsActive
    {
        get { return !hasFinished && !isEliminated && !isLastPlace; }
    }
}
