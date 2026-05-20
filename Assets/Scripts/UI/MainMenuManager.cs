using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public Button playOfflineButton;
    public Button multiplayerButton;
    public Button quitButton;

    private bool themeBuilt;
    private static CardSpriteDatabase cardDatabase;

    private void Start()
    {
        BuildRuntimeTheme();

        playOfflineButton.onClick.AddListener(OnPlayOfflineClicked);
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log("MainMenuManager started");
    }

    private void OnPlayOfflineClicked()
    {
        Debug.Log("Play Offline clicked");
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        SceneManager.LoadScene("GameScene");
    }

    private void OnMultiplayerClicked()
    {
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        SceneManager.LoadScene("LobbyScene");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        Application.Quit();
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
        ConfigureCanvas(canvas);
        BuildBackground(canvas.transform);
        BuildCardPreview(canvas.transform);
        StyleTitle(canvas.transform);
        StyleMenuButtons(canvas.transform);

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
        RectTransform background = RuntimeUITheme.CreateGradient(canvasTransform, "Runtime_MenuBackground", new Color(0.01f, 0.03f, 0.04f, 1f), new Color(0.03f, 0.13f, 0.14f, 1f));
        background.SetAsFirstSibling();
        SetRect(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform tableGlow = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_MenuTableGlow", new Color(0.02f, 0.29f, 0.23f, 0.86f), new Color(0.18f, 0.95f, 0.86f, 0.42f), 34, 4);
        tableGlow.SetSiblingIndex(1);
        SetRect(tableGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1040f, 440f));
        AddShadow(tableGlow.gameObject, new Color(0f, 0f, 0f, 0.42f), new Vector2(0f, -10f));

        Outline outline = tableGlow.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 1f, 0.82f, 0.24f);
        outline.effectDistance = new Vector2(5f, -5f);

        RectTransform actionPanel = RuntimeUITheme.CreatePanel(canvasTransform, "Runtime_MenuActionPanel", new Color(0.01f, 0.04f, 0.05f, 0.88f), new Color(1f, 0.78f, 0.28f, 0.38f), 26, 3);
        actionPanel.SetSiblingIndex(2);
        SetRect(actionPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -76f), new Vector2(500f, 322f));
        AddShadow(actionPanel.gameObject, new Color(0f, 0f, 0f, 0.46f), new Vector2(0f, -8f));
    }

    private void BuildCardPreview(Transform canvasTransform)
    {
        if (cardDatabase == null)
        {
            return;
        }

        Sprite[] sprites =
        {
            cardDatabase.card_back,
            cardDatabase.wild_change_color,
            cardDatabase.red_2plus,
            cardDatabase.wild_plus4
        };

        Vector2[] positions =
        {
            new Vector2(-450f, -78f),
            new Vector2(-350f, -42f),
            new Vector2(350f, -42f),
            new Vector2(450f, -78f)
        };

        float[] rotations = { -12f, 8f, -8f, 12f };

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
            {
                continue;
            }

            Image image = CreateImage(canvasTransform, "Runtime_MenuCard_" + i);
            image.sprite = sprites[i];
            image.preserveAspect = true;
            image.raycastTarget = false;
            SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[i], new Vector2(150f, 220f));
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
        StyleButton(playOfflineButton, new Color(1f, 0.76f, 0.18f, 1f), new Color(0.03f, 0.04f, 0.05f, 1f), "Play Offline");
        StyleButton(multiplayerButton, new Color(0.08f, 0.48f, 0.92f, 1f), Color.white, "Multiplayer");
        StyleButton(quitButton, new Color(0.12f, 0.18f, 0.20f, 0.98f), Color.white, "Quit");

        SetRect(playOfflineButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(360f, 66f));
        SetRect(multiplayerButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -76f), new Vector2(360f, 66f));
        SetRect(quitButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -158f), new Vector2(360f, 66f));

        playOfflineButton.transform.SetAsLastSibling();
        multiplayerButton.transform.SetAsLastSibling();
        quitButton.transform.SetAsLastSibling();
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
}
