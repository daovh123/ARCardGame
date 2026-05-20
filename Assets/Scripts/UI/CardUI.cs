using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    public Image cardImage;
    public TMP_Text fallbackText;
    public Button button;

    private static CardSpriteDatabase database;

    private int handIndex;
    private System.Action<int> onClick;
    private bool playable;
    private bool hovering;
    private Vector3 hoverBaseScale;
    private Vector3 pressBaseScale;

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

    public void SetPlayable(bool playable)
    {
        this.playable = playable;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = playable ? 1f : 0.42f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
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

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.72f);
            this.onClick?.Invoke(this.handIndex);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playable || hovering)
        {
            return;
        }

        hovering = true;
        hoverBaseScale = transform.localScale;
        transform.localScale = hoverBaseScale * 1.055f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hovering)
        {
            return;
        }

        hovering = false;
        transform.localScale = hoverBaseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!playable)
        {
            return;
        }

        pressBaseScale = transform.localScale;
        transform.localScale = pressBaseScale * 0.97f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!playable)
        {
            return;
        }

        transform.localScale = hovering ? hoverBaseScale * 1.055f : pressBaseScale;
    }
}
