using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    private const string PlayerNameKey = "PlayerName";
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
    private bool themeBuilt;

    private void Start()
    {
        BuildRuntimeTheme();
        LoadSavedPlayerName();
        playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
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

    private void LoadSavedPlayerName()
    {
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            playerNameInput.text = PlayerPrefs.GetString(PlayerNameKey);
        }
    }

    private void OnPlayerNameChanged(string value)
    {
        PlayerPrefs.SetString(PlayerNameKey, value);
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
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
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
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        SetPlayerName();

        string roomCode = roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            messageText.text = "Please enter room code.";
            return;
        }

        PhotonNetwork.JoinRoom(roomCode);
        messageText.text = "Joining room " + roomCode + "...";
    }
    public void CopyRoomCodeToClipboard()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            messageText.text = "No room code available.";
            return;
        }

        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        messageText.text = "Room code copied to clipboard!";
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
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
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
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            messageText.text = "Only host can start game.";
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            messageText.text = "Need at least 2 players.";
            return;
        }

        if (!AllPlayersReady())
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            messageText.text = "Not all players are ready.";
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Special, 0.82f);
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
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadScene("MainMenuScene");
    }

    private void BuildRuntimeTheme()
    {
        if (themeBuilt)
        {
            return;
        }

        Canvas canvas = ResolveCanvas();
        if (canvas == null)
        {
            return;
        }

        RuntimeUITheme.ConfigureCanvas(canvas);
        BuildBackground(canvas.transform);
        StyleLobbyControls();
        themeBuilt = true;
    }

    private Canvas ResolveCanvas()
    {
        if (createRoomButton != null)
        {
            Canvas canvas = createRoomButton.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
        }

        return FindAnyObjectByType<Canvas>();
    }

    private void BuildBackground(Transform canvasTransform)
    {
        RectTransform background = RuntimeUITheme.CreateGradient(canvasTransform, "Runtime_LobbyBackground", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.13f, 0.14f, 1f));
        background.SetAsFirstSibling();
        RuntimeUITheme.SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform leftPanel = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_LobbyPlayersPanel", new Color(0.01f, 0.04f, 0.05f, 0.90f), new Color(0.18f, 0.95f, 0.86f, 0.36f), 26, 3);
        leftPanel.SetSiblingIndex(1);
        RuntimeUITheme.SetRect(leftPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, -20f), new Vector2(560f, 610f));
        RuntimeUITheme.AddShadow(leftPanel.gameObject, new Color(0f, 0f, 0f, 0.44f), new Vector2(0f, -8f));

        RectTransform rightPanel = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_LobbyActionPanel", new Color(0.01f, 0.04f, 0.05f, 0.90f), new Color(1f, 0.78f, 0.28f, 0.40f), 26, 3);
        rightPanel.SetSiblingIndex(2);
        RuntimeUITheme.SetRect(rightPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, -20f), new Vector2(560f, 610f));
        RuntimeUITheme.AddShadow(rightPanel.gameObject, new Color(0f, 0f, 0f, 0.44f), new Vector2(0f, -8f));

        TMP_Text title = ResolveLobbyTitle(canvasTransform);
        title.text = "Multiplayer Lobby";
        RuntimeUITheme.StyleText(title, 54, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        RuntimeUITheme.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(900f, 74f));
        RuntimeUITheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.60f), new Vector2(0f, -5f));
        title.transform.SetAsLastSibling();
    }

    private TMP_Text ResolveLobbyTitle(Transform canvasTransform)
    {
        Transform existingTitle = canvasTransform.Find("LobbyTitleText");
        if (existingTitle != null)
        {
            TMP_Text title = existingTitle.GetComponent<TMP_Text>();
            if (title != null)
            {
                return title;
            }
        }

        return RuntimeUITheme.CreateLabel(canvasTransform, "Runtime_LobbyTitle", "Multiplayer Lobby", 54, Color.white);
    }

    private void StyleLobbyControls()
    {
        RuntimeUITheme.StyleText(connectionStatusText, 23, new Color(0.70f, 0.96f, 1f, 1f), TextAlignmentOptions.Left, FontStyles.Bold);
        connectionStatusText.overflowMode = TextOverflowModes.Ellipsis;
        RuntimeUITheme.SetRect(connectionStatusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(184f, -40f), new Vector2(430f, 42f), new Vector2(0f, 0.5f));

        RuntimeUITheme.StyleText(roomCodeText, 35, new Color(1f, 0.82f, 0.34f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        RuntimeUITheme.SetRect(roomCodeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, 190f), new Vector2(480f, 52f));

        RuntimeUITheme.StyleText(playerListText, 26, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Bold);
        RuntimeUITheme.SetRect(playerListText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, -72f), new Vector2(460f, 420f));

        RuntimeUITheme.StyleText(messageText, 23, new Color(1f, 0.90f, 0.66f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        RuntimeUITheme.SetRect(messageText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(960f, 44f));

        RuntimeUITheme.StyleInput(playerNameInput, "Player name");
        RuntimeUITheme.SetRect(playerNameInput.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, 206f), new Vector2(400f, 62f));

        RuntimeUITheme.StyleInput(roomCodeInput, "Room code");
        RuntimeUITheme.SetRect(roomCodeInput.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, 124f), new Vector2(400f, 62f));

        RuntimeUITheme.StyleButton(createRoomButton, RuntimeUITheme.Gold, RuntimeUITheme.Ink, "Create Room");
        RuntimeUITheme.SetRect(createRoomButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, 34f), new Vector2(400f, 64f));

        RuntimeUITheme.StyleButton(joinRoomButton, RuntimeUITheme.Blue, Color.white, "Join Room");
        RuntimeUITheme.SetRect(joinRoomButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, -48f), new Vector2(400f, 64f));

        RuntimeUITheme.StyleButton(readyButton, RuntimeUITheme.Felt, Color.white, "Ready");
        RuntimeUITheme.SetRect(readyButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, -132f), new Vector2(400f, 64f));

        RuntimeUITheme.StyleButton(startGameButton, new Color(0.88f, 0.08f, 0.12f, 1f), Color.white, "Start Game");
        RuntimeUITheme.SetRect(startGameButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350f, -216f), new Vector2(400f, 64f));

        RuntimeUITheme.StyleButton(backButton, new Color(0.08f, 0.16f, 0.18f, 0.98f), Color.white, "Back");
        RuntimeUITheme.SetRect(backButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(94f, -40f), new Vector2(140f, 48f));

        createRoomButton.transform.SetAsLastSibling();
        joinRoomButton.transform.SetAsLastSibling();
        readyButton.transform.SetAsLastSibling();
        startGameButton.transform.SetAsLastSibling();
        backButton.transform.SetAsLastSibling();
    }
}
