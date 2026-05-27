using UnityEngine;

public class UnoARGameEventBridge : MonoBehaviour
{
    public ARTableController arTableController;

    private void OnEnable()
    {
        GameEvents.OnCardPlayed += HandleCardPlayed;
        GameEvents.OnCardDrawn += HandleCardDrawn;
        GameEvents.OnTurnChanged += HandleTurnChanged;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnSpecialCardPlayed += HandleSpecialCardPlayed;
    }

    private void OnDisable()
    {
        GameEvents.OnCardPlayed -= HandleCardPlayed;
        GameEvents.OnCardDrawn -= HandleCardDrawn;
        GameEvents.OnTurnChanged -= HandleTurnChanged;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnSpecialCardPlayed -= HandleSpecialCardPlayed;
    }

    private void HandleCardPlayed(CardData card, int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowPlayedUnoCard(card, playerIndex);
        }
    }

    private void HandleCardDrawn(int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowDrawEffect(playerIndex, 1);
        }
    }

    private void HandleTurnChanged(int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowTurn(playerIndex);
        }
    }

    private void HandleGameOver(string winnerName)
    {
        if (arTableController != null)
        {
            arTableController.ShowWinner(winnerName);
        }
    }

    private void HandleSpecialCardPlayed(CardData card, int playerIndex)
    {
        if (arTableController != null)
        {
            arTableController.ShowUnoSpecialEffect(card, playerIndex);
        }
    }
}
