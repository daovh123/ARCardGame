using UnityEngine;

public class GameEventDebugListener : MonoBehaviour
{
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
        Debug.Log("[EVENT] Player " + (playerIndex + 1) + " played " + card.GetDisplayName());
    }

    private void HandleCardDrawn(int playerIndex)
    {
        Debug.Log("[EVENT] Player " + (playerIndex + 1) + " drew a card");
    }

    private void HandleTurnChanged(int currentPlayerIndex)
    {
        Debug.Log("[EVENT] Turn changed to Player " + (currentPlayerIndex + 1));
    }

    private void HandleGameOver(string winnerName)
    {
        Debug.Log("[EVENT] Game over. Winner: " + winnerName);
    }

    private void HandleSpecialCardPlayed(CardData card, int playerIndex)
    {
        Debug.Log("[EVENT] Special card effect: " + card.GetDisplayName() + " by Player " + (playerIndex + 1));
    }
}