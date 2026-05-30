using System;
using System.Collections.Generic;

[Serializable]
public class GameStateData
{
    public List<PlayerData> players = new List<PlayerData>();
    public List<CardData> deckCards = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    public CardData topDiscardCard;
    public CardColor currentColor;

    public int currentPlayerIndex;
    public int direction;
    public bool hasDrawnThisTurn;
    public int drawnCardIndex;
    public int pendingDrawPenalty;
    public int unoDeclaredPlayerIndex;
    public int finishOrder;

    public bool isGameOver;
    public string lastMessage;
    public string winnerName;

    public int visualEventSequence;
    public string visualEventType;
    public int visualEventPlayerIndex;
    public CardData visualEventCard;
    public string visualEventWinner;
}
