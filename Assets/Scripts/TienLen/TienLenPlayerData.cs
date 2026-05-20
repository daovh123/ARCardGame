using System;
using System.Collections.Generic;

[Serializable]
public class TienLenPlayerData
{
    public int playerIndex;
    public string playerName;
    public List<PlayingCardData> handCards = new List<PlayingCardData>();
    public bool hasFinished;
    public int finishRank;

    public TienLenPlayerData()
    {
    }

    public TienLenPlayerData(int playerIndex, string playerName)
    {
        this.playerIndex = playerIndex;
        this.playerName = playerName;
    }

    public bool IsActive
    {
        get { return !hasFinished; }
    }
}
