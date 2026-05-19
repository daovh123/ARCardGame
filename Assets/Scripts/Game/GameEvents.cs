using System;

public static class GameEvents
{
    public static Action<CardData, int> OnCardPlayed;
    public static Action<int> OnCardDrawn;
    public static Action<int> OnTurnChanged;
    public static Action<string> OnGameOver;
    public static Action<CardData, int> OnSpecialCardPlayed;

    public static void CardPlayed(CardData card, int playerIndex)
    {
        OnCardPlayed?.Invoke(card, playerIndex);

        if (card.type != CardType.Number)
        {
            OnSpecialCardPlayed?.Invoke(card, playerIndex);
        }
    }

    public static void CardDrawn(int playerIndex)
    {
        OnCardDrawn?.Invoke(playerIndex);
    }

    public static void TurnChanged(int currentPlayerIndex)
    {
        OnTurnChanged?.Invoke(currentPlayerIndex);
    }

    public static void GameOver(string winnerName)
    {
        OnGameOver?.Invoke(winnerName);
    }
}