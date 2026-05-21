using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TienLenUIManager : MonoBehaviour
{
    private const float CardAspect = 0.694f;

    private readonly HashSet<int> selectedIndices = new HashSet<int>();
    private readonly List<TienLenCardView> handCardViews = new List<TienLenCardView>();

    private TienLenGameManager gameManager;
    private TienLenThemeAssetDatabase themeAssets;
    private Canvas canvas;
    private RectTransform root;
    private RectTransform handPanel;
    private HorizontalLayoutGroup handLayout;
    private RectTransform tableCardsPanel;
    private RectTransform tableFlash;
    private CanvasGroup tableFlashGroup;
    private RectTransform effectsLayer;
    private RectTransform toastPanel;
    private CanvasGroup toastGroup;
    private TMP_Text toastText;
    private TMP_Text turnText;
    private TMP_Text messageText;
    private TMP_Text tableText;
    private Button playButton;
    private Button passButton;
    private Button restartButton;
    private Button menuButton;
    private RectTransform rankingPanel;
    private RectTransform[] seatPanels;
    private TMP_Text[] seatNameTexts;
    private TMP_Text[] seatStatusTexts;
    private bool inputLocked;
    private int renderedPlayerIndex = -1;
    private int seenFeedbackVersion;
    private Coroutine toastRoutine;
    private Coroutine tableFlashRoutine;

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
        themeAssets = Resources.Load<TienLenThemeAssetDatabase>("TienLen/TienLenThemeAssetDatabase");

        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        GameObject rootObject = new GameObject("TienLenRoot", typeof(RectTransform));
        rootObject.transform.SetParent(canvas.transform, false);
        root = rootObject.GetComponent<RectTransform>();
        RuntimeUITheme.SetRect(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        BuildBackground();

        BuildHeader();
        BuildTable();
        BuildSeats();
        BuildHandPanel();
        BuildButtons();
        BuildEffectsLayer();
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

    private void BuildBackground()
    {
        Sprite backgroundSprite = themeAssets == null ? null : RuntimeUITheme.GetTextureSprite(themeAssets.backgroundTexture, "tlmn_luxury_background");
        if (backgroundSprite != null)
        {
            Image background = RuntimeUITheme.CreateImage(root, "TienLen_BackgroundArt", backgroundSprite);
            background.preserveAspect = false;
            background.color = new Color(0.72f, 0.82f, 0.84f, 1f);
            RuntimeUITheme.SetRect(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
        else
        {
            RectTransform background = RuntimeUITheme.CreateGradient(root, "TienLen_Background", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.12f, 0.10f, 1f));
            RuntimeUITheme.SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        RectTransform vignette = RuntimeUITheme.CreateGradient(root, "TienLen_BackgroundVignette", new Color(0.01f, 0.02f, 0.02f, 0.36f), new Color(0.00f, 0.00f, 0.00f, 0.72f));
        RuntimeUITheme.SetRect(vignette, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void BuildTable()
    {
        Sprite tableSprite = themeAssets == null ? null : RuntimeUITheme.GetTextureSprite(themeAssets.tableTexture, "tlmn_luxury_table");
        if (tableSprite != null)
        {
            Image tableArt = RuntimeUITheme.CreateImage(root, "TienLen_TableArt", tableSprite);
            tableArt.preserveAspect = true;
            tableArt.color = Color.white;
            RuntimeUITheme.SetRect(tableArt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(1100f, 514f));
            RuntimeUITheme.AddShadow(tableArt.gameObject, new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, -14f));
        }
        else
        {
            RectTransform frame = RuntimeUITheme.CreatePanel(root, "TienLen_TableFrame", new Color(0.31f, 0.15f, 0.06f, 1f), new Color(0.88f, 0.58f, 0.24f, 1f), 34, 6);
            RuntimeUITheme.SetRect(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1040f, 410f));
            RuntimeUITheme.AddShadow(frame.gameObject, new Color(0f, 0f, 0f, 0.52f), new Vector2(0f, -10f));

            RectTransform surface = RuntimeUITheme.CreatePanel(root, "TienLen_TableSurface", new Color(0.02f, 0.34f, 0.25f, 0.96f), new Color(0.18f, 0.95f, 0.84f, 0.50f), 28, 4);
            RuntimeUITheme.SetRect(surface, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(940f, 320f));
        }

        tableFlash = RuntimeUITheme.CreatePanel(root, "TienLen_TableFlash", new Color(1f, 1f, 1f, 0.20f), new Color(1f, 0.84f, 0.34f, 0.20f), 28, 3);
        RuntimeUITheme.SetRect(tableFlash, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(850f, 276f));
        tableFlashGroup = tableFlash.gameObject.AddComponent<CanvasGroup>();
        tableFlashGroup.alpha = 0f;

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

        handLayout = handPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        handLayout.childAlignment = TextAnchor.MiddleCenter;
        handLayout.childControlHeight = false;
        handLayout.childControlWidth = false;
        handLayout.childForceExpandHeight = false;
        handLayout.childForceExpandWidth = false;
        handLayout.spacing = 8f;
        handLayout.padding = new RectOffset(18, 18, 8, 8);
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

    private void BuildEffectsLayer()
    {
        effectsLayer = new GameObject("TienLen_EffectsLayer", typeof(RectTransform)).GetComponent<RectTransform>();
        effectsLayer.SetParent(root, false);
        RuntimeUITheme.SetRect(effectsLayer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        effectsLayer.SetAsLastSibling();

        toastPanel = RuntimeUITheme.CreatePanel(effectsLayer, "TienLen_Toast", new Color(0.01f, 0.04f, 0.05f, 0.94f), new Color(1f, 0.76f, 0.24f, 0.82f), 18, 3);
        RuntimeUITheme.SetRect(toastPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(760f, 86f));
        toastGroup = toastPanel.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastPanel.GetComponent<Image>().raycastTarget = false;

        toastText = RuntimeUITheme.CreateLabel(toastPanel, "ToastText", "", 24, Color.white);
        RuntimeUITheme.SetRect(toastText.rectTransform, new Vector2(0.06f, 0.10f), new Vector2(0.94f, 0.90f), Vector2.zero, Vector2.zero);
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

    private void Refresh(bool processFeedback = true)
    {
        if (gameManager == null || root == null)
        {
            return;
        }

        turnText.text = gameManager.IsGameOver() ? "ROUND COMPLETE" : "TURN - " + gameManager.GetCurrentPlayerName();
        messageText.text = gameManager.GetLastMessage();
        tableText.text = gameManager.GetTableLabel();

        ClearSelectionIfPlayerChanged();
        RefreshSeats();
        RefreshTableCards();
        RefreshHand();
        UpdateActionButtons();

        if (processFeedback)
        {
            ProcessFeedback(gameManager.GetFeedbackSnapshot());
        }
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
            GameObject cardObject = CreateCardObject(tableCardsPanel, CardSizeFromHeight(126f));
            cardObject.GetComponent<TienLenCardView>().Setup(card, false, true);
        }
    }

    private void RefreshHand()
    {
        ClearChildren(handPanel);
        handCardViews.Clear();
        List<PlayingCardData> hand = gameManager.GetCurrentHand();

        if (handLayout != null)
        {
            handLayout.spacing = hand.Count > 11 ? -8f : 4f;
            handLayout.padding = new RectOffset(18, 18, 4, 4);
        }

        float cardHeight = hand.Count > 11 ? 142f : 156f;
        Vector2 cardSize = CardSizeFromHeight(cardHeight);

        for (int i = 0; i < hand.Count; i++)
        {
            int handIndex = i;
            GameObject cardObject = CreateCardObject(handPanel, cardSize);
            Button button = cardObject.AddComponent<Button>();
            button.onClick.AddListener(() => ToggleCard(handIndex));

            bool selected = selectedIndices.Contains(i);
            TienLenCardView cardView = cardObject.GetComponent<TienLenCardView>();
            cardView.Setup(hand[i], selected, !gameManager.IsGameOver() && !inputLocked);
            handCardViews.Add(cardView);
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

    private Vector2 CardSizeFromHeight(float height)
    {
        return new Vector2(Mathf.Round(height * CardAspect), height);
    }

    private void ToggleCard(int index)
    {
        if (inputLocked || gameManager.IsGameOver() || index < 0 || index >= handCardViews.Count)
        {
            return;
        }

        bool selected;
        if (selectedIndices.Contains(index))
        {
            selectedIndices.Remove(index);
            selected = false;
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.38f);
        }
        else
        {
            selectedIndices.Add(index);
            selected = true;
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.55f);
        }

        handCardViews[index].SetSelected(selected, true);
        UpdateActionButtons();
    }

    private void OnPlayClicked()
    {
        if (inputLocked)
        {
            return;
        }

        List<int> indices = new List<int>(selectedIndices);
        indices.Sort();

        if (!gameManager.CanPlaySelection(indices))
        {
            bool played = gameManager.PlayCards(indices);
            if (!played)
            {
                RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
                Refresh();
            }

            return;
        }

        StartCoroutine(PlaySelectedCardsRoutine(indices));
    }

    private IEnumerator PlaySelectedCardsRoutine(List<int> indices)
    {
        inputLocked = true;
        UpdateActionButtons();
        RuntimeSfx.Play(RuntimeSfxType.Play, 0.74f);

        List<PlayingCardData> hand = new List<PlayingCardData>(gameManager.GetCurrentHand());
        List<RectTransform> clones = new List<RectTransform>();
        List<Vector2> startPositions = new List<Vector2>();
        List<Vector2> targetPositions = new List<Vector2>();
        List<Vector2> startSizes = new List<Vector2>();
        Vector2 targetSize = CardSizeFromHeight(126f);
        int visibleIndex = 0;

        foreach (int index in indices)
        {
            if (index < 0 || index >= hand.Count || index >= handCardViews.Count)
            {
                continue;
            }

            RectTransform sourceRect = handCardViews[index].transform as RectTransform;
            Vector2 startPosition = ToEffectsLayerPosition(sourceRect);
            Vector2 targetPosition = tableCardsPanel.anchoredPosition + new Vector2((visibleIndex - (indices.Count - 1) * 0.5f) * 58f, 0f);
            GameObject cloneObject = CreateCardObject(effectsLayer, sourceRect.rect.size);
            RectTransform cloneRect = cloneObject.transform as RectTransform;
            RuntimeUITheme.SetRect(cloneRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), startPosition, sourceRect.rect.size);
            cloneObject.GetComponent<TienLenCardView>().Setup(hand[index], false, true);

            handCardViews[index].SetTemporaryHidden(true);
            clones.Add(cloneRect);
            startPositions.Add(startPosition);
            targetPositions.Add(targetPosition);
            startSizes.Add(sourceRect.rect.size);
            visibleIndex++;
        }

        yield return AnimateCardsToTable(clones, startPositions, targetPositions, startSizes, targetSize);

        bool played = gameManager.PlayCards(indices);
        foreach (RectTransform clone in clones)
        {
            if (clone != null)
            {
                Destroy(clone.gameObject);
            }
        }

        if (!played)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
        }

        selectedIndices.Clear();
        inputLocked = false;
        Refresh();
    }

    private IEnumerator AnimateCardsToTable(List<RectTransform> clones, List<Vector2> starts, List<Vector2> targets, List<Vector2> startSizes, Vector2 targetSize)
    {
        float duration = 0.32f;
        float stagger = 0.035f;
        float elapsed = 0f;
        float totalDuration = duration + Mathf.Max(0, clones.Count - 1) * stagger;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < clones.Count; i++)
            {
                if (clones[i] == null)
                {
                    continue;
                }

                float localTime = Mathf.Clamp01((elapsed - i * stagger) / duration);
                float t = EaseOutCubic(localTime);
                clones[i].anchoredPosition = Vector2.LerpUnclamped(starts[i], targets[i], t);
                clones[i].sizeDelta = Vector2.LerpUnclamped(startSizes[i], targetSize, t);
                clones[i].localScale = Vector3.LerpUnclamped(Vector3.one * 1.04f, Vector3.one, t);
            }

            yield return null;
        }
    }

    private void OnPassClicked()
    {
        if (inputLocked)
        {
            return;
        }

        int actorIndex = gameManager.GetCurrentPlayerIndex();
        bool passed = gameManager.Pass();

        if (!passed)
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
            Refresh();
            return;
        }

        TienLenFeedbackSnapshot snapshot = gameManager.GetFeedbackSnapshot();
        selectedIndices.Clear();

        if (snapshot.kind == TienLenFeedbackKind.NewTrick)
        {
            StartCoroutine(PassAndClearTableRoutine(actorIndex));
        }
        else
        {
            Refresh();
        }
    }

    private IEnumerator PassAndClearTableRoutine(int actorIndex)
    {
        inputLocked = true;
        UpdateActionButtons();
        ShowFloatingBadge(actorIndex, "PASS", RuntimeUITheme.Blue);
        yield return FadeTableCardsOut();
        inputLocked = false;
        Refresh();
    }

    private IEnumerator FadeTableCardsOut()
    {
        List<RectTransform> cards = new List<RectTransform>();
        List<CanvasGroup> groups = new List<CanvasGroup>();
        List<Vector2> starts = new List<Vector2>();

        foreach (Transform child in tableCardsPanel)
        {
            RectTransform rect = child as RectTransform;
            if (rect == null)
            {
                continue;
            }

            CanvasGroup group = rect.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = rect.gameObject.AddComponent<CanvasGroup>();
            }

            cards.Add(rect);
            groups.Add(group);
            starts.Add(rect.anchoredPosition);
        }

        float duration = 0.22f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < cards.Count; i++)
            {
                groups[i].alpha = 1f - t;
                cards[i].anchoredPosition = starts[i] + new Vector2(0f, 28f * t);
            }

            yield return null;
        }
    }

    private void OnRestartClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        selectedIndices.Clear();
        seenFeedbackVersion = 0;
        inputLocked = false;
        CloseRankingPanel();
        gameManager.StartGame();
        Refresh();
    }

    private void OnMenuClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        GameModeSelection.CurrentMode = GameMode.Uno;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void UpdateActionButtons()
    {
        if (playButton != null)
        {
            playButton.interactable = !inputLocked && !gameManager.IsGameOver() && selectedIndices.Count > 0;
        }

        if (passButton != null)
        {
            passButton.interactable = !inputLocked && !gameManager.IsGameOver() && gameManager.HasActiveTable();
        }
    }

    private void ClearSelectionIfPlayerChanged()
    {
        int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
        if (renderedPlayerIndex != currentPlayerIndex)
        {
            selectedIndices.Clear();
            renderedPlayerIndex = currentPlayerIndex;
        }
    }

    private void ProcessFeedback(TienLenFeedbackSnapshot feedback)
    {
        if (feedback == null || feedback.version <= seenFeedbackVersion)
        {
            return;
        }

        seenFeedbackVersion = feedback.version;

        switch (feedback.kind)
        {
            case TienLenFeedbackKind.GameStart:
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Gold);
                PulseSeat(feedback.actorIndex);
                RuntimeSfx.Play(RuntimeSfxType.Turn, 0.45f);
                break;

            case TienLenFeedbackKind.Play:
                FlashTable(new Color(1f, 0.84f, 0.28f, 0.42f));
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Gold);
                PulseSeat(feedback.nextPlayerIndex);
                break;

            case TienLenFeedbackKind.Bomb:
                FlashTable(new Color(1f, 0.18f, 0.10f, 0.52f));
                ShowToast(feedback.title, feedback.message, new Color(1f, 0.38f, 0.24f, 1f));
                SpawnBurst(tableCardsPanel.anchoredPosition, new Color(1f, 0.46f, 0.16f, 1f), 18);
                RuntimeSfx.Play(RuntimeSfxType.Bomb, 0.92f);
                break;

            case TienLenFeedbackKind.Pass:
                ShowFloatingBadge(feedback.actorIndex, "PASS", RuntimeUITheme.Blue);
                PulseSeat(feedback.nextPlayerIndex);
                break;

            case TienLenFeedbackKind.NewTrick:
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Cyan);
                PulseSeat(feedback.nextPlayerIndex);
                RuntimeSfx.Play(RuntimeSfxType.Turn, 0.48f);
                break;

            case TienLenFeedbackKind.Invalid:
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Red);
                ShakeSelectedCards();
                break;

            case TienLenFeedbackKind.Finish:
                ShowFloatingBadge(feedback.actorIndex, "#" + feedback.finishRank, RuntimeUITheme.Gold);
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Gold);
                SpawnBurst(GetSeatPosition(feedback.actorIndex), RuntimeUITheme.Gold, 18);
                break;

            case TienLenFeedbackKind.InstantWin:
            case TienLenFeedbackKind.RoundComplete:
                ShowToast(feedback.title, feedback.message, RuntimeUITheme.Gold);
                SpawnBurst(Vector2.zero, RuntimeUITheme.Gold, 26);
                RuntimeSfx.Play(RuntimeSfxType.RoundComplete, 0.92f);
                ShowRankingPanel();
                break;
        }
    }

    private void ShowToast(string title, string message, Color accent)
    {
        if (toastPanel == null || toastGroup == null || toastText == null)
        {
            return;
        }

        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        Image image = toastPanel.GetComponent<Image>();
        image.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_toast_" + title, 256, 96, 18, new Color(0.01f, 0.04f, 0.05f, 0.94f), accent, 3);
        toastText.text = string.IsNullOrEmpty(message) ? title : title + "\n" + message;
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    private IEnumerator ToastRoutine()
    {
        toastPanel.localScale = Vector3.one * 0.96f;
        yield return FadeCanvas(toastGroup, 0f, 1f, 0.10f);
        yield return ScaleRect(toastPanel, Vector3.one * 0.96f, Vector3.one, 0.14f);
        yield return new WaitForSecondsRealtime(1.05f);
        yield return FadeCanvas(toastGroup, 1f, 0f, 0.22f);
    }

    private void FlashTable(Color color)
    {
        if (tableFlashGroup == null || tableFlash == null)
        {
            return;
        }

        if (tableFlashRoutine != null)
        {
            StopCoroutine(tableFlashRoutine);
        }

        tableFlash.GetComponent<Image>().color = color;
        tableFlashRoutine = StartCoroutine(TableFlashRoutine());
    }

    private IEnumerator TableFlashRoutine()
    {
        yield return FadeCanvas(tableFlashGroup, 0f, 1f, 0.06f);
        yield return FadeCanvas(tableFlashGroup, 1f, 0f, 0.26f);
    }

    private void ShowFloatingBadge(int playerIndex, string text, Color fill)
    {
        if (playerIndex < 0 || playerIndex >= seatPanels.Length || effectsLayer == null)
        {
            return;
        }

        RectTransform badge = RuntimeUITheme.CreatePanel(effectsLayer, "TienLen_FloatingBadge", fill, new Color(1f, 0.94f, 0.62f, 0.90f), 18, 3);
        RuntimeUITheme.SetRect(badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), GetSeatPosition(playerIndex) + new Vector2(0f, 56f), new Vector2(154f, 48f));
        TMP_Text label = RuntimeUITheme.CreateLabel(badge, "Label", text, 24, playerIndex == 0 ? RuntimeUITheme.Ink : Color.white);
        RuntimeUITheme.SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = badge.gameObject.AddComponent<CanvasGroup>();
        StartCoroutine(FloatingBadgeRoutine(badge, group));
    }

    private IEnumerator FloatingBadgeRoutine(RectTransform badge, CanvasGroup group)
    {
        Vector2 start = badge.anchoredPosition;
        badge.localScale = Vector3.one * 0.88f;
        float duration = 0.72f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            badge.anchoredPosition = start + new Vector2(0f, 34f * t);
            badge.localScale = Vector3.LerpUnclamped(Vector3.one * 0.88f, Vector3.one * 1.08f, EaseOutCubic(Mathf.Min(t * 1.6f, 1f)));
            group.alpha = t < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.72f) / 0.28f);
            yield return null;
        }

        Destroy(badge.gameObject);
    }

    private void ShakeSelectedCards()
    {
        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < handCardViews.Count)
            {
                handCardViews[index].FlashInvalid();
            }
        }

        if (selectedIndices.Count == 0 && handPanel != null)
        {
            StartCoroutine(ShakeRect(handPanel, 8f, 0.20f));
        }
    }

    private void PulseSeat(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= seatPanels.Length)
        {
            return;
        }

        StartCoroutine(PulseRect(seatPanels[playerIndex]));
    }

    private IEnumerator PulseRect(RectTransform rect)
    {
        Vector3 start = rect.localScale;
        Vector3 peak = start * 1.10f;
        yield return ScaleRect(rect, start, peak, 0.10f);
        yield return ScaleRect(rect, peak, start, 0.16f);
    }

    private void SpawnBurst(Vector2 center, Color color, int count)
    {
        if (effectsLayer == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Image particle = RuntimeUITheme.CreateImage(effectsLayer, "TienLen_Burst", RuntimeUITheme.GetCircleSprite("tlmn_burst", 18, color, Color.white, 2));
            particle.color = Color.white;
            float angle = (360f / count) * i + Random.Range(-8f, 8f);
            float distance = Random.Range(62f, 138f);
            Vector2 target = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;
            RuntimeUITheme.SetRect(particle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), center, new Vector2(16f, 16f));
            StartCoroutine(BurstParticleRoutine(particle.rectTransform, target));
        }
    }

    private IEnumerator BurstParticleRoutine(RectTransform particle, Vector2 target)
    {
        CanvasGroup group = particle.gameObject.AddComponent<CanvasGroup>();
        Vector2 start = particle.anchoredPosition;
        float duration = 0.58f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            particle.anchoredPosition = Vector2.LerpUnclamped(start, target, EaseOutCubic(t));
            particle.localScale = Vector3.one * Mathf.Lerp(1f, 0.30f, t);
            group.alpha = 1f - t;
            yield return null;
        }

        Destroy(particle.gameObject);
    }

    private void ShowRankingPanel()
    {
        CloseRankingPanel();

        rankingPanel = RuntimeUITheme.CreatePanel(effectsLayer, "TienLen_RankingPanel", new Color(0.01f, 0.04f, 0.05f, 0.96f), new Color(1f, 0.76f, 0.24f, 0.86f), 26, 4);
        RuntimeUITheme.SetRect(rankingPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 430f));
        RuntimeUITheme.AddShadow(rankingPanel.gameObject, new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, -10f));
        CanvasGroup group = rankingPanel.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        TMP_Text title = RuntimeUITheme.CreateLabel(rankingPanel, "Title", "ROUND RESULTS", 34, RuntimeUITheme.Gold);
        RuntimeUITheme.SetRect(title.rectTransform, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero);

        List<TienLenPlayerData> players = new List<TienLenPlayerData>(gameManager.GetPlayers());
        players.Sort((a, b) => a.finishRank.CompareTo(b.finishRank));

        for (int i = 0; i < players.Count; i++)
        {
            TienLenPlayerData player = players[i];
            string suffix = player.finishRank == 1 ? " WINNER" : (player.finishRank == players.Count ? " LAST PLACE" : "");
            TMP_Text row = RuntimeUITheme.CreateLabel(rankingPanel, "Rank_" + i, player.finishRank + ". " + player.playerName + suffix, 24, Color.white);
            RuntimeUITheme.SetRect(row.rectTransform, new Vector2(0.12f, 0.62f - i * 0.12f), new Vector2(0.88f, 0.72f - i * 0.12f), Vector2.zero, Vector2.zero);
            row.alignment = TextAlignmentOptions.Left;
        }

        Button restart = CreateOverlayButton(rankingPanel, "Play Again", RuntimeUITheme.Gold, RuntimeUITheme.Ink);
        RuntimeUITheme.SetRect(restart.transform as RectTransform, new Vector2(0.16f, 0.07f), new Vector2(0.48f, 0.20f), Vector2.zero, Vector2.zero);
        restart.onClick.AddListener(OnRestartClicked);

        Button menu = CreateOverlayButton(rankingPanel, "Menu", new Color(0.08f, 0.16f, 0.18f, 1f), Color.white);
        RuntimeUITheme.SetRect(menu.transform as RectTransform, new Vector2(0.52f, 0.07f), new Vector2(0.84f, 0.20f), Vector2.zero, Vector2.zero);
        menu.onClick.AddListener(OnMenuClicked);

        StartCoroutine(FadeCanvas(group, 0f, 1f, 0.18f));
    }

    private Button CreateOverlayButton(Transform parent, string label, Color fill, Color textColor)
    {
        GameObject buttonObject = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RuntimeUITheme.SetRect(labelObject.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Button button = buttonObject.GetComponent<Button>();
        RuntimeUITheme.StyleButton(button, fill, textColor, label);
        return button;
    }

    private void CloseRankingPanel()
    {
        if (rankingPanel != null)
        {
            Destroy(rankingPanel.gameObject);
            rankingPanel = null;
        }
    }

    private Vector2 ToEffectsLayerPosition(RectTransform rect)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(effectsLayer, screenPoint, canvas.worldCamera, out Vector2 localPoint);
        return localPoint;
    }

    private Vector2 GetSeatPosition(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= seatPanels.Length)
        {
            return Vector2.zero;
        }

        return seatPanels[playerIndex].anchoredPosition;
    }

    private IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, EaseOutCubic(t));
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator ScaleRect(RectTransform rect, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        rect.localScale = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localScale = Vector3.LerpUnclamped(from, to, EaseOutCubic(t));
            yield return null;
        }

        rect.localScale = to;
    }

    private IEnumerator ShakeRect(RectTransform rect, float strength, float duration)
    {
        Vector2 start = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float offset = Mathf.Sin(t * Mathf.PI * 8f) * strength * (1f - t);
            rect.anchoredPosition = start + new Vector2(offset, 0f);
            yield return null;
        }

        rect.anchoredPosition = start;
    }

    private float EaseOutCubic(float t)
    {
        float p = t - 1f;
        return p * p * p + 1f;
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
