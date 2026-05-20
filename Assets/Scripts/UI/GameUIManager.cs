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
    private RectTransform currentColorBadge;
    private TMP_Text currentColorBadgeText;
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
    private RectTransform toastPanel;
    private TMP_Text toastTitleText;
    private TMP_Text toastBodyText;
    private CanvasGroup toastGroup;
    private Coroutine toastRoutine;
    private string lastToastMessage = "";
    private int lastAnnouncedTurnIndex = -1;
    private readonly Dictionary<string, Sprite> runtimeSprites = new Dictionary<string, Sprite>();

    private void OnEnable()
    {
        GameEvents.OnCardDrawn += HandleCardDrawnEvent;
        GameEvents.OnCardPlayed += HandleCardPlayedEvent;
        GameEvents.OnSpecialCardPlayed += HandleSpecialCardPlayedEvent;
        GameEvents.OnTurnChanged += HandleTurnChangedEvent;
        GameEvents.OnGameOver += HandleGameOverEvent;
    }

    private void OnDisable()
    {
        GameEvents.OnCardDrawn -= HandleCardDrawnEvent;
        GameEvents.OnCardPlayed -= HandleCardPlayedEvent;
        GameEvents.OnSpecialCardPlayed -= HandleSpecialCardPlayedEvent;
        GameEvents.OnTurnChanged -= HandleTurnChangedEvent;
        GameEvents.OnGameOver -= HandleGameOverEvent;
    }

    private void Start()
    {
        if (GameModeSelection.CurrentMode == GameMode.TienLenMienNam)
        {
            LaunchTienLenMode();
            enabled = false;
            return;
        }

        BuildRuntimeTheme();

        drawButton.onClick.AddListener(OnDrawButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        backMenuButton.onClick.AddListener(OnBackMenuClicked);

        gameOverPanel.SetActive(false);

        RefreshUI();
    }

    private void LaunchTienLenMode()
    {
        GameObject runtimeObject = new GameObject("TienLenRuntime");
        TienLenGameManager tienLenManager = runtimeObject.AddComponent<TienLenGameManager>();
        TienLenUIManager tienLenUI = runtimeObject.AddComponent<TienLenUIManager>();

        tienLenManager.StartGame();
        tienLenUI.Initialize(tienLenManager);
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

        ResetPileScales();

        bool isLocalTurn = gameManager.IsLocalPlayerTurn();
        bool isGameOver = gameManager.IsGameOver();

        currentTurnText.text = isLocalTurn
            ? "YOUR TURN - " + gameManager.GetCurrentPlayerName()
            : "TURN - " + gameManager.GetCurrentPlayerName();
        messageText.text = gameManager.GetLastMessage();
        MaybeShowMessageToast(messageText.text);

        bool pendingDrawPenalty = gameManager.HasPendingDrawPenalty();

        if (currentColorText != null)
        {
            currentColorText.text = pendingDrawPenalty
                ? "STACK " + gameManager.GetPendingDrawStackLabel().ToUpperInvariant() + " OR DRAW " + gameManager.GetPendingDrawPenalty() + " | COLOR " + gameManager.GetCurrentColor().ToString().ToUpperInvariant()
                : "COLOR " + gameManager.GetCurrentColor().ToString().ToUpperInvariant();
            currentColorText.color = pendingDrawPenalty ? new Color(1f, 0.78f, 0.22f) : GetUITextColor(gameManager.GetCurrentColor());
        }

        UpdateCurrentColorBadge(pendingDrawPenalty);

        SetButtonLabel(drawButton, pendingDrawPenalty ? "DRAW +" + gameManager.GetPendingDrawPenalty() : "DRAW");
        drawButton.interactable = isLocalTurn && !isGameOver;

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();
        AdjustHandLayout(handCards.Count);

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
            ConfigureHandCardObject(cardObject, handCards.Count, canPlayCard);

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
            gameOverPanel.transform.SetAsLastSibling();
            winnerText.text = gameManager.GetLastMessage();
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
        BuildTopHud(canvas.transform);
        BuildTopCardDisplay(canvas.transform);
        BuildSeatPanels(canvas.transform);
        BuildActionRail(canvas.transform);
        StyleExistingLayout();
        BuildUnoButton(canvas.transform);
        BuildColorChoicePanel(canvas.transform);
        BuildToastPanel(canvas.transform);
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
        RectTransform background = CreatePanel(canvasTransform, "Runtime_Background", Color.white);
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = GetVerticalGradientSprite("game_bg", new Color(0.01f, 0.04f, 0.05f), new Color(0.02f, 0.12f, 0.13f));
        background.SetAsFirstSibling();
        SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform frame = CreatePanel(canvasTransform, "Runtime_TableFrame", Color.white);
        Image frameImage = frame.GetComponent<Image>();
        frameImage.sprite = GetRoundedRectSprite("table_frame", 256, 128, 34, new Color(0.30f, 0.16f, 0.07f, 1f), new Color(0.82f, 0.55f, 0.24f, 1f), 7);
        frameImage.type = Image.Type.Sliced;
        frame.SetSiblingIndex(1);
        SetRect(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 38f), new Vector2(1030f, 416f));
        AddShadow(frame.gameObject, new Color(0f, 0f, 0f, 0.48f), new Vector2(0f, -10f));

        RectTransform table = CreatePanel(canvasTransform, "Runtime_TableSurface", Color.white);
        Image tableImage = table.GetComponent<Image>();
        tableImage.sprite = GetRoundedRectSprite("felt_surface", 256, 128, 28, new Color(0.02f, 0.32f, 0.25f, 0.97f), new Color(0.18f, 0.92f, 0.80f, 0.58f), 4);
        tableImage.type = Image.Type.Sliced;
        table.SetSiblingIndex(2);
        SetRect(table, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 38f), new Vector2(910f, 332f));

        Outline tableOutline = table.gameObject.AddComponent<Outline>();
        tableOutline.effectColor = new Color(0.20f, 0.95f, 0.85f, 0.28f);
        tableOutline.effectDistance = new Vector2(4f, -4f);
    }

    private void BuildTopHud(Transform canvasTransform)
    {
        RectTransform hud = CreatePanel(canvasTransform, "Runtime_TopHud", Color.white);
        Image hudImage = hud.GetComponent<Image>();
        hudImage.sprite = GetRoundedRectSprite("top_hud", 320, 108, 18, new Color(0.01f, 0.04f, 0.05f, 0.58f), new Color(0.20f, 0.95f, 0.86f, 0.20f), 2);
        hudImage.type = Image.Type.Sliced;
        hud.SetSiblingIndex(3);
        SetRect(hud, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(820f, 118f));
        AddShadow(hud.gameObject, new Color(0f, 0f, 0f, 0.28f), new Vector2(0f, -4f));
    }

    private void BuildActionRail(Transform canvasTransform)
    {
        RectTransform rail = CreatePanel(canvasTransform, "Runtime_ActionRail", Color.white);
        Image railImage = rail.GetComponent<Image>();
        railImage.sprite = GetRoundedRectSprite("action_rail", 180, 170, 20, new Color(0.01f, 0.04f, 0.05f, 0.54f), new Color(1f, 0.80f, 0.32f, 0.24f), 2);
        railImage.type = Image.Type.Sliced;
        SetRect(rail, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, -54f), new Vector2(184f, 160f));
        AddShadow(rail.gameObject, new Color(0f, 0f, 0f, 0.32f), new Vector2(0f, -5f));
    }

    private void BuildTopCardDisplay(Transform canvasTransform)
    {
        RectTransform topPanel = CreatePanel(canvasTransform, "Runtime_DiscardPanel", Color.white);
        Image topPanelImage = topPanel.GetComponent<Image>();
        topPanelImage.sprite = GetRoundedRectSprite("discard_panel", 256, 128, 18, new Color(0.01f, 0.04f, 0.05f, 0.82f), new Color(1f, 0.82f, 0.32f, 0.28f), 3);
        topPanelImage.type = Image.Type.Sliced;
        AddShadow(topPanel.gameObject, new Color(0f, 0f, 0f, 0.44f), new Vector2(0f, -6f));
        SetRect(topPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 38f), new Vector2(360f, 270f));

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

        currentColorBadge = CreatePanel(topPanel, "CurrentColorBadge", Color.white);
        Image badgeImage = currentColorBadge.GetComponent<Image>();
        badgeImage.sprite = GetRoundedRectSprite("current_color_badge", 256, 64, 16, new Color(0.08f, 0.40f, 0.92f, 0.96f), new Color(1f, 1f, 1f, 0.72f), 3);
        badgeImage.type = Image.Type.Sliced;
        SetRect(currentColorBadge, new Vector2(0.5f, 0.04f), new Vector2(0.5f, 0.04f), new Vector2(0f, 0f), new Vector2(270f, 38f));
        AddShadow(currentColorBadge.gameObject, new Color(0f, 0f, 0f, 0.40f), new Vector2(0f, -3f));

        currentColorBadgeText = CreateLabel(currentColorBadge, "CURRENT COLOR", 16, Color.white);
        currentColorBadgeText.fontStyle = FontStyles.Bold;
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
            new Vector2(0f, -214f),
            new Vector2(-610f, 52f),
            new Vector2(0f, 270f),
            new Vector2(610f, 52f)
        };

        for (int i = 0; i < seatPanels.Length; i++)
        {
            RectTransform panel = CreatePanel(canvasTransform, "Runtime_PlayerSeat_" + i, Color.white);
            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = GetRoundedRectSprite("seat_panel", 256, 96, 20, new Color(0.01f, 0.04f, 0.05f, 0.88f), new Color(0.22f, 0.92f, 0.82f, 0.30f), 2);
            panelImage.type = Image.Type.Sliced;
            AddShadow(panel.gameObject, new Color(0f, 0f, 0f, 0.38f), new Vector2(0f, -5f));
            SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[i], new Vector2(250f, 72f));

            Image avatar = CreateImage(panel, "Avatar");
            avatar.sprite = GetCircleSprite("avatar_disc", 96, new Color(0.10f, 0.38f, 0.42f, 1f), new Color(1f, 0.82f, 0.32f, 1f), 4);
            SetRect(avatar.rectTransform, new Vector2(0.04f, 0.18f), new Vector2(0.22f, 0.82f), Vector2.zero, Vector2.zero);

            TMP_Text avatarText = CreateLabel(avatar.transform, (i + 1).ToString(), 20, Color.white);
            avatarText.fontStyle = FontStyles.Bold;

            Image cardBack = CreateImage(panel, "CardBack");
            cardBack.preserveAspect = true;
            if (cardDatabase != null)
            {
                cardBack.sprite = cardDatabase.card_back;
            }
            SetRect(cardBack.rectTransform, new Vector2(0.78f, 0.12f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero);

            TMP_Text nameText = CreateLabel(panel, "Player", 18, Color.white);
            SetRect(nameText.rectTransform, new Vector2(0.26f, 0.50f), new Vector2(0.76f, 0.88f), Vector2.zero, Vector2.zero);
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.fontStyle = FontStyles.Bold;

            TMP_Text countText = CreateLabel(panel, "0 cards", 16, new Color(0.78f, 0.96f, 1f, 0.90f));
            SetRect(countText.rectTransform, new Vector2(0.26f, 0.14f), new Vector2(0.76f, 0.50f), Vector2.zero, Vector2.zero);
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
        SetRect(handRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(940f, 178f));

        Image handImage = handPanel.GetComponent<Image>();
        if (handImage == null)
        {
            handImage = handPanel.gameObject.AddComponent<Image>();
        }
        handImage.sprite = GetRoundedRectSprite("hand_tray", 256, 96, 22, new Color(0.01f, 0.03f, 0.04f, 0.68f), new Color(0.26f, 0.98f, 0.88f, 0.20f), 2);
        handImage.type = Image.Type.Sliced;
        handImage.color = Color.white;
        AddShadow(handPanel.gameObject, new Color(0f, 0f, 0f, 0.38f), new Vector2(0f, -5f));

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

        StyleButton(drawButton, new Color(1f, 0.82f, 0.12f, 1f), new Color(0.05f, 0.08f, 0.10f, 1f));
        StyleDrawButton();
        SetRect(drawButton.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, -18f), new Vector2(148f, 56f));

        StyleButton(restartButton, new Color(0.05f, 0.16f, 0.20f, 0.96f), Color.white);
        SetRect(restartButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -34f), new Vector2(144f, 48f));

        StyleButton(backMenuButton, new Color(0.05f, 0.16f, 0.20f, 0.96f), Color.white);
        SetRect(backMenuButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(92f, -34f), new Vector2(144f, 48f));
    }

    private void BuildUnoButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject("Runtime_UNOButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, -84f), new Vector2(148f, 56f), new Vector2(0.5f, 0.5f));

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetRoundedRectSprite("uno_button", 220, 84, 18, new Color(0.86f, 0.08f, 0.12f, 0.98f), new Color(1f, 0.72f, 0.26f, 0.80f), 4);
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        AddShadow(buttonObject, new Color(0f, 0f, 0f, 0.40f), new Vector2(0f, -4f));

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
        panelImage.sprite = GetRoundedRectSprite("color_choice_panel", 320, 140, 24, new Color(0.02f, 0.04f, 0.06f, 0.96f), new Color(1f, 0.82f, 0.36f, 0.50f), 4);
        panelImage.type = Image.Type.Sliced;
        panelImage.color = Color.white;
        AddShadow(colorChoicePanel, new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -8f));

        TMP_Text title = CreateLabel(colorChoicePanel.transform, "CHOOSE COLOR", 34, new Color(1f, 0.82f, 0.36f, 1f));
        SetRect(title.rectTransform, new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyles.Bold;

        CreateColorButton(colorChoicePanel.transform, CardColor.Red, "RED", new Vector2(-218f, -44f), new Color(0.90f, 0.08f, 0.12f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Blue, "BLUE", new Vector2(-72f, -44f), new Color(0.08f, 0.30f, 0.94f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Green, "GREEN", new Vector2(72f, -44f), new Color(0.02f, 0.62f, 0.30f), Color.white);
        CreateColorButton(colorChoicePanel.transform, CardColor.Yellow, "YELLOW", new Vector2(218f, -44f), new Color(0.98f, 0.76f, 0.08f), Color.black);

        colorChoicePanel.SetActive(false);
    }

    private void BuildToastPanel(Transform canvasTransform)
    {
        toastPanel = CreatePanel(canvasTransform, "Runtime_ToastPanel", Color.white);
        Image image = toastPanel.GetComponent<Image>();
        image.sprite = GetRoundedRectSprite("toast_panel", 320, 120, 22, new Color(0.01f, 0.04f, 0.05f, 0.94f), new Color(1f, 0.78f, 0.26f, 0.62f), 4);
        image.type = Image.Type.Sliced;
        SetRect(toastPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -164f), new Vector2(760f, 86f));
        AddShadow(toastPanel.gameObject, new Color(0f, 0f, 0f, 0.46f), new Vector2(0f, -6f));

        toastGroup = toastPanel.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
        toastGroup.interactable = false;

        toastTitleText = CreateLabel(toastPanel, "STATUS", 24, new Color(1f, 0.82f, 0.34f, 1f));
        SetRect(toastTitleText.rectTransform, new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
        toastTitleText.fontStyle = FontStyles.Bold;

        toastBodyText = CreateLabel(toastPanel, "", 19, Color.white);
        SetRect(toastBodyText.rectTransform, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.54f), Vector2.zero, Vector2.zero);
        toastBodyText.fontStyle = FontStyles.Normal;

        toastPanel.gameObject.SetActive(false);
    }

    private void CreateColorButton(Transform parent, CardColor color, string labelText, Vector2 position, Color fill, Color textColor)
    {
        GameObject buttonObject = new GameObject(labelText + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), position, new Vector2(124f, 70f));

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetRoundedRectSprite("color_button_" + labelText, 180, 96, 18, fill, new Color(1f, 1f, 1f, 0.76f), 4);
        image.type = Image.Type.Sliced;
        image.color = Color.white;

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
            image.sprite = GetRoundedRectSprite("game_over_panel", 360, 220, 28, new Color(0.02f, 0.04f, 0.06f, 0.97f), new Color(1f, 0.78f, 0.26f, 0.74f), 5);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        AddShadow(gameOverPanel, new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, -10f));

        TMP_Text title = CreateLabel(gameOverPanel.transform, "MATCH COMPLETE", 34, new Color(1f, 0.82f, 0.36f, 1f));
        SetRect(title.rectTransform, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.94f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyles.Bold;

        StyleText(winnerText, 36, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(winnerText.rectTransform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero);

        StyleButton(gameOverRestartButton, new Color(1f, 0.74f, 0.18f, 1f), Color.black);
        SetRect(gameOverRestartButton.transform as RectTransform, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(260f, 62f));
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

    private void UpdateCurrentColorBadge(bool pendingDrawPenalty)
    {
        if (currentColorBadge == null || currentColorBadgeText == null || gameManager == null)
        {
            return;
        }

        CardColor currentColor = gameManager.GetCurrentColor();
        Color fill = GetUITextColor(currentColor);
        Color textColor = currentColor == CardColor.Yellow ? new Color(0.04f, 0.04f, 0.04f, 1f) : Color.white;

        Image image = currentColorBadge.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedRectSprite(
                "current_color_badge_" + currentColor,
                256,
                64,
                16,
                fill,
                pendingDrawPenalty ? new Color(1f, 0.78f, 0.22f, 0.95f) : new Color(1f, 1f, 1f, 0.78f),
                pendingDrawPenalty ? 5 : 3);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        currentColorBadgeText.text = pendingDrawPenalty
            ? "COLOR " + currentColor.ToString().ToUpperInvariant() + "  |  +" + gameManager.GetPendingDrawPenalty()
            : "CURRENT COLOR: " + currentColor.ToString().ToUpperInvariant();
        currentColorBadgeText.color = textColor;
    }

    private void AdjustHandLayout(int handCount)
    {
        HorizontalLayoutGroup layout = handPanel != null ? handPanel.GetComponent<HorizontalLayoutGroup>() : null;
        if (layout == null)
        {
            return;
        }

        if (handCount > 13)
        {
            layout.spacing = 4f;
            layout.padding = new RectOffset(10, 10, 6, 6);
        }
        else if (handCount > 9)
        {
            layout.spacing = 6f;
            layout.padding = new RectOffset(12, 12, 6, 6);
        }
        else
        {
            layout.spacing = 10f;
            layout.padding = new RectOffset(16, 16, 6, 6);
        }
    }

    private void ConfigureHandCardObject(GameObject cardObject, int handCount, bool playable)
    {
        if (cardObject == null)
        {
            return;
        }

        RectTransform rect = cardObject.transform as RectTransform;
        LayoutElement layoutElement = cardObject.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = cardObject.AddComponent<LayoutElement>();
        }

        float spacing = handCount > 13 ? 4f : handCount > 9 ? 6f : 10f;
        float availableWidth = 900f - Mathf.Max(0, handCount - 1) * spacing;
        float width = handCount > 0 ? Mathf.Clamp(availableWidth / handCount, 54f, 102f) : 102f;

        float height = width * 1.48f;
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = height;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        if (rect != null)
        {
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = playable ? Vector3.one : Vector3.one * 0.96f;
        }

        Shadow shadow = cardObject.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = cardObject.AddComponent<Shadow>();
        }

        shadow.effectColor = playable ? new Color(0f, 0f, 0f, 0.54f) : new Color(0f, 0f, 0f, 0.30f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Outline outline = cardObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = cardObject.AddComponent<Outline>();
        }

        outline.effectColor = playable ? new Color(1f, 0.86f, 0.24f, 0.78f) : new Color(0f, 0f, 0f, 0.30f);
        outline.effectDistance = playable ? new Vector2(3f, -3f) : new Vector2(1f, -1f);

        SetInvalidCardOverlay(cardObject, !playable);
    }

    private void SetInvalidCardOverlay(GameObject cardObject, bool show)
    {
        if (cardObject == null)
        {
            return;
        }

        Transform existing = cardObject.transform.Find("Runtime_InvalidCardOverlay");
        GameObject overlayObject = existing != null ? existing.gameObject : null;

        if (overlayObject == null)
        {
            overlayObject = new GameObject("Runtime_InvalidCardOverlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(cardObject.transform, false);

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.sprite = GetRoundedRectSprite("invalid_card_overlay", 120, 180, 10, new Color(0f, 0f, 0f, 0.52f), new Color(0f, 0f, 0f, 0.12f), 1);
            overlayImage.type = Image.Type.Sliced;
            overlayImage.raycastTarget = false;

            TMP_Text mark = CreateLabel(overlayObject.transform, "LOCKED", 18, new Color(1f, 1f, 1f, 0.82f));
            mark.fontStyle = FontStyles.Bold;
            SetRect(mark.rectTransform, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.60f), Vector2.zero, Vector2.zero);
        }

        overlayObject.SetActive(show);
        overlayObject.transform.SetAsLastSibling();
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

        if (delay <= 0.001f)
        {
            RuntimeSfx.Play(RuntimeSfxType.Draw, 0.62f);
        }

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

        RuntimeSfx.Play(RuntimeSfxType.Play, 0.72f);
        StartCoroutine(AnimateCardSprite(sprite, start, end, new Vector2(142f, 210f), 0.50f));
        StartCoroutine(PulseRect(topCardImage.rectTransform, 0.35f));
    }

    private void HandleSpecialCardPlayedEvent(CardData card, int playerIndex)
    {
        if (!isActiveAndEnabled || card == null)
        {
            return;
        }

        BuildRuntimeTheme();
        if (canvasRect == null)
        {
            return;
        }

        string label = "";
        Color color = new Color(1f, 0.82f, 0.22f);

        switch (card.type)
        {
            case CardType.DrawTwo:
                label = "+2";
                color = new Color(1f, 0.72f, 0.16f);
                break;

            case CardType.DrawFour:
                label = "+4\n" + gameManager.GetCurrentColor().ToString().ToUpperInvariant();
                color = new Color(1f, 0.24f, 0.22f);
                break;

            case CardType.Block:
                label = "SKIP";
                color = new Color(0.45f, 0.86f, 1f);
                break;

            case CardType.Reverse:
                label = "REVERSE";
                color = new Color(0.50f, 1f, 0.62f);
                break;

            case CardType.ChangeColor:
                label = "WILD\n" + gameManager.GetCurrentColor().ToString().ToUpperInvariant();
                color = new Color(1f, 0.92f, 0.28f);
                break;
        }

        if (!string.IsNullOrEmpty(label))
        {
            RuntimeSfx.Play(RuntimeSfxType.Special, 0.82f);
            StartCoroutine(ShowCenterBurst(label, color));

            if (card.type == CardType.ChangeColor || card.type == CardType.DrawFour)
            {
                ShowToast(
                    "COLOR CHANGED",
                    GetPlayerName(playerIndex) + " changed color to " + gameManager.GetCurrentColor().ToString().ToUpperInvariant() + " with " + card.GetDisplayName() + ".",
                    GetUITextColor(gameManager.GetCurrentColor()),
                    2.4f);
            }
        }
    }

    private string GetPlayerName(int playerIndex)
    {
        if (gameManager == null)
        {
            return "Player " + (playerIndex + 1);
        }

        List<PlayerData> players = gameManager.GetPlayers();
        if (players != null && playerIndex >= 0 && playerIndex < players.Count)
        {
            return players[playerIndex].playerName;
        }

        return "Player " + (playerIndex + 1);
    }

    private void HandleTurnChangedEvent(int playerIndex)
    {
        if (!isActiveAndEnabled || playerIndex == lastAnnouncedTurnIndex)
        {
            return;
        }

        lastAnnouncedTurnIndex = playerIndex;
        BuildRuntimeTheme();
        RuntimeSfx.Play(RuntimeSfxType.Turn, 0.50f);

        if (gameManager != null)
        {
            ShowToast("TURN", gameManager.GetCurrentPlayerName(), new Color(0.18f, 0.95f, 0.86f, 1f));
        }
    }

    private void HandleGameOverEvent(string winner)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        BuildRuntimeTheme();
        RuntimeSfx.Play(RuntimeSfxType.Win, 0.92f);
        ShowToast("MATCH COMPLETE", string.IsNullOrEmpty(winner) ? "Game over" : winner, new Color(1f, 0.82f, 0.32f, 1f), 2.8f);
        StartCoroutine(ShowCenterBurst("GAME OVER", new Color(1f, 0.82f, 0.32f, 1f)));
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

        Vector3 originalScale = Vector3.one;
        rect.localScale = originalScale;
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

    private void ResetPileScales()
    {
        if (drawPileImage != null)
        {
            drawPileImage.rectTransform.localScale = Vector3.one;
        }

        if (topCardImage != null)
        {
            topCardImage.rectTransform.localScale = Vector3.one;
        }
    }

    private IEnumerator ShakeRect(RectTransform rect)
    {
        if (rect == null)
        {
            yield break;
        }

        Vector2 originalPosition = rect.anchoredPosition;
        float duration = 0.22f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float strength = Mathf.Lerp(10f, 0f, t);
            rect.anchoredPosition = originalPosition + new Vector2(Mathf.Sin(t * Mathf.PI * 8f) * strength, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.anchoredPosition = originalPosition;
    }

    private IEnumerator ShowCenterBurst(string label, Color color)
    {
        GameObject burstObject = new GameObject("Runtime_CenterBurst", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI));
        burstObject.transform.SetParent(canvasRect, false);
        burstObject.transform.SetAsLastSibling();

        RectTransform rect = burstObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(520f, 110f));

        TMP_Text text = burstObject.GetComponent<TMP_Text>();
        StyleText(text, 64, color, TextAlignmentOptions.Center, FontStyles.Bold);
        text.text = label;

        Outline outline = burstObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(4f, -4f);

        CanvasGroup group = burstObject.GetComponent<CanvasGroup>();
        float duration = 0.72f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localScale = Vector3.one * Mathf.Lerp(0.74f, 1.18f, 1f - Mathf.Pow(1f - t, 3f));
            rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(66f, 118f, t));
            group.alpha = t < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.72f) / 0.28f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Destroy(burstObject);
    }

    private void MaybeShowMessageToast(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message == lastToastMessage || toastPanel == null)
        {
            return;
        }

        lastToastMessage = message;

        string lower = message.ToLowerInvariant();
        string title = "STATUS";
        Color accent = new Color(0.18f, 0.95f, 0.86f, 1f);

        if (lower.Contains("cannot") || lower.Contains("invalid") || lower.Contains("choose a color"))
        {
            title = "CHECK MOVE";
            accent = new Color(1f, 0.28f, 0.22f, 1f);
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.70f);
        }
        else if (lower.Contains("draw") || lower.Contains("penalty"))
        {
            title = "DRAW";
            accent = new Color(1f, 0.74f, 0.18f, 1f);
        }
        else if (lower.Contains("played"))
        {
            title = "CARD PLAYED";
            accent = new Color(0.18f, 0.95f, 0.86f, 1f);
        }
        else if (lower.Contains("uno"))
        {
            title = "UNO";
            accent = new Color(0.92f, 0.08f, 0.12f, 1f);
        }
        else if (lower.Contains("wins") || lower.Contains("loses"))
        {
            title = "MATCH COMPLETE";
            accent = new Color(1f, 0.82f, 0.32f, 1f);
        }

        ShowToast(title, message, accent);
    }

    private void ShowToast(string title, string body, Color accent, float duration = 1.9f)
    {
        if (toastPanel == null || toastGroup == null)
        {
            return;
        }

        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        toastRoutine = StartCoroutine(ShowToastRoutine(title, body, accent, duration));
    }

    private IEnumerator ShowToastRoutine(string title, string body, Color accent, float holdDuration)
    {
        toastPanel.gameObject.SetActive(true);
        toastPanel.transform.SetAsLastSibling();

        Image image = toastPanel.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedRectSprite("toast_panel_" + ColorKey(accent), 320, 120, 22, new Color(0.01f, 0.04f, 0.05f, 0.94f), accent, 4);
            image.type = Image.Type.Sliced;
        }

        toastTitleText.text = title;
        toastTitleText.color = accent;
        toastBodyText.text = body;

        float fadeIn = 0.14f;
        float fadeOut = 0.22f;
        float elapsed = 0f;

        while (elapsed < fadeIn)
        {
            float t = elapsed / fadeIn;
            toastGroup.alpha = t;
            toastPanel.anchoredPosition = new Vector2(0f, Mathf.Lerp(-176f, -164f, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        toastGroup.alpha = 1f;
        toastPanel.anchoredPosition = new Vector2(0f, -164f);

        yield return new WaitForSecondsRealtime(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            float t = elapsed / fadeOut;
            toastGroup.alpha = 1f - t;
            toastPanel.anchoredPosition = new Vector2(0f, Mathf.Lerp(-164f, -182f, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        toastGroup.alpha = 0f;
        toastPanel.gameObject.SetActive(false);
        toastRoutine = null;
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

            PlayerData player = players[i];
            bool isActivePlayer = player.IsActive;
            bool isCurrent = isActivePlayer && i == currentIndex;
            Image panelImage = seatPanels[i].GetComponent<Image>();
            panelImage.sprite = isCurrent
                ? GetRoundedRectSprite("seat_panel_active", 256, 96, 20, new Color(0.96f, 0.61f, 0.10f, 0.96f), new Color(1f, 0.90f, 0.34f, 0.92f), 4)
                : GetRoundedRectSprite(
                    isActivePlayer ? "seat_panel" : "seat_panel_inactive",
                    256,
                    96,
                    20,
                    isActivePlayer ? new Color(0.01f, 0.04f, 0.05f, 0.88f) : new Color(0.03f, 0.04f, 0.05f, 0.72f),
                    isActivePlayer ? new Color(0.22f, 0.92f, 0.82f, 0.30f) : new Color(0.75f, 0.78f, 0.82f, 0.24f),
                    2);
            panelImage.color = Color.white;

            seatNameTexts[i].text = player.playerName;
            seatNameTexts[i].color = isCurrent ? Color.black : isActivePlayer ? Color.white : new Color(0.78f, 0.82f, 0.86f, 0.86f);
            seatCountTexts[i].text = GetSeatStatusText(player);
            seatCountTexts[i].color = isCurrent ? Color.black : isActivePlayer ? new Color(0.78f, 0.96f, 1f, 0.90f) : new Color(0.98f, 0.82f, 0.40f, 0.88f);
            seatPanels[i].localScale = isCurrent ? Vector3.one * 1.04f : Vector3.one;

            if (seatCardImages[i] != null)
            {
                seatCardImages[i].color = isActivePlayer ? Color.white : new Color(1f, 1f, 1f, 0.42f);
            }
        }
    }

    private string GetSeatStatusText(PlayerData player)
    {
        if (player.hasFinished)
        {
            return player.finishRank > 0 ? "FINISHED #" + player.finishRank : "FINISHED";
        }

        if (player.isLastPlace)
        {
            return "LAST PLACE";
        }

        if (player.isEliminated)
        {
            return "OUT";
        }

        return player.handCards.Count + " cards";
    }

    private void OnCardClicked(int handIndex)
    {
        if (!gameManager.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn.");
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.68f);
            ShowToast("WAIT", "It is " + gameManager.GetCurrentPlayerName() + "'s turn.", new Color(1f, 0.74f, 0.18f, 1f));
            return;
        }

        if (!gameManager.CanPlayHandCard(handIndex))
        {
            Debug.Log("Card is not playable.");
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.72f);
            string invalidReason = gameManager.HasPendingDrawPenalty()
                ? "A +" + gameManager.GetPendingDrawPenalty() + " stack is active. Play +2/+4 or press DRAW +" + gameManager.GetPendingDrawPenalty() + "."
                : "Match color, number, symbol, Wild, or draw a card.";
            ShowToast("INVALID CARD", invalidReason, new Color(1f, 0.28f, 0.22f, 1f));

            if (handPanel != null && handIndex >= 0 && handIndex < handPanel.childCount)
            {
                StartCoroutine(ShakeRect(handPanel.GetChild(handIndex) as RectTransform));
            }

            HideColorChoice();
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
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.68f);
            ShowToast("WAIT", "It is " + gameManager.GetCurrentPlayerName() + "'s turn.", new Color(1f, 0.74f, 0.18f, 1f));
            return;
        }

        RuntimeSfx.Play(RuntimeSfxType.Click, 0.76f);
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

        RuntimeSfx.Play(RuntimeSfxType.Uno, 0.90f);
        gameManager.CallUno();
        RefreshUI();
    }

    private void OnRestartButtonClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        HideColorChoice();
        gameManager.StartOfflineGame();
        RefreshUI();
    }

    private void OnBackMenuClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);

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
            RuntimeSfx.Play(RuntimeSfxType.Special, 0.58f);
            ShowToast("CHOOSE COLOR", "Pick the next match color.", new Color(1f, 0.82f, 0.32f, 1f));
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
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.72f);
        HideColorChoice();
        RefreshUI();
    }

    private void UpdateUnoButton(bool isLocalTurn, bool isGameOver, int handCount)
    {
        if (unoButton != null)
        {
            unoButton.interactable = isLocalTurn && !isGameOver && handCount == 2;
            Image image = unoButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = unoButton.interactable ? Color.white : new Color(1f, 1f, 1f, 0.72f);
            }
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
            image.sprite = GetRoundedRectSprite("button_" + ColorKey(fill), 220, 84, 18, fill, new Color(1f, 0.95f, 0.78f, 0.52f), 3);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        AddShadow(button.gameObject, new Color(0f, 0f, 0f, 0.42f), new Vector2(0f, -4f));

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.78f, 0.82f, 0.85f, 1f);
        colors.disabledColor = new Color(0.32f, 0.34f, 0.36f, 0.64f);
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        StyleText(text, 26, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
    }

    private void StyleDrawButton()
    {
        if (drawButton == null)
        {
            return;
        }

        Image image = drawButton.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedRectSprite("draw_button_bright", 240, 90, 20, new Color(1f, 0.84f, 0.10f, 1f), new Color(1f, 0.98f, 0.72f, 1f), 5);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        Outline outline = drawButton.GetComponent<Outline>();
        if (outline == null)
        {
            outline = drawButton.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = new Color(1f, 0.95f, 0.48f, 0.62f);
        outline.effectDistance = new Vector2(3f, -3f);

        ColorBlock colors = drawButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.86f, 1f);
        colors.pressedColor = new Color(1f, 0.70f, 0.10f, 1f);
        colors.disabledColor = new Color(1f, 0.78f, 0.25f, 0.58f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        drawButton.colors = colors;

        TMP_Text text = drawButton.GetComponentInChildren<TMP_Text>(true);
        StyleText(text, 28, new Color(0.03f, 0.04f, 0.05f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
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

    private void AddShadow(GameObject target, Color color, Vector2 distance)
    {
        if (target == null)
        {
            return;
        }

        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = target.AddComponent<Shadow>();
        }

        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    private Sprite GetVerticalGradientSprite(string key, Color top, Color bottom)
    {
        string spriteKey = key + "_" + ColorKey(top) + "_" + ColorKey(bottom);
        if (runtimeSprites.TryGetValue(spriteKey, out Sprite sprite))
        {
            return sprite;
        }

        int width = 16;
        int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            Color color = Color.Lerp(bottom, top, y / (height - 1f));
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        runtimeSprites[spriteKey] = sprite;
        return sprite;
    }

    private Sprite GetRoundedRectSprite(string key, int width, int height, int radius, Color fill, Color border, int borderWidth)
    {
        string spriteKey = key + "_" + width + "x" + height + "_" + radius + "_" + ColorKey(fill) + "_" + ColorKey(border) + "_" + borderWidth;
        if (runtimeSprites.TryGetValue(spriteKey, out Sprite sprite))
        {
            return sprite;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                if (!inside)
                {
                    texture.SetPixel(x, y, clear);
                    continue;
                }

                bool inner = borderWidth <= 0 || IsInsideRoundedRect(
                    x - borderWidth,
                    y - borderWidth,
                    width - borderWidth * 2,
                    height - borderWidth * 2,
                    Mathf.Max(1, radius - borderWidth));

                texture.SetPixel(x, y, inner ? fill : border);
            }
        }

        texture.Apply();
        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        runtimeSprites[spriteKey] = sprite;
        return sprite;
    }

    private Sprite GetCircleSprite(string key, int size, Color fill, Color border, int borderWidth)
    {
        string spriteKey = key + "_" + size + "_" + ColorKey(fill) + "_" + ColorKey(border) + "_" + borderWidth;
        if (runtimeSprites.TryGetValue(spriteKey, out Sprite sprite))
        {
            return sprite;
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f - 1f;
        float innerRadius = Mathf.Max(0f, radius - borderWidth);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance > radius)
                {
                    texture.SetPixel(x, y, clear);
                }
                else
                {
                    texture.SetPixel(x, y, distance > innerRadius ? border : fill);
                }
            }
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        runtimeSprites[spriteKey] = sprite;
        return sprite;
    }

    private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        int clampedRadius = Mathf.Min(radius, Mathf.Min(width, height) / 2);
        int cx = Mathf.Clamp(x, clampedRadius, width - clampedRadius - 1);
        int cy = Mathf.Clamp(y, clampedRadius, height - clampedRadius - 1);
        int dx = x - cx;
        int dy = y - cy;
        return dx * dx + dy * dy <= clampedRadius * clampedRadius;
    }

    private string ColorKey(Color color)
    {
        return Mathf.RoundToInt(color.r * 255f) + "_" +
               Mathf.RoundToInt(color.g * 255f) + "_" +
               Mathf.RoundToInt(color.b * 255f) + "_" +
               Mathf.RoundToInt(color.a * 255f);
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
