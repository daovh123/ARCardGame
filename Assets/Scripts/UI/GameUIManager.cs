using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Button backMenuButton;

    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public Button gameOverRestartButton;

    private void Start()
    {
        drawButton.onClick.AddListener(OnDrawButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        backMenuButton.onClick.AddListener(OnBackMenuClicked);

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

        bool isLocalTurn = gameManager.IsLocalPlayerTurn();

        drawButton.interactable = isLocalTurn && !gameManager.IsGameOver();

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();

        for (int i = 0; i < handCards.Count; i++)
        {
            GameObject cardObject = Instantiate(cardUIPrefab, handPanel);

            CardUI cardUI = cardObject.GetComponent<CardUI>();
            cardUI.Setup(handCards[i], i, OnCardClicked);

            Button cardButton = cardObject.GetComponent<Button>();

            if (cardButton != null)
            {
                cardButton.interactable = isLocalTurn && !gameManager.IsGameOver();
            }
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
        if (!gameManager.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn.");
            return;
        }

        gameManager.PlayCard(handIndex);
        RefreshUI();
    }

    private void OnDrawButtonClicked()
    {
        if (!gameManager.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn.");
            return;
        }

        gameManager.DrawCard();
        RefreshUI();
    }

    private void OnRestartButtonClicked()
    {
        gameManager.StartOfflineGame();
        RefreshUI();
    }

    private void OnBackMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void RefreshPlayerStatus()
    {
        List<PlayerData> players = gameManager.GetPlayers();
        int currentIndex = gameManager.GetCurrentPlayerIndex();

        string status = "Players:\n";

        for (int i = 0; i < players.Count; i++)
        {
            string turnMark = i == currentIndex ? "> " : "  ";
            status += turnMark + players[i].playerName + ": " + players[i].handCards.Count + " cards\n";
        }

        playerStatusText.text = status;
    }
}