using UnityEngine;

public class GameVisualEventBridge : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnCardPlayed += OnCardPlayed;
        GameEvents.OnSpecialCardPlayed += OnSpecialCardPlayed;
        GameEvents.OnCardDrawn += OnCardDrawn;
        GameEvents.OnTurnChanged += OnTurnChanged;
        GameEvents.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnCardPlayed -= OnCardPlayed;
        GameEvents.OnSpecialCardPlayed -= OnSpecialCardPlayed;
        GameEvents.OnCardDrawn -= OnCardDrawn;
        GameEvents.OnTurnChanged -= OnTurnChanged;
        GameEvents.OnGameOver -= OnGameOver;
    }

    private void OnCardPlayed(CardData card, int playerIndex)
    {
        Debug.Log("[VISUAL BRIDGE] Card Played: " + card.GetDisplayName() + " by Player " + (playerIndex + 1));
    }

    private void OnSpecialCardPlayed(CardData card, int playerIndex)
    {
        Debug.Log("[VISUAL BRIDGE] Special Effect: " + card.type + " by Player " + (playerIndex + 1));
    }

    private void OnCardDrawn(int playerIndex)
    {
        Debug.Log("[VISUAL BRIDGE] Card Drawn by Player " + (playerIndex + 1));
    }

    private void OnTurnChanged(int currentPlayerIndex)
    {
        Debug.Log("[VISUAL BRIDGE] Turn Changed to Player " + (currentPlayerIndex + 1));
    }

    private void OnGameOver(string winnerName)
    {
        Debug.Log("[VISUAL BRIDGE] Game Over: " + winnerName);
    }
}   