using System.Collections.Generic;
using UnityEngine;

public class TienLenARGameEventBridge : MonoBehaviour
{
    public ARTableController arTableController;

    private void OnEnable()
    {
        TienLenGameEvents.OnCardsPlayed += HandleCardsPlayed;
        TienLenGameEvents.OnPlayerPassed += HandlePlayerPassed;
        TienLenGameEvents.OnTurnChanged += HandleTurnChanged;
        TienLenGameEvents.OnSpecialEffect += HandleSpecialEffect;
        TienLenGameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        TienLenGameEvents.OnCardsPlayed -= HandleCardsPlayed;
        TienLenGameEvents.OnPlayerPassed -= HandlePlayerPassed;
        TienLenGameEvents.OnTurnChanged -= HandleTurnChanged;
        TienLenGameEvents.OnSpecialEffect -= HandleSpecialEffect;
        TienLenGameEvents.OnGameOver -= HandleGameOver;
    }

    private void HandleCardsPlayed(List<PlayingCardData> cards, int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowTienLenCards(cards, playerIndex);
        }
    }

    private void HandlePlayerPassed(int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowTienLenPass(playerIndex);
        }
    }

    private void HandleTurnChanged(int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowTurn(playerIndex);
        }
    }

    private void HandleSpecialEffect(string effectName, int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowTienLenSpecialEffect(effectName, playerIndex);
        }
    }

    private void HandleGameOver(string winnerName)
    {
        if (arTableController != null)
        {
            arTableController.ShowWinner(winnerName);
        }
    }
}
