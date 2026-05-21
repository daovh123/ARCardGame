using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TienLenCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    private Image border;
    private TMP_Text topText;
    private TMP_Text centerText;
    private TMP_Text bottomText;
    private RectTransform visualRoot;
    private CanvasGroup canvasGroup;
    private Shadow shadow;
    private Coroutine motionRoutine;
    private Coroutine invalidRoutine;
    private bool selected;
    private bool interactable;
    private bool hovered;

    public void Setup(PlayingCardData card, bool selected, bool interactable)
    {
        Build();
        this.selected = selected;
        this.interactable = interactable;
        hovered = false;

        Sprite cardSprite = TienLenCardSpriteDatabase.GetSprite(card);
        if (cardSprite != null)
        {
            background.sprite = cardSprite;
            background.type = Image.Type.Simple;
            background.preserveAspect = false;
            background.color = interactable ? Color.white : new Color(0.72f, 0.74f, 0.76f, 1f);
            topText.gameObject.SetActive(false);
            centerText.gameObject.SetActive(false);
            bottomText.gameObject.SetActive(false);
        }
        else
        {
            ApplyFallbackFace(card, interactable);
        }

        border.gameObject.SetActive(false);
        border.color = Color.clear;

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = interactable ? 1f : 0.58f;
        ApplyPose(false);
    }

    private void ApplyFallbackFace(PlayingCardData card, bool interactable)
    {
        Color suitColor = GetSuitColor(card.suit);
        background.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_card_bg", 180, 252, 18, Color.white, new Color(0.08f, 0.10f, 0.12f, 1f), 5);
        background.type = Image.Type.Sliced;
        background.preserveAspect = false;
        background.color = interactable ? Color.white : new Color(0.76f, 0.78f, 0.80f, 1f);

        topText.gameObject.SetActive(true);
        centerText.gameObject.SetActive(true);
        bottomText.gameObject.SetActive(true);

        string label = card.RankLabel + "\n" + card.SuitLabel;
        topText.text = label;
        topText.color = suitColor;
        centerText.text = card.SuitLabel;
        centerText.color = suitColor;
        bottomText.text = label;
        bottomText.color = suitColor;
    }

    private void Build()
    {
        if (background != null)
        {
            return;
        }

        Image hitBox = gameObject.GetComponent<Image>();
        if (hitBox == null)
        {
            hitBox = gameObject.AddComponent<Image>();
        }

        hitBox.sprite = null;
        hitBox.type = Image.Type.Simple;
        hitBox.color = new Color(1f, 1f, 1f, 0f);
        hitBox.raycastTarget = true;

        Shadow rootShadow = GetComponent<Shadow>();
        if (rootShadow != null)
        {
            rootShadow.enabled = false;
        }

        GameObject visualObject = new GameObject("VisualRoot", typeof(RectTransform), typeof(Image), typeof(Shadow));
        visualObject.transform.SetParent(transform, false);
        visualRoot = visualObject.GetComponent<RectTransform>();
        RuntimeUITheme.SetRect(visualRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        background = visualObject.GetComponent<Image>();
        background.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_card_bg", 180, 252, 18, Color.white, new Color(0.08f, 0.10f, 0.12f, 1f), 5);
        background.type = Image.Type.Sliced;
        background.preserveAspect = false;
        background.raycastTarget = false;

        shadow = visualObject.GetComponent<Shadow>();
        shadow.effectDistance = new Vector2(0f, -4f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.useGraphicAlpha = true;

        GameObject borderObject = new GameObject("SelectedBorder", typeof(RectTransform), typeof(Image));
        borderObject.transform.SetParent(visualRoot, false);
        border = borderObject.GetComponent<Image>();
        border.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_card_border", 180, 252, 18, new Color(0f, 0f, 0f, 0f), Color.white, 5);
        border.type = Image.Type.Sliced;
        border.raycastTarget = false;
        RuntimeUITheme.SetRect(border.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        topText = CreateText("Top", TextAlignmentOptions.TopLeft, 21, new Vector2(0.08f, 0.58f), new Vector2(0.42f, 0.94f));
        centerText = CreateText("Center", TextAlignmentOptions.Center, 52, new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.84f));
        bottomText = CreateText("Bottom", TextAlignmentOptions.BottomRight, 21, new Vector2(0.58f, 0.06f), new Vector2(0.92f, 0.42f));
    }

    public void SetSelected(bool isSelected, bool animated)
    {
        selected = isSelected;
        border.gameObject.SetActive(false);
        border.color = Color.clear;
        ApplyPose(animated);
    }

    public void SetTemporaryHidden(bool hidden)
    {
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = hidden ? 0.18f : (interactable ? 1f : 0.58f);
    }

    public void FlashInvalid()
    {
        Build();

        if (invalidRoutine != null)
        {
            StopCoroutine(invalidRoutine);
        }

        invalidRoutine = StartCoroutine(FlashInvalidRoutine());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable)
        {
            return;
        }

        hovered = true;
        ApplyPose(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        ApplyPose(false);
    }

    private void ApplyPose(bool animated)
    {
        Vector2 targetPosition = selected ? new Vector2(0f, 18f) : Vector2.zero;
        Vector3 targetScale = Vector3.one;

        if (shadow == null)
        {
            shadow = visualRoot != null ? visualRoot.GetComponent<Shadow>() : GetComponent<Shadow>();
        }

        if (shadow != null)
        {
            shadow.effectDistance = selected ? new Vector2(0f, -8f) : new Vector2(0f, -4f);
            shadow.effectColor = selected
                ? new Color(0f, 0f, 0f, 0.66f)
                : new Color(0f, 0f, 0f, hovered ? 0.50f : 0.42f);
        }

        if (!animated)
        {
            visualRoot.anchoredPosition = targetPosition;
            visualRoot.localScale = targetScale;
            return;
        }

        if (motionRoutine != null)
        {
            StopCoroutine(motionRoutine);
        }

        motionRoutine = StartCoroutine(AnimatePose(targetPosition, targetScale, 0.12f));
    }

    private IEnumerator AnimatePose(Vector2 targetPosition, Vector3 targetScale, float duration)
    {
        Vector2 startPosition = visualRoot.anchoredPosition;
        Vector3 startScale = visualRoot.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
            visualRoot.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
            visualRoot.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            yield return null;
        }

        visualRoot.anchoredPosition = targetPosition;
        visualRoot.localScale = targetScale;
    }

    private IEnumerator FlashInvalidRoutine()
    {
        Vector2 basePosition = visualRoot.anchoredPosition;
        border.gameObject.SetActive(true);
        border.color = RuntimeUITheme.Red;

        for (int i = 0; i < 8; i++)
        {
            float offset = i % 2 == 0 ? -8f : 8f;
            visualRoot.anchoredPosition = basePosition + new Vector2(offset, 0f);
            yield return new WaitForSecondsRealtime(0.035f);
        }

        visualRoot.anchoredPosition = basePosition;
        border.gameObject.SetActive(false);
        border.color = Color.clear;
        invalidRoutine = null;
    }

    private float EaseOutCubic(float t)
    {
        float p = t - 1f;
        return p * p * p + 1f;
    }

    private TMP_Text CreateText(string name, TextAlignmentOptions alignment, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(visualRoot, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        RuntimeUITheme.StyleText(text, fontSize, Color.black, alignment, FontStyles.Bold);
        text.enableAutoSizing = true;
        text.raycastTarget = false;
        RuntimeUITheme.SetRect(text.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        return text;
    }

    private Color GetSuitColor(PlayingCardSuit suit)
    {
        if (suit == PlayingCardSuit.Hearts || suit == PlayingCardSuit.Diamonds)
        {
            return new Color(0.80f, 0.04f, 0.08f, 1f);
        }

        return new Color(0.02f, 0.04f, 0.05f, 1f);
    }
}
