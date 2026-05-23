using UnityEngine;
using UnityEngine.InputSystem;

public class ARVisualTestHelper : MonoBehaviour
{
    private int currentPlayerIndex = 0;
    private int cardIdCounter = 1000;

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Press 0, 1, 2, 3 to trigger play card for that player index
        if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame) TriggerPlayCard(0);
        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) TriggerPlayCard(1);
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) TriggerPlayCard(2);
        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) TriggerPlayCard(3);

        // Press D to trigger draw card for the current player
        if (keyboard.dKey.wasPressedThisFrame)
        {
            Debug.Log($"[Mock Test] Triggering CardDrawn for Player {currentPlayerIndex + 1}");
            GameEvents.CardDrawn(currentPlayerIndex);
        }

        // Press T to switch turns to the next player
        if (keyboard.tKey.wasPressedThisFrame)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % 4;
            Debug.Log($"[Mock Test] Triggering TurnChanged to Player {currentPlayerIndex + 1}");
            GameEvents.TurnChanged(currentPlayerIndex);
        }

        // Press W to trigger game over with a victory celebration
        if (keyboard.wKey.wasPressedThisFrame)
        {
            string[] names = { "Antigravity", "AlphaCoder", "MasterChief", "DoomSlayer" };
            string randomWinner = names[Random.Range(0, names.Length)];
            Debug.Log($"[Mock Test] Triggering GameOver. Winner: {randomWinner}");
            GameEvents.GameOver(randomWinner);
        }
    }

    private void TriggerPlayCard(int playerIndex)
    {
        CardData randomCard = GenerateRandomCard();
        Debug.Log($"[Mock Test] Triggering CardPlayed: {randomCard.GetDisplayName()} by Player {playerIndex + 1}");
        GameEvents.CardPlayed(randomCard, playerIndex);
    }

    private CardData GenerateRandomCard()
    {
        cardIdCounter++;
        CardColor color = (CardColor)Random.Range(0, 4); // Red, Blue, Green, Yellow
        CardType type = (CardType)Random.Range(0, 6); // Number, Reverse, DrawTwo, Block, DrawFour, ChangeColor
        
        int number = -1;
        if (type == CardType.Number)
        {
            number = Random.Range(0, 10); // 0-9
        }
        else if (type == CardType.ChangeColor || type == CardType.DrawFour)
        {
            color = CardColor.Wild;
        }

        return new CardData(cardIdCounter, color, type, number);
    }
}
