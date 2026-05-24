using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public Button playOfflineButton;
    public Button multiplayerButton;
    public Button quitButton;

    private bool themeBuilt;
    private static CardSpriteDatabase cardDatabase;
    private static TienLenThemeAssetDatabase tienLenThemeAssets;
    private Button tienLenButton;
    private Button rulesButton;
    private Button settingsButton;
    private RectTransform rulesPanel;
    private RuntimePauseMenu settingsPanel;

    private void Start()
    {
        BuildRuntimeTheme();
        ApplyMainMenuButtonInteractions();

        playOfflineButton.onClick.AddListener(OnPlayOfflineClicked);
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log("MainMenuManager started");
    }

    private void OnPlayOfflineClicked()
    {
        Debug.Log("Play Offline clicked");
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        GameModeSelection.CurrentMode = GameMode.Uno;
        SceneManager.LoadScene("GameScene");
    }

    private void OnTienLenClicked()
    {
        Debug.Log("Tien Len clicked");
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        GameModeSelection.CurrentMode = GameMode.TienLenMienNam;
        SceneManager.LoadScene("GameScene");
    }

    private void OnMultiplayerClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        GameModeSelection.CurrentMode = GameMode.Uno;
        SceneManager.LoadScene("LobbyScene");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        Application.Quit();
    }

    private void OnRulesClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        ShowRulesPanel(true);
    }

    private void OnSettingsClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        ShowSettingsPanel(true);
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

        cardDatabase = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
        tienLenThemeAssets = Resources.Load<TienLenThemeAssetDatabase>("TienLen/TienLenThemeAssetDatabase");
        ConfigureCanvas(canvas);
        BuildBackground(canvas.transform);
        BuildCardPreview(canvas.transform);
        StyleTitle(canvas.transform);
        StyleMenuButtons(canvas.transform);
        BuildRulesPanel(canvas.transform);
        BuildSettingsPanel(canvas.transform);

        themeBuilt = true;
    }

    private Canvas ResolveCanvas()
    {
        if (playOfflineButton != null)
        {
            Canvas canvas = playOfflineButton.GetComponentInParent<Canvas>();
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
            RuntimeUITheme.ConfigureCanvas(canvas);
        }
    }

    private void BuildBackground(Transform canvasTransform)
    {
        Texture2D menuBackgroundTexture = tienLenThemeAssets == null ? null : tienLenThemeAssets.menuBackgroundTexture;
        if (menuBackgroundTexture == null && tienLenThemeAssets != null)
        {
            menuBackgroundTexture = tienLenThemeAssets.backgroundTexture;
        }

        Sprite backgroundSprite = RuntimeUITheme.GetTextureSprite(menuBackgroundTexture, "menu_luxury_background");
        if (backgroundSprite != null)
        {
            Image backgroundImage = RuntimeUITheme.CreateImage(canvasTransform, "Runtime_MenuBackgroundArt", backgroundSprite);
            backgroundImage.preserveAspect = false;
            backgroundImage.color = new Color(0.72f, 0.82f, 0.84f, 1f);
            SetRect(backgroundImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(360f, 360f));
            backgroundImage.transform.SetAsFirstSibling();
        }
        else
        {
            RectTransform background = RuntimeUITheme.CreateGradient(canvasTransform, "Runtime_MenuBackground", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.13f, 0.14f, 1f));
            background.SetAsFirstSibling();
            SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(360f, 360f));
        }

        RectTransform vignette = RuntimeUITheme.CreateGradient(canvasTransform, "Runtime_MenuVignette", new Color(0.01f, 0.02f, 0.02f, 0.32f), new Color(0f, 0f, 0f, 0.72f));
        vignette.SetSiblingIndex(1);
        SetRect(vignette, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(360f, 360f));

        Texture2D menuTableTexture = tienLenThemeAssets == null ? null : tienLenThemeAssets.menuTableTexture;
        if (menuTableTexture == null && tienLenThemeAssets != null)
        {
            menuTableTexture = tienLenThemeAssets.tableTexture;
        }

        Sprite tableSprite = RuntimeUITheme.GetTextureSprite(menuTableTexture, "menu_luxury_table");
        if (tableSprite != null)
        {
            Image tableArt = RuntimeUITheme.CreateImage(canvasTransform, "Runtime_MenuTableArt", tableSprite);
            tableArt.preserveAspect = true;
            SetRect(tableArt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -74f), new Vector2(1050f, 490f));
            tableArt.transform.SetSiblingIndex(2);
            AddShadow(tableArt.gameObject, new Color(0f, 0f, 0f, 0.58f), new Vector2(0f, -12f));
        }
        else
        {
            RectTransform tableGlow = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_MenuTableGlow", new Color(0.02f, 0.29f, 0.23f, 0.86f), new Color(0.18f, 0.95f, 0.86f, 0.42f), 34, 4);
            tableGlow.SetSiblingIndex(2);
            SetRect(tableGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1040f, 440f));
            AddShadow(tableGlow.gameObject, new Color(0f, 0f, 0f, 0.42f), new Vector2(0f, -10f));

            Outline outline = tableGlow.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 1f, 0.82f, 0.24f);
            outline.effectDistance = new Vector2(5f, -5f);
        }

        RectTransform actionPanel = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_MenuActionPanel", new Color(0.01f, 0.04f, 0.05f, 0.92f), new Color(1f, 0.78f, 0.28f, 0.52f), 26, 3);
        actionPanel.SetSiblingIndex(3);
        SetRect(actionPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -76f), new Vector2(500f, 444f));
        AddShadow(actionPanel.gameObject, new Color(0f, 0f, 0f, 0.46f), new Vector2(0f, -8f));
    }

    private void BuildCardPreview(Transform canvasTransform)
    {
        Sprite[] sprites =
        {
            cardDatabase == null ? null : cardDatabase.card_back,
            cardDatabase == null ? null : cardDatabase.wild_change_color,
            TienLenCardSpriteDatabase.GetSprite(new PlayingCardData(0, 14, PlayingCardSuit.Spades)),
            TienLenCardSpriteDatabase.GetSprite(new PlayingCardData(0, 2, PlayingCardSuit.Hearts))
        };

        Vector2[] positions =
        {
            new Vector2(-450f, -78f),
            new Vector2(-350f, -42f),
            new Vector2(350f, -58f),
            new Vector2(468f, -72f)
        };

        float[] rotations = { -12f, 8f, -8f, 10f };

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
            {
                continue;
            }

            Image image = CreateImage(canvasTransform, "Runtime_MenuCard_" + i);
            image.sprite = sprites[i];
            image.preserveAspect = i < 2;
            image.raycastTarget = false;
            Vector2 size = new Vector2(150f, 220f);
            SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[i], size);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotations[i]);
            AddShadow(image.gameObject, new Color(0f, 0f, 0f, 0.50f), new Vector2(0f, -8f));
            image.transform.SetSiblingIndex(3 + i);
        }
    }

    private void StyleTitle(Transform canvasTransform)
    {
        TMP_Text title = null;
        TMP_Text[] texts = canvasTransform.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text.text == "Board Game AR")
            {
                title = text;
                break;
            }
        }

        if (title == null)
        {
            title = CreateLabel(canvasTransform, "Board Game AR", 56, Color.white);
        }

        StyleText(title, 58, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(900f, 82f));
        AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, -5f));
        title.transform.SetAsLastSibling();
    }

    private void StyleMenuButtons(Transform canvasTransform)
    {
        if (tienLenButton == null)
        {
            tienLenButton = CreateRuntimeButton(canvasTransform, "Runtime_TienLenButton", "Tien Len Mien Nam");
            tienLenButton.onClick.AddListener(OnTienLenClicked);
        }

        if (rulesButton == null)
        {
            rulesButton = CreateRuntimeButton(canvasTransform, "Runtime_RulesButton", "Rules");
            rulesButton.onClick.AddListener(OnRulesClicked);
        }

        if (settingsButton == null)
        {
            settingsButton = CreateRuntimeButton(canvasTransform, "Runtime_SettingsButton", "Settings");
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        StyleButton(playOfflineButton, new Color(1f, 0.76f, 0.18f, 1f), new Color(0.03f, 0.04f, 0.05f, 1f), "UNO Offline");
        StyleButton(tienLenButton, new Color(0.04f, 0.56f, 0.34f, 1f), Color.white, "Tien Len Mien Nam");
        StyleButton(multiplayerButton, new Color(0.08f, 0.48f, 0.92f, 1f), Color.white, "Multiplayer");
        StyleButton(rulesButton, new Color(0.64f, 0.12f, 0.12f, 1f), Color.white, "Rules");
        StyleButton(settingsButton, new Color(0.38f, 0.20f, 0.78f, 1f), Color.white, "Settings");
        StyleButton(quitButton, new Color(0.12f, 0.18f, 0.20f, 0.98f), Color.white, "Quit");

        SetRect(playOfflineButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 69f), new Vector2(360f, 50f));
        SetRect(tienLenButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 11f), new Vector2(360f, 50f));
        SetRect(multiplayerButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -47f), new Vector2(360f, 50f));
        SetRect(rulesButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -105f), new Vector2(360f, 50f));
        SetRect(settingsButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -163f), new Vector2(360f, 50f));
        SetRect(quitButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -221f), new Vector2(360f, 50f));

        playOfflineButton.transform.SetAsLastSibling();
        tienLenButton.transform.SetAsLastSibling();
        multiplayerButton.transform.SetAsLastSibling();
        rulesButton.transform.SetAsLastSibling();
        settingsButton.transform.SetAsLastSibling();
        quitButton.transform.SetAsLastSibling();
    }

    private void ApplyMainMenuButtonInteractions()
    {
        AttachMainMenuButtonFx(playOfflineButton);
        AttachMainMenuButtonFx(tienLenButton);
        AttachMainMenuButtonFx(multiplayerButton);
        AttachMainMenuButtonFx(rulesButton);
        AttachMainMenuButtonFx(settingsButton);
        AttachMainMenuButtonFx(quitButton);
    }

    private void AttachMainMenuButtonFx(Button button)
    {
        if (button == null)
        {
            return;
        }

        MainMenuButtonFx fx = button.GetComponent<MainMenuButtonFx>();
        if (fx == null)
        {
            fx = button.gameObject.AddComponent<MainMenuButtonFx>();
        }

        fx.Configure(1.05f, 0.95f);
    }

    private void BuildRulesPanel(Transform canvasTransform)
    {
        if (rulesPanel != null)
        {
            return;
        }

        rulesPanel = RuntimeUITheme.CreatePanel(
            canvasTransform,
            "Runtime_RulesPanel",
            new Color(0.01f, 0.04f, 0.05f, 0.97f),
            new Color(1f, 0.78f, 0.28f, 0.70f),
            28,
            4);
        SetRect(rulesPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(1180f, 780f));
        AddShadow(rulesPanel.gameObject, new Color(0f, 0f, 0f, 0.68f), new Vector2(0f, -12f));

        TMP_Text title = CreateLabel(rulesPanel, "RULES", 36, RuntimeUITheme.Gold);
        SetRect(title.rectTransform, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);

        TMP_Text unoRules = CreateLabel(rulesPanel, BuildUnoRulesText(), 22, Color.white);
        unoRules.alignment = TextAlignmentOptions.TopLeft;
        unoRules.fontStyle = FontStyles.Normal;
        unoRules.lineSpacing = -8f;
        SetRect(unoRules.rectTransform, new Vector2(0.06f, 0.16f), new Vector2(0.48f, 0.86f), Vector2.zero, Vector2.zero);

        TMP_Text tienLenRules = CreateLabel(rulesPanel, BuildTienLenRulesText(), 22, Color.white);
        tienLenRules.alignment = TextAlignmentOptions.TopLeft;
        tienLenRules.fontStyle = FontStyles.Normal;
        tienLenRules.lineSpacing = -8f;
        SetRect(tienLenRules.rectTransform, new Vector2(0.52f, 0.16f), new Vector2(0.94f, 0.86f), Vector2.zero, Vector2.zero);

        Button closeButton = CreateRuntimeButton(rulesPanel, "Runtime_CloseRulesButton", "Close");
        StyleButton(closeButton, RuntimeUITheme.Gold, RuntimeUITheme.Ink, "Close");
        SetRect(closeButton.transform as RectTransform, new Vector2(0.5f, 0.04f), new Vector2(0.5f, 0.04f), Vector2.zero, new Vector2(260f, 56f));
        closeButton.onClick.AddListener(() =>
        {
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
            ShowRulesPanel(false);
        });

        rulesPanel.gameObject.SetActive(false);
    }

    private void BuildSettingsPanel(Transform canvasTransform)
    {
        if (settingsPanel != null)
        {
            return;
        }

        settingsPanel = new RuntimePauseMenu();
        settingsPanel.BuildSettings(canvasTransform, "MainMenuSettings", () => ShowSettingsPanel(false));
    }

    private string BuildUnoRulesText()
    {
        return "<b>UNO OFFLINE</b>\n\n" +
            "- Match color, number, symbol, or play a Wild.\n" +
            "- Wild and +4 may be played freely, then choose a color.\n" +
            "- +2 and +4 stack together. The next player must stack another + card or draw the full penalty.\n" +
            "- If a player cannot stack, Draw takes all stacked penalty cards and loses the turn.\n" +
            "- If there is no stack and a player cannot play, Draw one card and lose the turn.\n" +
            "- Skip skips the next player. Reverse changes direction.\n" +
            "- Press UNO before playing your second-to-last card. Forgetting UNO draws 2 cards.\n" +
            "- If the draw deck runs out, the discard pile is shuffled into a new deck.\n" +
            "- A player with more than 25 cards loses.";
    }

    private string BuildTienLenRulesText()
    {
        return "<b>TIEN LEN MIEN NAM</b>\n\n" +
            "- Four players use a 52-card deck. Rank order from low to high: 3, 4, ... A, 2.\n" +
            "- Suit order for ties from low to high: Spades, Clubs, Diamonds, Hearts.\n" +
            "- The player holding 3 of Spades starts, but may lead any valid set.\n" +
            "- Valid sets: single, pair, triple, straight, four-kind, and consecutive pairs.\n" +
            "- Beat with the same type and same card count at higher value, unless it is a chop.\n" +
            "- Straights compare by highest card. 2 cannot be used in normal straights or consecutive pairs.\n" +
            "- Three consecutive pairs can chop one 2.\n" +
            "- Four consecutive pairs can chop one 2, a pair of 2s, or one four-kind.\n" +
            "- Four-kind can chop one 2 or a pair of 2s. Four-kind can beat lower four-kind.\n" +
            "- Consecutive pairs beat only the same length if not chopping.\n" +
            "- Finishing with 2 is allowed. Three 2s may be played.\n" +
            "- Instant win: four 2s, all red/all black 13-card hand, or 13-rank straight A-2-3-...-K.\n" +
            "- When a player finishes, the remaining players continue until ranking is complete.";
    }

    private void ShowRulesPanel(bool visible)
    {
        if (rulesPanel == null)
        {
            return;
        }

        rulesPanel.gameObject.SetActive(visible);
        if (visible)
        {
            rulesPanel.SetAsLastSibling();
        }
    }

    private void ShowSettingsPanel(bool visible)
    {
        if (settingsPanel == null)
        {
            return;
        }

        if (visible)
        {
            settingsPanel.Show();
        }
        else
        {
            settingsPanel.Hide();
        }
    }

    private Button CreateRuntimeButton(Transform parent, string name, string label)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        SetRect(labelObject.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        TMP_Text text = labelObject.GetComponent<TMP_Text>();
        text.text = label;
        StyleText(text, 27, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);

        return buttonObject.GetComponent<Button>();
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
        GameObject labelObject = new GameObject("Runtime_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        StyleText(label, fontSize, color, TextAlignmentOptions.Center, FontStyles.Bold);
        label.text = text;
        label.raycastTarget = false;
        return label;
    }

    private void StyleButton(Button button, Color fill, Color textColor, string labelText)
    {
        if (button == null)
        {
            return;
        }

        RuntimeUITheme.StyleButton(button, fill, textColor, labelText);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = labelText;
            StyleText(text, 27, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        }
    }

    private void StyleText(TMP_Text text, int fontSize, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.fontSizeMin = Mathf.Max(14, fontSize - 14);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
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

    private sealed class MainMenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const float ScaleDuration = 0.08f;
        private const float ClickReturnDuration = 0.10f;

        private RectTransform target;
        private TMP_Text label;
        private Image background;
        private Vector3 baseScale;
        private Color baseColor;
        private Color baseBackgroundColor;
        private float hoverScale = 1.05f;
        private float downScale = 0.95f;
        private bool isPointerOver;
        private Coroutine scaleRoutine;
        private const float BrightenAmount = 0.18f;
        private static readonly Color HoverLightTextColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        private static readonly Color HoverDarkTextColor = new Color(1f, 0.85f, 0.4f, 1f);

        public void Configure(float newHoverScale, float newDownScale)
        {
            hoverScale = newHoverScale;
            downScale = newDownScale;
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (label == null)
            {
                label = GetComponentInChildren<TMP_Text>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (target != null)
            {
                baseScale = target.localScale;
            }

            if (label != null)
            {
                baseColor = label.color;
            }

            if (background != null)
            {
                baseBackgroundColor = background.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            AnimateScale(hoverScale);
            SetLabelColor(GetHoverTextColor());
            SetBackgroundColor(BrightenColor(baseBackgroundColor));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            AnimateScale(1f);
            SetLabelColor(baseColor);
            SetBackgroundColor(baseBackgroundColor);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateScale(downScale);
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ClickPulse());
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateScale(isPointerOver ? hoverScale : 1f);
        }

        private void SetLabelColor(Color color)
        {
            if (label == null)
            {
                return;
            }

            label.color = color;
        }

        private void SetBackgroundColor(Color color)
        {
            if (background == null)
            {
                return;
            }

            background.color = color;
        }

        private Color GetHoverTextColor()
        {
            if (IsLightColor(baseBackgroundColor))
            {
                return HoverLightTextColor;
            }

            return HoverDarkTextColor;
        }

        private bool IsLightColor(Color color)
        {
            float luminance = (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
            return luminance >= 0.62f;
        }

        private Color BrightenColor(Color color)
        {
            Color bright = Color.Lerp(color, Color.white, BrightenAmount);
            bright.a = color.a;
            return bright;
        }

        private void AnimateScale(float scaleMultiplier)
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetScale = baseScale * scaleMultiplier;
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(ScaleRoutine(targetScale, ScaleDuration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 start = target.localScale;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                target.localScale = Vector3.Lerp(start, targetScale, t);
                yield return null;
            }

            target.localScale = targetScale;
        }

        private IEnumerator ClickPulse()
        {
            yield return new WaitForSecondsRealtime(0.06f);
            float returnScale = isPointerOver ? hoverScale : 1f;

            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(ScaleRoutine(baseScale * returnScale, ClickReturnDuration));
        }
    }
}
