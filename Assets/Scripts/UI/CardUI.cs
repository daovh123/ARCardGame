using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TMP_Text cardNameText;
    public Button button;
    public Image backgroundImage;

    private int handIndex;
    private System.Action<int> onClick;

    public void Setup(CardData card, int handIndex, System.Action<int> onClick)
    {
        this.handIndex = handIndex;
        this.onClick = onClick;

        cardNameText.text = card.GetDisplayName();

        if (backgroundImage != null)
        {
            backgroundImage.color = GetColor(card.color);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            this.onClick?.Invoke(this.handIndex);
        });
    }

    private Color GetColor(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red:
                return new Color(0.9f, 0.2f, 0.2f);

            case CardColor.Blue:
                return new Color(0.2f, 0.4f, 0.9f);

            case CardColor.Green:
                return new Color(0.2f, 0.7f, 0.3f);

            case CardColor.Yellow:
                return new Color(0.95f, 0.8f, 0.2f);

            default:
                return Color.white;
        }
    }
}