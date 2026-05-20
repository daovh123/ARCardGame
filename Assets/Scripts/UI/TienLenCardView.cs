using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TienLenCardView : MonoBehaviour
{
    private Image background;
    private Image border;
    private TMP_Text topText;
    private TMP_Text centerText;
    private TMP_Text bottomText;
    private RectTransform rectTransform;

    public void Setup(PlayingCardData card, bool selected, bool interactable)
    {
        Build();

        Color suitColor = GetSuitColor(card.suit);
        background.color = interactable ? Color.white : new Color(0.76f, 0.78f, 0.80f, 1f);
        border.color = selected ? new Color(1f, 0.76f, 0.18f, 1f) : new Color(0.08f, 0.10f, 0.12f, 1f);

        string label = card.RankLabel + "\n" + card.SuitLabel;
        topText.text = label;
        topText.color = suitColor;
        centerText.text = card.SuitLabel;
        centerText.color = suitColor;
        bottomText.text = label;
        bottomText.color = suitColor;

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }

        group.alpha = interactable ? 1f : 0.58f;
        rectTransform.anchoredPosition = selected ? new Vector2(0f, 18f) : Vector2.zero;
        rectTransform.localScale = selected ? Vector3.one * 1.06f : Vector3.one;
    }

    private void Build()
    {
        if (background != null)
        {
            return;
        }

        rectTransform = transform as RectTransform;

        background = gameObject.GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }

        background.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_card_bg", 180, 252, 18, Color.white, new Color(0.08f, 0.10f, 0.12f, 1f), 5);
        background.type = Image.Type.Sliced;

        GameObject borderObject = new GameObject("SelectedBorder", typeof(RectTransform), typeof(Image));
        borderObject.transform.SetParent(transform, false);
        border = borderObject.GetComponent<Image>();
        border.sprite = RuntimeUITheme.GetRoundedRectSprite("tlmn_card_border", 180, 252, 18, new Color(0f, 0f, 0f, 0f), Color.white, 5);
        border.type = Image.Type.Sliced;
        RuntimeUITheme.SetRect(border.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        topText = CreateText("Top", TextAlignmentOptions.TopLeft, 21, new Vector2(0.08f, 0.58f), new Vector2(0.42f, 0.94f));
        centerText = CreateText("Center", TextAlignmentOptions.Center, 52, new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.84f));
        bottomText = CreateText("Bottom", TextAlignmentOptions.BottomRight, 21, new Vector2(0.58f, 0.06f), new Vector2(0.92f, 0.42f));
    }

    private TMP_Text CreateText(string name, TextAlignmentOptions alignment, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        RuntimeUITheme.StyleText(text, fontSize, Color.black, alignment, FontStyles.Bold);
        text.enableAutoSizing = true;
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
