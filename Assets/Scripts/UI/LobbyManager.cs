using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text roomCodeText;
    public TMP_Text playerListText;

    public TMP_InputField roomCodeInput;

    public Button createRoomButton;
    public Button joinRoomButton;
    public Button readyButton;
    public Button startGameButton;
    public Button backButton;

    private string roomCode = "----";
    private bool isReady = false;

    private List<string> players = new List<string>();

    private void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backButton.onClick.AddListener(OnBackClicked);

        startGameButton.interactable = false;

        RefreshLobbyUI();
    }

    private void OnCreateRoomClicked()
    {
        roomCode = Random.Range(1000, 9999).ToString();

        players.Clear();
        players.Add("You");

        isReady = false;
        startGameButton.interactable = true;

        Debug.Log("Created room: " + roomCode);

        RefreshLobbyUI();
    }

    private void OnJoinRoomClicked()
    {
        string inputCode = roomCodeInput.text;

        if (string.IsNullOrEmpty(inputCode))
        {
            Debug.Log("Please enter room code.");
            return;
        }

        roomCode = inputCode;

        players.Clear();
        players.Add("You");
        players.Add("Player 2");
        players.Add("Player 3");

        isReady = false;
        startGameButton.interactable = false;

        Debug.Log("Joined room: " + roomCode);

        RefreshLobbyUI();
    }

    private void OnReadyClicked()
    {
        if (players.Count == 0)
        {
            Debug.Log("Create or join a room first.");
            return;
        }

        isReady = !isReady;

        Debug.Log("Ready state: " + isReady);

        RefreshLobbyUI();
    }

    private void OnStartGameClicked()
    {
        if (players.Count == 0)
        {
            Debug.Log("No players in room.");
            return;
        }

        Debug.Log("Start game.");
        SceneManager.LoadScene("GameScene");
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void RefreshLobbyUI()
    {
        roomCodeText.text = "Room Code: " + roomCode;

        string list = "Players:\n";

        if (players.Count == 0)
        {
            list += "No room created/joined";
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                string readyText = "";

                if (i == 0)
                {
                    readyText = isReady ? " - Ready" : " - Not Ready";
                }

                list += players[i] + readyText + "\n";
            }
        }

        playerListText.text = list;

        TMP_Text readyButtonText = readyButton.GetComponentInChildren<TMP_Text>();
        if (readyButtonText != null)
        {
            readyButtonText.text = isReady ? "Unready" : "Ready";
        }
    }
}