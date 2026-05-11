using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text topCardText;
    public TMP_Text currentTurnText;
    public TMP_Text messageText;
    public TMP_Text playerStatusText;
    public Transform handPanel;
    public GameObject cardUIPrefab;

    public Button drawButton;
    public Button restartButton;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public Button gameOverRestartButton;
    private void Start()
    {
        drawButton.onClick.AddListener(OnDrawButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);

        gameOverPanel.SetActive(false);

        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        CardData topCard = gameManager.GetTopDiscardCard();

        if (topCard != null)
        {
            topCardText.text = "Top Card: " + topCard.GetDisplayName();
        }

        currentTurnText.text = "Turn: " + gameManager.GetCurrentPlayerName();
        messageText.text = gameManager.GetLastMessage();

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();

        for (int i = 0; i < handCards.Count; i++)
        {
            GameObject cardObject = Instantiate(cardUIPrefab, handPanel);
            CardUI cardUI = cardObject.GetComponent<CardUI>();
            cardUI.Setup(handCards[i], i, OnCardClicked);
        }

        RefreshPlayerStatus();

        if (gameManager.IsGameOver())
        {
            gameOverPanel.SetActive(true);
            winnerText.text = gameManager.GetWinnerName() + " wins!";
        }
        else
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnCardClicked(int handIndex)
    {
        gameManager.PlayCard(handIndex);
        RefreshUI();
    }

    private void OnDrawButtonClicked()
    {
        gameManager.DrawCard();
        RefreshUI();
    }
    private void RefreshPlayerStatus()
    {
        List<PlayerData> players = gameManager.GetPlayers();
        int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();

        string status = "Players:\n";

        for (int i = 0; i < players.Count; i++)
        {
            string turnMark = i == currentPlayerIndex ? "> " : "  ";
            status += turnMark + players[i].playerName + ": " + players[i].handCards.Count + " cards\n";
        }

        playerStatusText.text = status;

        Debug.Log(status);
    }
        private void OnRestartButtonClicked()
    {
        gameManager.StartOfflineGame();
        RefreshUI();
    }
}