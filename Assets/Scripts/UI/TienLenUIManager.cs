using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TienLenUIManager : MonoBehaviour
{
    private TienLenGameManager gameManager;
    private Canvas canvas;
    private RectTransform root;
    private RectTransform handPanel;
    private RectTransform tableCardsPanel;
    private TMP_Text turnText;
    private TMP_Text messageText;
    private TMP_Text tableText;
    private Button playButton;
    private Button passButton;
    private Button restartButton;
    private Button menuButton;
    private readonly HashSet<int> selectedIndices = new HashSet<int>();
    private RectTransform[] seatPanels;
    private TMP_Text[] seatNameTexts;
    private TMP_Text[] seatStatusTexts;

    public void Initialize(TienLenGameManager manager)
    {
        gameManager = manager;
        BuildUI();
        Refresh();
    }

    private void BuildUI()
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        RuntimeUITheme.ConfigureCanvas(canvas);

        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        GameObject rootObject = new GameObject("TienLenRoot", typeof(RectTransform));
        rootObject.transform.SetParent(canvas.transform, false);
        root = rootObject.GetComponent<RectTransform>();
        RuntimeUITheme.SetRect(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform background = RuntimeUITheme.CreateGradient(root, "TienLen_Background", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.12f, 0.10f, 1f));
        RuntimeUITheme.SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        BuildHeader();
        BuildTable();
        BuildSeats();
        BuildHandPanel();
        BuildButtons();
    }

    private void BuildHeader()
    {
        TMP_Text title = RuntimeUITheme.CreateLabel(root, "TienLen_Title", "TIEN LEN MIEN NAM", 42, Color.white);
        RuntimeUITheme.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(760f, 48f));
        RuntimeUITheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, -4f));

        RectTransform hud = RuntimeUITheme.CreatePanel(root, "TienLen_Hud", new Color(0.01f, 0.04f, 0.05f, 0.72f), new Color(0.18f, 0.95f, 0.86f, 0.24f), 18, 2);
        RuntimeUITheme.SetRect(hud, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(860f, 96f));

        turnText = RuntimeUITheme.CreateLabel(hud, "TurnText", "TURN", 27, Color.white);
        RuntimeUITheme.SetRect(turnText.rectTransform, new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.94f), Vector2.zero, Vector2.zero);

        messageText = RuntimeUITheme.CreateLabel(hud, "MessageText", "Message", 20, new Color(1f, 0.90f, 0.66f, 1f));
        RuntimeUITheme.SetRect(messageText.rectTransform, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.54f), Vector2.zero, Vector2.zero);
    }

    private void BuildTable()
    {
        RectTransform frame = RuntimeUITheme.CreatePanel(root, "TienLen_TableFrame", new Color(0.31f, 0.15f, 0.06f, 1f), new Color(0.88f, 0.58f, 0.24f, 1f), 34, 6);
        RuntimeUITheme.SetRect(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1040f, 410f));
        RuntimeUITheme.AddShadow(frame.gameObject, new Color(0f, 0f, 0f, 0.52f), new Vector2(0f, -10f));

        RectTransform surface = RuntimeUITheme.CreatePanel(root, "TienLen_TableSurface", new Color(0.02f, 0.34f, 0.25f, 0.96f), new Color(0.18f, 0.95f, 0.84f, 0.50f), 28, 4);
        RuntimeUITheme.SetRect(surface, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(940f, 320f));

        tableText = RuntimeUITheme.CreateLabel(root, "TienLen_TableText", "Lead any valid set", 23, RuntimeUITheme.Gold);
        RuntimeUITheme.SetRect(tableText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(620f, 34f));

        tableCardsPanel = new GameObject("TienLen_TableCards", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
        tableCardsPanel.SetParent(root, false);
        RuntimeUITheme.SetRect(tableCardsPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 28f), new Vector2(760f, 160f));

        HorizontalLayoutGroup layout = tableCardsPanel.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = -18f;
    }

    private void BuildSeats()
    {
        seatPanels = new RectTransform[4];
        seatNameTexts = new TMP_Text[4];
        seatStatusTexts = new TMP_Text[4];

        Vector2[] positions =
        {
            new Vector2(0f, -214f),
            new Vector2(-635f, 34f),
            new Vector2(0f, 264f),
            new Vector2(635f, 34f)
        };

        for (int i = 0; i < 4; i++)
        {
            RectTransform panel = RuntimeUITheme.CreatePanel(root, "TienLen_Seat_" + i, new Color(0.01f, 0.04f, 0.05f, 0.88f), new Color(0.18f, 0.95f, 0.86f, 0.30f), 20, 2);
            RuntimeUITheme.SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[i], new Vector2(256f, 72f));

            seatNameTexts[i] = RuntimeUITheme.CreateLabel(panel, "Name", "Player", 19, Color.white);
            RuntimeUITheme.SetRect(seatNameTexts[i].rectTransform, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.90f), Vector2.zero, Vector2.zero);
            seatNameTexts[i].alignment = TextAlignmentOptions.Left;

            seatStatusTexts[i] = RuntimeUITheme.CreateLabel(panel, "Status", "13 cards", 16, new Color(0.78f, 0.96f, 1f, 0.92f));
            RuntimeUITheme.SetRect(seatStatusTexts[i].rectTransform, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.52f), Vector2.zero, Vector2.zero);
            seatStatusTexts[i].alignment = TextAlignmentOptions.Left;

            seatPanels[i] = panel;
        }
    }

    private void BuildHandPanel()
    {
        handPanel = RuntimeUITheme.CreatePanel(root, "TienLen_HandPanel", new Color(0.01f, 0.03f, 0.04f, 0.70f), new Color(0.18f, 0.95f, 0.86f, 0.22f), 24, 2);
        RuntimeUITheme.SetRect(handPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 105f), new Vector2(1260f, 180f));

        HorizontalLayoutGroup layout = handPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 8f;
        layout.padding = new RectOffset(18, 18, 8, 8);
    }

    private void BuildButtons()
    {
        playButton = CreateButton("TienLen_PlayButton", "PLAY SELECTED", RuntimeUITheme.Gold, RuntimeUITheme.Ink);
        RuntimeUITheme.SetRect(playButton.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-130f, -8f), new Vector2(204f, 58f));
        playButton.onClick.AddListener(OnPlayClicked);

        passButton = CreateButton("TienLen_PassButton", "PASS", RuntimeUITheme.Blue, Color.white);
        RuntimeUITheme.SetRect(passButton.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-130f, -78f), new Vector2(204f, 58f));
        passButton.onClick.AddListener(OnPassClicked);

        menuButton = CreateButton("TienLen_MenuButton", "Menu", new Color(0.08f, 0.16f, 0.18f, 0.98f), Color.white);
        RuntimeUITheme.SetRect(menuButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(92f, -36f), new Vector2(142f, 48f));
        menuButton.onClick.AddListener(OnMenuClicked);

        restartButton = CreateButton("TienLen_RestartButton", "Restart", new Color(0.08f, 0.16f, 0.18f, 0.98f), Color.white);
        RuntimeUITheme.SetRect(restartButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-96f, -36f), new Vector2(154f, 48f));
        restartButton.onClick.AddListener(OnRestartClicked);
    }

    private Button CreateButton(string name, string label, Color fill, Color textColor)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(root, false);

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RuntimeUITheme.SetRect(labelObject.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Button button = buttonObject.GetComponent<Button>();
        RuntimeUITheme.StyleButton(button, fill, textColor, label);
        return button;
    }

    private void Refresh()
    {
        if (gameManager == null || root == null)
        {
            return;
        }

        turnText.text = gameManager.IsGameOver() ? "ROUND COMPLETE" : "TURN - " + gameManager.GetCurrentPlayerName();
        messageText.text = gameManager.GetLastMessage();
        tableText.text = gameManager.GetTableLabel();

        RefreshSeats();
        RefreshTableCards();
        RefreshHand();

        playButton.interactable = !gameManager.IsGameOver() && selectedIndices.Count > 0;
        passButton.interactable = !gameManager.IsGameOver() && gameManager.HasActiveTable();
    }

    private void RefreshSeats()
    {
        List<TienLenPlayerData> players = gameManager.GetPlayers();
        int currentIndex = gameManager.GetCurrentPlayerIndex();

        for (int i = 0; i < seatPanels.Length; i++)
        {
            TienLenPlayerData player = players[i];
            bool isCurrent = !gameManager.IsGameOver() && i == currentIndex;
            Image image = seatPanels[i].GetComponent<Image>();
            image.sprite = RuntimeUITheme.GetRoundedRectSprite(
                "tlmn_seat_" + isCurrent + "_" + player.hasFinished,
                256,
                96,
                20,
                isCurrent ? new Color(0.96f, 0.61f, 0.10f, 0.96f) : new Color(0.01f, 0.04f, 0.05f, player.hasFinished ? 0.62f : 0.88f),
                isCurrent ? new Color(1f, 0.90f, 0.34f, 0.92f) : new Color(0.18f, 0.95f, 0.86f, 0.30f),
                isCurrent ? 4 : 2);

            seatNameTexts[i].text = player.playerName;
            seatNameTexts[i].color = isCurrent ? RuntimeUITheme.Ink : Color.white;

            if (player.hasFinished)
            {
                seatStatusTexts[i].text = player.finishRank == players.Count ? "LAST PLACE" : "FINISHED #" + player.finishRank;
            }
            else if (gameManager.HasPassed(i))
            {
                seatStatusTexts[i].text = "PASS";
            }
            else
            {
                seatStatusTexts[i].text = player.handCards.Count + " cards";
            }

            seatStatusTexts[i].color = isCurrent ? RuntimeUITheme.Ink : new Color(0.78f, 0.96f, 1f, 0.92f);
            seatPanels[i].localScale = isCurrent ? Vector3.one * 1.04f : Vector3.one;
        }
    }

    private void RefreshTableCards()
    {
        ClearChildren(tableCardsPanel);
        List<PlayingCardData> cards = gameManager.GetTableCards();

        foreach (PlayingCardData card in cards)
        {
            GameObject cardObject = CreateCardObject(tableCardsPanel, new Vector2(82f, 118f));
            cardObject.GetComponent<TienLenCardView>().Setup(card, false, true);
        }
    }

    private void RefreshHand()
    {
        ClearChildren(handPanel);
        List<PlayingCardData> hand = gameManager.GetCurrentHand();
        float cardWidth = hand.Count > 11 ? 78f : 88f;
        float cardHeight = hand.Count > 11 ? 116f : 130f;

        for (int i = 0; i < hand.Count; i++)
        {
            int handIndex = i;
            GameObject cardObject = CreateCardObject(handPanel, new Vector2(cardWidth, cardHeight));
            Button button = cardObject.AddComponent<Button>();
            button.onClick.AddListener(() => ToggleCard(handIndex));

            bool selected = selectedIndices.Contains(i);
            cardObject.GetComponent<TienLenCardView>().Setup(hand[i], selected, !gameManager.IsGameOver());
        }
    }

    private GameObject CreateCardObject(Transform parent, Vector2 size)
    {
        GameObject cardObject = new GameObject("TienLenCard", typeof(RectTransform), typeof(Image), typeof(TienLenCardView), typeof(LayoutElement));
        cardObject.transform.SetParent(parent, false);
        LayoutElement layout = cardObject.GetComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
        RuntimeUITheme.SetRect(cardObject.transform as RectTransform, Vector2.zero, Vector2.zero, Vector2.zero, size);
        RuntimeUITheme.AddShadow(cardObject, new Color(0f, 0f, 0f, 0.42f), new Vector2(0f, -4f));
        return cardObject;
    }

    private void ToggleCard(int index)
    {
        if (gameManager.IsGameOver())
        {
            return;
        }

        if (selectedIndices.Contains(index))
        {
            selectedIndices.Remove(index);
        }
        else
        {
            selectedIndices.Add(index);
        }

        RuntimeSfx.Play(RuntimeSfxType.Click, 0.50f);
        Refresh();
    }

    private void OnPlayClicked()
    {
        List<int> indices = new List<int>(selectedIndices);
        bool played = gameManager.PlayCards(indices);

        if (!played)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
        }

        selectedIndices.Clear();
        Refresh();
    }

    private void OnPassClicked()
    {
        bool passed = gameManager.Pass();

        if (!passed)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
        }

        selectedIndices.Clear();
        Refresh();
    }

    private void OnRestartClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        selectedIndices.Clear();
        gameManager.StartGame();
        Refresh();
    }

    private void OnMenuClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        GameModeSelection.CurrentMode = GameMode.Uno;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
