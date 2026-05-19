using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image cardImage;
    public TMP_Text fallbackText;
    public Button button;

    private static CardSpriteDatabase database;

    private int handIndex;
    private System.Action<int> onClick;

    private void Awake()
    {
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        if (database == null)
        {
            database = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
        }
    }

    public void Setup(CardData card, int handIndex, System.Action<int> onClick)
    {
        this.handIndex = handIndex;
        this.onClick = onClick;

        LoadDatabase();

        Sprite sprite = database != null ? database.GetSprite(card) : null;

        if (cardImage != null && sprite != null)
        {
            cardImage.sprite = sprite;
            cardImage.color = Color.white;
        }

        if (fallbackText != null)
        {
            fallbackText.text = sprite == null ? card.GetDisplayName() : "";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            this.onClick?.Invoke(this.handIndex);
        });
    }
}