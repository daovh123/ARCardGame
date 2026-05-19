using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

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
    public TMP_Text currentColorText;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public Button gameOverRestartButton;

    private static CardSpriteDatabase cardDatabase;

    private Image topCardImage;
    private Image drawPileImage;
    private GameObject colorChoicePanel;
    private Button unoButton;
    private RectTransform[] seatPanels;
    private TMP_Text[] seatNameTexts;
    private TMP_Text[] seatCountTexts;
    private Image[] seatCardImages;
    private int pendingWildCardIndex = -1;
    private bool themeBuilt;
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private int drawAnimationSequence;

    private void OnEnable()
    {
        GameEvents.OnCardDrawn += HandleCardDrawnEvent;
        GameEvents.OnCardPlayed += HandleCardPlayedEvent;
    }

    private void OnDisable()
    {
        GameEvents.OnCardDrawn -= HandleCardDrawnEvent;
        GameEvents.OnCardPlayed -= HandleCardPlayedEvent;
    }

    private void Start()
    {
        BuildRuntimeTheme();

        drawButton.onClick.AddListener(OnDrawButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        backMenuButton.onClick.AddListener(OnBackMenuClicked);

        gameOverPanel.SetActive(false);

        RefreshUI();
    }

    public void RefreshUI()
    {
        BuildRuntimeTheme();

        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        CardData topCard = gameManager.GetTopDiscardCard();

        if (topCard != null)
        {
            topCardText.text = "DISCARD";
            UpdateTopCardVisual(topCard);
        }

        bool isLocalTurn = gameManager.IsLocalPlayerTurn();
        bool isGameOver = gameManager.IsGameOver();

        currentTurnText.text = isLocalTurn
            ? "YOUR TURN - " + gameManager.GetCurrentPlayerName()
            : "TURN - " + gameManager.GetCurrentPlayerName();
        messageText.text = gameManager.GetLastMessage();

        bool pendingDrawPenalty = gameManager.HasPendingDrawPenalty();

        if (currentColorText != null)
        {
            currentColorText.text = pendingDrawPenalty
                ? "STACK " + gameManager.GetPendingDrawStackLabel().ToUpperInvariant() + " OR DRAW " + gameManager.GetPendingDrawPenalty()
                : "COLOR " + gameManager.GetCurrentColor().ToString().ToUpperInvariant();
            currentColorText.color = pendingDrawPenalty ? new Color(1f, 0.78f, 0.22f) : GetUITextColor(gameManager.GetCurrentColor());
        }

        SetButtonLabel(drawButton, pendingDrawPenalty ? "DRAW +" + gameManager.GetPendingDrawPenalty() : "DRAW");
        drawButton.interactable = isLocalTurn && !isGameOver;

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();

        if ((!isLocalTurn || isGameOver || pendingWildCardIndex < 0 || pendingWildCardIndex >= handCards.Count) && colorChoicePanel != null)
        {
            HideColorChoice();
        }

        for (int i = 0; i < handCards.Count; i++)
        {
            GameObject cardObject = Instantiate(cardUIPrefab, handPanel);

            CardUI cardUI = cardObject.GetComponent<CardUI>();
            cardUI.Setup(handCards[i], i, OnCardClicked);

            bool canPlayCard = isLocalTurn && !isGameOver && gameManager.CanPlayHandCard(i);
            cardUI.SetPlayable(canPlayCard);

            Button cardButton = cardObject.GetComponent<Button>();

            if (cardButton != null)
            {
                cardButton.interactable = isLocalTurn && !isGameOver;
            }
        }

        UpdateUnoButton(isLocalTurn, isGameOver, handCards.Count);
        RefreshPlayerStatus();
        RefreshSeatPanels();

        if (isGameOver)
        {
            gameOverPanel.SetActive(true);
            winnerText.text = gameManager.GetWinnerName() + " wins!";
        }
        else
        {
            gameOverPanel.SetActive(false);
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

        rootCanvas = canvas;
        canvasRect = canvas.transform as RectTransform;

        LoadCardDatabase();
        ConfigureCanvas(canvas);
        BuildBackground(canvas.transform);
        BuildTopCardDisplay(canvas.transform);
        BuildSeatPanels(canvas.transform);
        StyleExistingLayout();
        BuildUnoButton(canvas.transform);
        BuildColorChoicePanel(canvas.transform);
        StyleGameOverPanel();

        themeBuilt = true;
    }

    private void LoadCardDatabase()
    {
        if (cardDatabase == null)
        {
            cardDatabase = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
        }
    }

    private Canvas ResolveCanvas()
    {
        if (drawButton != null)
        {
            Canvas canvas = drawButton.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
        }

        return FindAnyObjectByType<Canvas>();
    }

    private void ConfigureCanvas(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void BuildBackground(Transform canvasTransform)
    {
        RectTransform background = CreatePanel(canvasTransform, "Runtime_Background", new Color(0.02f, 0.10f, 0.13f, 1f));
        background.SetAsFirstSibling();
        SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform table = CreatePanel(canvasTransform, "Runtime_TableSurface", new Color(0.02f, 0.30f, 0.24f, 0.92f));
        table.SetSiblingIndex(1);
        SetRect(table, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(900f, 360f));

        Outline tableOutline = table.GetComponent<Outline>();
        if (tableOutline == null)
        {
            tableOutline = table.gameObject.AddComponent<Outline>();
        }

        tableOutline.effectColor = new Color(0.20f, 0.95f, 0.85f, 0.28f);
        tableOutline.effectDistance = new Vector2(4f, -4f);
    }

    private void BuildTopCardDisplay(Transform canvasTransform)
    {
        RectTransform topPanel = CreatePanel(canvasTransform, "Runtime_DiscardPanel", new Color(0.01f, 0.04f, 0.05f, 0.74f));
        SetRect(topPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(360f, 270f));

        topCardImage = CreateImage(topPanel, "TopCardImage");
        topCardImage.preserveAspect = true;
        SetRect(topCardImage.rectTransform, new Vector2(0.50f, 0.48f), new Vector2(0.50f, 0.48f), new Vector2(62f, -8f), new Vector2(118f, 174f));

        drawPileImage = CreateImage(topPanel, "DrawPileImage");
        drawPileImage.preserveAspect = true;
        if (cardDatabase != null)
        {
            drawPileImage.sprite = cardDatabase.card_back;
        }
        SetRect(drawPileImage.rectTransform, new Vector2(0.50f, 0.48f), new Vector2(0.50f, 0.48f), new Vector2(-78f, -8f), new Vector2(118f, 174f));
        BuildDrawPileStack(topPanel);
        drawPileImage.transform.SetAsLastSibling();
        topCardImage.transform.SetAsLastSibling();

        TMP_Text drawLabel = CreateLabel(topPanel, "DRAW", 21, new Color(0.80f, 1f, 0.95f, 0.92f));
        SetRect(drawLabel.rectTransform, new Vector2(0.05f, 0.84f), new Vector2(0.45f, 0.98f), Vector2.zero, Vector2.zero);

        TMP_Text discardLabel = CreateLabel(topPanel, "DISCARD", 21, new Color(1f, 0.82f, 0.36f, 0.95f));
        SetRect(discardLabel.rectTransform, new Vector2(0.55f, 0.84f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);
    }

    private void BuildDrawPileStack(Transform parent)
    {
        if (cardDatabase == null || cardDatabase.card_back == null)
        {
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            Image stackImage = CreateImage(parent, "DrawPileStack_" + i);
            stackImage.sprite = cardDatabase.card_back;
            stackImage.preserveAspect = true;
            stackImage.color = new Color(1f, 1f, 1f, 0.62f + i * 0.12f);
            SetRect(
                stackImage.rectTransform,
                new Vector2(0.50f, 0.48f),
                new Vector2(0.50f, 0.48f),
                new Vector2(-88f + i * 5f, -16f + i * 4f),
                new Vector2(118f, 174f));
            stackImage.transform.SetSiblingIndex(0);
        }
    }

    private void BuildSeatPanels(Transform canvasTransform)
    {
        seatPanels = new RectTransform[4];
        seatNameTexts = new TMP_Text[4];
        seatCountTexts = new TMP_Text[4];
        seatCardImages = new Image[4];

        Vector2[] positions =
        {
            new Vector2(0f, -228f),
            new Vector2(-450f, -10f),
            new Vector2(0f, 236f),
            new Vector2(450f, -10f)
        };

        for (int i = 0; i < seatPanels.Length; i++)
        {
            RectTransform panel = CreatePanel(canvasTransform, "Runtime_PlayerSeat_" + i, new Color(0.02f, 0.06f, 0.08f, 0.78f));
            SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[i], new Vector2(220f, 58f));

            Image cardBack = CreateImage(panel, "CardBack");
            cardBack.preserveAspect = true;
            if (cardDatabase != null)
            {
                cardBack.sprite = cardDatabase.card_back;
            }
            SetRect(cardBack.rectTransform, new Vector2(0.04f, 0.14f), new Vector2(0.22f, 0.86f), Vector2.zero, Vector2.zero);

            TMP_Text nameText = CreateLabel(panel, "Player", 18, Color.white);
            SetRect(nameText.rectTransform, new Vector2(0.27f, 0.50f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero);
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.fontStyle = FontStyles.Bold;

            TMP_Text countText = CreateLabel(panel, "0 cards", 16, new Color(0.78f, 0.96f, 1f, 0.90f));
            SetRect(countText.rectTransform, new Vector2(0.27f, 0.14f), new Vector2(0.96f, 0.50f), Vector2.zero, Vector2.zero);
            countText.alignment = TextAlignmentOptions.Left;

            seatPanels[i] = panel;
            seatNameTexts[i] = nameText;
            seatCountTexts[i] = countText;
            seatCardImages[i] = cardBack;
        }
    }

    private void StyleExistingLayout()
    {
        RectTransform handRect = handPanel as RectTransform;
        SetRect(handRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 122f), new Vector2(900f, 156f));

        Image handImage = handPanel.GetComponent<Image>();
        if (handImage == null)
        {
            handImage = handPanel.gameObject.AddComponent<Image>();
        }
        handImage.color = new Color(0.02f, 0.04f, 0.06f, 0.54f);

        HorizontalLayoutGroup layout = handPanel.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(16, 16, 6, 6);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        StyleText(currentTurnText, 28, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(currentTurnText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(700f, 36f));

        StyleText(currentColorText, 22, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(currentColorText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(620f, 30f));

        StyleText(messageText, 21, new Color(1f, 0.90f, 0.66f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(messageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(900f, 30f));

        if (topCardText != null)
        {
            topCardText.gameObject.SetActive(false);
        }

        if (playerStatusText != null)
        {
            playerStatusText.gameObject.SetActive(false);
        }

        StyleButton(drawButton, new Color(1f, 0.74f, 0.18f, 1f), new Color(0.05f, 0.08f, 0.10f, 1f));
        SetRect(drawButton.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, -22f), new Vector2(148f, 54f));

        StyleButton(restartButton, new Color(0.05f, 0.16f, 0.20f, 0.96f), Color.white);
        SetRect(restartButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-88f, -32f), new Vector2(140f, 44f));

        StyleButton(backMenuButton, new Color(0.05f, 0.16f, 0.20f, 0.96f), Color.white);
        SetRect(backMenuButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(88f, -32f), new Vector2(140f, 44f));
    }

    private void BuildUnoButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject("Runtime_UNOButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, -86f), new Vector2(148f, 54f), new Vector2(0.5f, 0.5f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.86f, 0.08f, 0.12f, 0.98f);

        unoButton = buttonObject.GetComponent<Button>();
        unoButton.onClick.AddListener(OnUnoButtonClicked);

        TMP_Text label = CreateLabel(buttonObject.transform, "UNO", 32, Color.white);
        label.fontStyle = FontStyles.Bold;
    }

    private void BuildColorChoicePanel(Transform canvasTransform)
    {
        colorChoicePanel = new GameObject("Runtime_ColorChoicePanel", typeof(RectTransform), typeof(Image));
        colorChoicePanel.transform.SetParent(canvasTransform, false);

        RectTransform rect = colorChoicePanel.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(610f, 226f));

        Image panelImage = colorChoicePanel.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.04f, 0.06f, 0.96f);

        TMP_Text title = CreateLabel(colorChoicePanel.transform, "CHOOSE COLOR", 34, new Color(1f, 0.82f, 0.36f, 1f));
        SetRect(title.rectTransform, new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyles.Bold;

        CreateColorButton(colorChoicePanel.transform, CardColor.Red, "RED", new Vector2(-218f, -44f), new Color(0.90f, 0.08f, 0.12f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Blue, "BLUE", new Vector2(-72f, -44f), new Color(0.08f, 0.30f, 0.94f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Green, "GREEN", new Vector2(72f, -44f), new Color(0.02f, 0.62f, 0.30f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Yellow, "YELLOW", new Vector2(218f, -44f), new Color(0.98f, 0.76f, 0.08f), Color.black);

        colorChoicePanel.SetActive(false);
    }

    private void CreateColorButton(Transform parent, CardColor color, string labelText, Vector2 position, Color fill, Color textColor)
    {
        GameObject buttonObject = new GameObject(labelText + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), position, new Vector2(124f, 70f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = fill;

        Button button = buttonObject.GetComponent<Button>();
        CardColor chosenColor = color;
        button.onClick.AddListener(() => OnColorChosen(chosenColor));

        TMP_Text label = CreateLabel(buttonObject.transform, labelText, 20, textColor);
        label.fontStyle = FontStyles.Bold;
    }

    private void StyleGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        RectTransform panelRect = gameOverPanel.transform as RectTransform;
        SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 360f));

        Image image = gameOverPanel.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.02f, 0.04f, 0.06f, 0.96f);
        }

        StyleText(winnerText, 48, new Color(1f, 0.82f, 0.36f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        StyleButton(gameOverRestartButton, new Color(1f, 0.74f, 0.18f, 1f), Color.black);
    }

    private void UpdateTopCardVisual(CardData card)
    {
        if (topCardImage == null)
        {
            return;
        }

        LoadCardDatabase();
        Sprite sprite = cardDatabase != null ? cardDatabase.GetSprite(card) : null;

        if (sprite != null)
        {
            topCardImage.sprite = sprite;
            topCardImage.color = Color.white;
        }
        else
        {
            topCardImage.sprite = null;
            topCardImage.color = GetUITextColor(card.color);
        }
    }

    private void HandleCardDrawnEvent(int playerIndex)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        BuildRuntimeTheme();
        if (drawPileImage == null || canvasRect == null)
        {
            return;
        }

        LoadCardDatabase();
        Sprite sprite = cardDatabase != null ? cardDatabase.card_back : null;
        Vector2 start = GetRectScreenCenter(drawPileImage.rectTransform);
        Vector2 end = GetPlayerTargetScreenPosition(playerIndex);
        float delay = (drawAnimationSequence++ % 6) * 0.045f;

        StartCoroutine(PulseRect(drawPileImage.rectTransform, delay));
        StartCoroutine(AnimateCardSprite(sprite, start, end, new Vector2(124f, 182f), 0.58f, delay));
    }

    private void HandleCardPlayedEvent(CardData card, int playerIndex)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        BuildRuntimeTheme();
        if (topCardImage == null || canvasRect == null)
        {
            return;
        }

        LoadCardDatabase();
        Sprite sprite = cardDatabase != null ? cardDatabase.GetSprite(card) : null;
        Vector2 start = GetPlayerPlayOrigin(playerIndex);
        Vector2 end = GetRectScreenCenter(topCardImage.rectTransform);

        StartCoroutine(AnimateCardSprite(sprite, start, end, new Vector2(142f, 210f), 0.50f));
        StartCoroutine(PulseRect(topCardImage.rectTransform, 0.35f));
    }

    private Vector2 GetPlayerPlayOrigin(int playerIndex)
    {
        if (gameManager != null && playerIndex == gameManager.GetCurrentPlayerIndex() && handPanel is RectTransform handRect)
        {
            return GetRectScreenCenter(handRect);
        }

        return GetPlayerTargetScreenPosition(playerIndex);
    }

    private Vector2 GetPlayerTargetScreenPosition(int playerIndex)
    {
        if (seatPanels != null && playerIndex >= 0 && playerIndex < seatPanels.Length && seatPanels[playerIndex] != null)
        {
            return GetRectScreenCenter(seatPanels[playerIndex]);
        }

        if (handPanel is RectTransform handRect)
        {
            return GetRectScreenCenter(handRect);
        }

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private Vector2 GetRectScreenCenter(RectTransform rect)
    {
        if (rect == null)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        return RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), rect.position);
    }

    private Camera GetCanvasCamera()
    {
        if (rootCanvas == null || rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return rootCanvas.worldCamera;
    }

    private IEnumerator AnimateCardSprite(Sprite sprite, Vector2 startScreen, Vector2 endScreen, Vector2 size, float duration, float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (canvasRect == null)
        {
            yield break;
        }

        GameObject cardObject = new GameObject("Runtime_FlyingCard", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        cardObject.transform.SetParent(canvasRect, false);
        cardObject.transform.SetAsLastSibling();

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = cardObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = sprite != null ? Color.white : new Color(1f, 0.82f, 0.20f, 0.92f);

        Outline outline = cardObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.90f, 0.34f, 0.75f);
        outline.effectDistance = new Vector2(4f, -4f);

        CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
        Camera camera = GetCanvasCamera();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, startScreen, camera, out Vector2 start);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, endScreen, camera, out Vector2 end);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            Vector2 arc = Vector2.up * Mathf.Sin(t * Mathf.PI) * 42f;

            rect.anchoredPosition = Vector2.LerpUnclamped(start, end, eased) + arc;
            rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.10f);
            group.alpha = 1f;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.anchoredPosition = end;
        Destroy(cardObject);
    }

    private IEnumerator PulseRect(RectTransform rect, float delay = 0f)
    {
        if (rect == null)
        {
            yield break;
        }

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector3 originalScale = rect.localScale;
        float duration = 0.28f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.10f;
            rect.localScale = originalScale * pulse;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.localScale = originalScale;
    }

    private void RefreshSeatPanels()
    {
        if (seatPanels == null)
        {
            return;
        }

        List<PlayerData> players = gameManager.GetPlayers();
        int currentIndex = gameManager.GetCurrentPlayerIndex();

        for (int i = 0; i < seatPanels.Length; i++)
        {
            bool hasPlayer = i < players.Count;
            seatPanels[i].gameObject.SetActive(hasPlayer);

            if (!hasPlayer)
            {
                continue;
            }

            bool isCurrent = i == currentIndex;
            Image panelImage = seatPanels[i].GetComponent<Image>();
            panelImage.color = isCurrent
                ? new Color(0.98f, 0.64f, 0.12f, 0.90f)
                : new Color(0.02f, 0.06f, 0.08f, 0.78f);

            seatNameTexts[i].text = players[i].playerName;
            seatNameTexts[i].color = isCurrent ? Color.black : Color.white;
            seatCountTexts[i].text = players[i].handCards.Count + " cards";
            seatCountTexts[i].color = isCurrent ? Color.black : new Color(0.78f, 0.96f, 1f, 0.90f);
        }
    }

    private void OnCardClicked(int handIndex)
    {
        if (!gameManager.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn.");
            return;
        }

        if (!gameManager.CanPlayHandCard(handIndex))
        {
            Debug.Log("Card is not playable.");
            HideColorChoice();
            RefreshUI();
            return;
        }

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();
        if (handIndex < 0 || handIndex >= handCards.Count)
        {
            HideColorChoice();
            return;
        }

        CardData selectedCard = handCards[handIndex];
        if (gameManager.RequiresColorChoice(selectedCard))
        {
            ShowColorChoice(handIndex);
            return;
        }

        HideColorChoice();
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

        HideColorChoice();

        gameManager.DrawCard();

        RefreshUI();
    }

    private void OnUnoButtonClicked()
    {
        if (!gameManager.IsLocalPlayerTurn())
        {
            return;
        }

        gameManager.CallUno();
        RefreshUI();
    }

    private void OnRestartButtonClicked()
    {
        HideColorChoice();
        gameManager.StartOfflineGame();
        RefreshUI();
    }

    private void OnBackMenuClicked()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            StartCoroutine(LoadMenuAfterLeavingRoom());
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    private IEnumerator LoadMenuAfterLeavingRoom()
    {
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        SceneManager.LoadScene("MainMenuScene");
    }

    private void RefreshPlayerStatus()
    {
        List<PlayerData> players = gameManager.GetPlayers();
        int currentIndex = gameManager.GetCurrentPlayerIndex();

        string status = "<color=#FFD36B><b>PLAYERS</b></color>\n";

        for (int i = 0; i < players.Count; i++)
        {
            string prefix = i == currentIndex ? "> " : "  ";
            status += prefix + players[i].playerName + "   " + players[i].handCards.Count + "\n";
        }

        playerStatusText.text = status;
    }

    private void ShowColorChoice(int handIndex)
    {
        pendingWildCardIndex = handIndex;

        if (colorChoicePanel != null)
        {
            colorChoicePanel.transform.SetAsLastSibling();
            colorChoicePanel.SetActive(true);
        }
    }

    private void HideColorChoice()
    {
        pendingWildCardIndex = -1;

        if (colorChoicePanel != null)
        {
            colorChoicePanel.SetActive(false);
        }
    }

    private void OnColorChosen(CardColor color)
    {
        if (pendingWildCardIndex < 0)
        {
            return;
        }

        gameManager.PlayCard(pendingWildCardIndex, color);
        HideColorChoice();
        RefreshUI();
    }

    private void UpdateUnoButton(bool isLocalTurn, bool isGameOver, int handCount)
    {
        if (unoButton != null)
        {
            unoButton.interactable = isLocalTurn && !isGameOver && handCount == 2;
        }
    }

    private RectTransform CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return panelObject.GetComponent<RectTransform>();
    }

    private Image CreateImage(Transform parent, string name)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateLabel(Transform parent, string text, int fontSize, Color color)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        StyleText(label, fontSize, color, TextAlignmentOptions.Center, FontStyles.Normal);
        label.text = text;
        label.raycastTarget = false;
        return label;
    }

    private void StyleText(TMP_Text text, int fontSize, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.fontSizeMin = Mathf.Max(12, fontSize - 10);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
    }

    private void StyleButton(Button button, Color fill, Color textColor)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = fill;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.78f, 0.82f, 0.85f, 1f);
        colors.disabledColor = new Color(0.32f, 0.34f, 0.36f, 0.64f);
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        StyleText(text, 26, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
    }

    private void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2? pivot = null)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private Color GetUITextColor(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red:
                return new Color(1f, 0.20f, 0.18f);

            case CardColor.Blue:
                return new Color(0.20f, 0.52f, 1f);

            case CardColor.Green:
                return new Color(0.16f, 0.88f, 0.38f);

            case CardColor.Yellow:
                return new Color(1f, 0.82f, 0.18f);

            default:
                return Color.white;
        }
    }
}
