using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Texts")]
    public TMP_Text connectionStatusText;
    public TMP_Text roomCodeText;
    public TMP_Text playerListText;
    public TMP_Text messageText;

    [Header("Input")]
    public TMP_InputField roomCodeInput;
    public TMP_InputField playerNameInput;

    [Header("Buttons")]
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button readyButton;
    public Button startGameButton;
    public Button backButton;

    private bool isReady = false;

    private void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backButton.onClick.AddListener(OnBackClicked);

        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        readyButton.interactable = false;
        startGameButton.interactable = false;

        roomCodeText.text = "Room Code: ----";
        playerListText.text = "Players:\nNot in room";
        messageText.text = "Connecting to Photon...";

        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        connectionStatusText.text = "Connected";
        messageText.text = "Connected to Photon server.";

        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        messageText.text = "Joined Photon Lobby.";
    }

    private void OnCreateRoomClicked()
    {
        SetPlayerName();

        string roomCode = Random.Range(1000, 9999).ToString();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(roomCode, roomOptions);

        messageText.text = "Creating room " + roomCode + "...";
    }

    private void OnJoinRoomClicked()
    {
        SetPlayerName();

        string roomCode = roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            messageText.text = "Please enter room code.";
            return;
        }

        PhotonNetwork.JoinRoom(roomCode);
        messageText.text = "Joining room " + roomCode + "...";
    }

    public override void OnJoinedRoom()
    {
        isReady = false;

        roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        messageText.text = "Joined room.";

        readyButton.interactable = true;
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        SetReadyProperty(false);
        RefreshPlayerList();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        messageText.text = "Create room failed: " + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        messageText.text = "Join room failed: " + message;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        messageText.text = newPlayer.NickName + " joined.";
        RefreshPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        messageText.text = otherPlayer.NickName + " left.";
        RefreshPlayerList();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshPlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
        RefreshPlayerList();
    }

    private void OnReadyClicked()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        isReady = !isReady;
        SetReadyProperty(isReady);

        RefreshPlayerList();
    }

    private void SetReadyProperty(bool ready)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["ready"] = ready;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void OnStartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            messageText.text = "Only host can start game.";
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            messageText.text = "Need at least 2 players.";
            return;
        }

        if (!AllPlayersReady())
        {
            messageText.text = "Not all players are ready.";
            return;
        }

        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        readyButton.interactable = false;
        startGameButton.interactable = false;
        backButton.interactable = false;

        PhotonNetwork.LoadLevel("GameScene");
    }

    private bool AllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("ready"))
            {
                return false;
            }

            bool ready = (bool)player.CustomProperties["ready"];

            if (!ready)
            {
                return false;
            }
        }

        return true;
    }

    private void RefreshPlayerList()
    {
        if (!PhotonNetwork.InRoom)
        {
            playerListText.text = "Players:\nNot in room";
            return;
        }

        string list = "Players:\n";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            bool ready = false;

            if (player.CustomProperties.ContainsKey("ready"))
            {
                ready = (bool)player.CustomProperties["ready"];
            }

            string hostMark = player.IsMasterClient ? "Host - " : "";
            string readyText = ready ? "Ready" : "Not Ready";

            list += hostMark + player.NickName + " - " + readyText + "\n";
        }

        playerListText.text = list;

        TMP_Text readyButtonText = readyButton.GetComponentInChildren<TMP_Text>();
        if (readyButtonText != null)
        {
            readyButtonText.text = isReady ? "Unready" : "Ready";
        }
    }

    private void SetPlayerName()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player " + Random.Range(100, 999);
        }

        PhotonNetwork.NickName = playerName;
    }

    private void OnBackClicked()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadScene("MainMenuScene");
    }
}