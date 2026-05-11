using System;
using System.Collections.Generic;

[Serializable]
public class GameStateData
{
    public List<PlayerData> players = new List<PlayerData>();
    public List<CardData> deckCards = new List<CardData>();

    public CardData topDiscardCard;

    public int currentPlayerIndex;
    public int direction;

    public bool isGameOver;
    public string lastMessage;
    public string winnerName;
}