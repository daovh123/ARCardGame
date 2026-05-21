using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RuntimeUITheme
{
    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

    public static readonly Color Ink = new Color(0.01f, 0.03f, 0.04f, 1f);
    public static readonly Color Panel = new Color(0.02f, 0.06f, 0.08f, 0.94f);
    public static readonly Color PanelSoft = new Color(0.02f, 0.11f, 0.12f, 0.84f);
    public static readonly Color Felt = new Color(0.02f, 0.32f, 0.25f, 0.96f);
    public static readonly Color Gold = new Color(1f, 0.76f, 0.24f, 1f);
    public static readonly Color Cyan = new Color(0.18f, 0.95f, 0.86f, 1f);
    public static readonly Color Red = new Color(0.86f, 0.08f, 0.12f, 1f);
    public static readonly Color Blue = new Color(0.08f, 0.40f, 0.92f, 1f);

    public static void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    public static RectTransform CreatePanel(Transform parent, string name, Color fill, Color border, int radius = 24, int borderWidth = 3)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.GetComponent<Image>();
        image.sprite = GetRoundedRectSprite(name, 256, 128, radius, fill, border, borderWidth);
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        image.raycastTarget = false;

        return panelObject.GetComponent<RectTransform>();
    }

    public static RectTransform CreateGradient(Transform parent, string name, Color top, Color bottom)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.GetComponent<Image>();
        image.sprite = GetVerticalGradientSprite(name, top, bottom);
        image.color = Color.white;
        image.raycastTarget = false;

        return panelObject.GetComponent<RectTransform>();
    }

    public static Image CreateImage(Transform parent, string name, Sprite sprite = null)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    public static Sprite GetTextureSprite(Texture2D texture, string key)
    {
        if (texture == null)
        {
            return null;
        }

        string spriteKey = key + "_" + texture.name + "_" + texture.width + "x" + texture.height;
        if (Sprites.TryGetValue(spriteKey, out Sprite sprite))
        {
            return sprite;
        }

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        Sprites[spriteKey] = sprite;
        return sprite;
    }

    public static TMP_Text CreateLabel(Transform parent, string name, string text, int fontSize, Color color)
    {
        GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        StyleText(label, fontSize, color, TextAlignmentOptions.Center, FontStyles.Bold);
        label.text = text;
        label.raycastTarget = false;
        return label;
    }

    public static void StyleText(TMP_Text text, int fontSize, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.fontSizeMin = Mathf.Max(12, fontSize - 14);
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
        text.characterSpacing = 0f;
    }

    public static void StyleButton(Button button, Color fill, Color textColor, string label = null)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedRectSprite("button_" + ColorKey(fill), 240, 90, 20, fill, new Color(1f, 0.94f, 0.68f, 0.56f), 4);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        AddShadow(button.gameObject, new Color(0f, 0f, 0f, 0.44f), new Vector2(0f, -5f));

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
        colors.pressedColor = new Color(0.78f, 0.84f, 0.86f, 1f);
        colors.disabledColor = new Color(0.26f, 0.28f, 0.30f, 0.68f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            if (!string.IsNullOrEmpty(label))
            {
                text.text = label;
            }

            StyleText(text, 27, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        }
    }

    public static void StyleInput(TMP_InputField input, string placeholderText)
    {
        if (input == null)
        {
            return;
        }

        Image image = input.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedRectSprite("input", 256, 84, 18, new Color(0.01f, 0.03f, 0.04f, 0.92f), new Color(0.18f, 0.95f, 0.86f, 0.42f), 3);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        if (input.textComponent != null)
        {
            StyleText(input.textComponent, 24, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
            input.textComponent.margin = new Vector4(20f, 0f, 16f, 0f);
        }

        TMP_Text placeholder = input.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.text = placeholderText;
            StyleText(placeholder, 22, new Color(0.72f, 0.86f, 0.88f, 0.62f), TextAlignmentOptions.Left, FontStyles.Normal);
            placeholder.margin = new Vector4(20f, 0f, 16f, 0f);
        }
    }

    public static void AddShadow(GameObject target, Color color, Vector2 distance)
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

    public static void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        if (target == null)
        {
            return;
        }

        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = target.AddComponent<Outline>();
        }

        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    public static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2? pivot = null)
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

    public static Sprite GetVerticalGradientSprite(string key, Color top, Color bottom)
    {
        string spriteKey = key + "_" + ColorKey(top) + "_" + ColorKey(bottom);
        if (Sprites.TryGetValue(spriteKey, out Sprite sprite))
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
        Sprites[spriteKey] = sprite;
        return sprite;
    }

    public static Sprite GetRoundedRectSprite(string key, int width, int height, int radius, Color fill, Color border, int borderWidth)
    {
        string spriteKey = key + "_" + width + "x" + height + "_" + radius + "_" + ColorKey(fill) + "_" + ColorKey(border) + "_" + borderWidth;
        if (Sprites.TryGetValue(spriteKey, out Sprite sprite))
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
        Sprites[spriteKey] = sprite;
        return sprite;
    }

    public static Sprite GetCircleSprite(string key, int size, Color fill, Color border, int borderWidth)
    {
        string spriteKey = key + "_" + size + "_" + ColorKey(fill) + "_" + ColorKey(border) + "_" + borderWidth;
        if (Sprites.TryGetValue(spriteKey, out Sprite sprite))
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
        Sprites[spriteKey] = sprite;
        return sprite;
    }

    private static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
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

    private static string ColorKey(Color color)
    {
        return Mathf.RoundToInt(color.r * 255f) + "_" +
               Mathf.RoundToInt(color.g * 255f) + "_" +
               Mathf.RoundToInt(color.b * 255f) + "_" +
               Mathf.RoundToInt(color.a * 255f);
    }
}
