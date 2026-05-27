using UnityEngine;

[RequireComponent(typeof(ARTableController))]
public class ARGameEventBridge : MonoBehaviour
{
    private ARTableController tableController;

    private void Awake()
    {
        tableController = GetComponent<ARTableController>();
    }

    private void OnEnable()
    {
        GameEvents.OnCardPlayed += HandleCardPlayed;
        GameEvents.OnCardDrawn += HandleCardDrawn;
        GameEvents.OnTurnChanged += HandleTurnChanged;
        GameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnCardPlayed -= HandleCardPlayed;
        GameEvents.OnCardDrawn -= HandleCardDrawn;
        GameEvents.OnTurnChanged -= HandleTurnChanged;
        GameEvents.OnGameOver -= HandleGameOver;
    }

    private void HandleCardPlayed(CardData card, int playerIndex)
    {
        if (tableController != null)
        {
            tableController.ShowPlayedUnoCard(card, playerIndex);
        }
    }

    private void HandleCardDrawn(int playerIndex)
    {
        if (tableController != null)
        {
            tableController.ShowDrawEffect(playerIndex, 1);
        }
    }

    private void HandleTurnChanged(int currentPlayerIndex)
    {
        if (tableController != null)
        {
            tableController.ShowTurn(currentPlayerIndex);
        }
    }

    private void HandleGameOver(string winnerName)
    {
        if (tableController != null)
        {
            tableController.ShowWinner(winnerName);
        }
    }
}
