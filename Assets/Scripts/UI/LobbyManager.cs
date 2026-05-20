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
    private bool themeBuilt;

    private List<string> players = new List<string>();

    private void Start()
    {
        BuildRuntimeTheme();

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
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
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
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            Debug.Log("Please enter room code.");
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
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
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            Debug.Log("Create or join a room first.");
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        isReady = !isReady;

        Debug.Log("Ready state: " + isReady);

        RefreshLobbyUI();
    }

    private void OnStartGameClicked()
    {
        if (players.Count == 0)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            Debug.Log("No players in room.");
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Special, 0.82f);
        Debug.Log("Start game.");
        SceneManager.LoadScene("GameScene");
    }

    private void OnBackClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
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
        RectTransform background = RuntimeUITheme.CreateGradient(canvas.transform, "Runtime_LobbyBackground", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.13f, 0.14f, 1f));
        background.SetAsFirstSibling();
        RuntimeUITheme.SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform panel = RuntimeUITheme.CreatePanel(canvas.transform, "Runtime_LobbyPanel", new Color(0.01f, 0.04f, 0.05f, 0.92f), new Color(0.18f, 0.95f, 0.86f, 0.36f), 28, 4);
        RuntimeUITheme.SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 640f));
        RuntimeUITheme.AddShadow(panel.gameObject, new Color(0f, 0f, 0f, 0.48f), new Vector2(0f, -8f));

        TMP_Text title = RuntimeUITheme.CreateLabel(canvas.transform, "Runtime_LobbyTitle", "Multiplayer Lobby", 54, Color.white);
        RuntimeUITheme.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(900f, 74f));
        RuntimeUITheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.60f), new Vector2(0f, -5f));

        RuntimeUITheme.StyleText(roomCodeText, 34, RuntimeUITheme.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
        RuntimeUITheme.SetRect(roomCodeText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250f, 170f), new Vector2(420f, 54f));

        RuntimeUITheme.StyleText(playerListText, 26, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Bold);
        RuntimeUITheme.SetRect(playerListText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250f, -70f), new Vector2(420f, 360f));

        RuntimeUITheme.StyleInput(roomCodeInput, "Room code");
        RuntimeUITheme.SetRect(roomCodeInput.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 150f), new Vector2(360f, 62f));

        RuntimeUITheme.StyleButton(createRoomButton, RuntimeUITheme.Gold, RuntimeUITheme.Ink, "Create Room");
        RuntimeUITheme.SetRect(createRoomButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 66f), new Vector2(360f, 62f));

        RuntimeUITheme.StyleButton(joinRoomButton, RuntimeUITheme.Blue, Color.white, "Join Room");
        RuntimeUITheme.SetRect(joinRoomButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, -16f), new Vector2(360f, 62f));

        RuntimeUITheme.StyleButton(readyButton, RuntimeUITheme.Felt, Color.white, "Ready");
        RuntimeUITheme.SetRect(readyButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, -98f), new Vector2(360f, 62f));

        RuntimeUITheme.StyleButton(startGameButton, RuntimeUITheme.Red, Color.white, "Start Game");
        RuntimeUITheme.SetRect(startGameButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, -180f), new Vector2(360f, 62f));

        RuntimeUITheme.StyleButton(backButton, new Color(0.08f, 0.16f, 0.18f, 0.98f), Color.white, "Back");
        RuntimeUITheme.SetRect(backButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(94f, -42f), new Vector2(140f, 48f));

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
}
