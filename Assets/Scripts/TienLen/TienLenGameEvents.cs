using System;
using System.Collections.Generic;

public static class TienLenGameEvents
{
    public static Action<List<PlayingCardData>, int> OnCardsPlayed;
    public static Action<int> OnPlayerPassed;
    public static Action<int> OnTurnChanged;
    public static Action<string, int> OnSpecialEffect;
    public static Action<string> OnGameOver;
    public static Action<string> OnToast;

    public static void CardsPlayed(List<PlayingCardData> cards, int playerIndex)
    {
        OnCardsPlayed?.Invoke(cards, playerIndex);
    }

    public static void PlayerPassed(int playerIndex)
    {
        OnPlayerPassed?.Invoke(playerIndex);
    }

    public static void TurnChanged(int playerIndex)
    {
        OnTurnChanged?.Invoke(playerIndex);
    }

    public static void SpecialEffect(string effectName, int playerIndex)
    {
        OnSpecialEffect?.Invoke(effectName, playerIndex);
    }

    public static void GameOver(string winnerName)
    {
        OnGameOver?.Invoke(winnerName);
    }

    public static void Toast(string message)
    {
        OnToast?.Invoke(message);
    }
}
