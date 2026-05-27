using UnityEngine;

public class ARCardVisual : MonoBehaviour
{
    [Header("Visual Renderers")]
    public SpriteRenderer frontRenderer;
    public SpriteRenderer backRenderer;

    private static CardSpriteDatabase cardDatabase;
    private bool isFaceUp = true;

    public void Initialize(CardData card)
    {
        if (cardDatabase == null)
        {
            cardDatabase = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
            if (cardDatabase == null)
            {
                Debug.LogError("[ARCardVisual] Failed to load CardSpriteDatabase from Resources.");
                return;
            }
        }

        Sprite frontSprite = card != null ? cardDatabase.GetSprite(card) : cardDatabase.card_back;
        Sprite backSprite = cardDatabase.card_back;

        if (frontRenderer != null)
        {
            frontRenderer.sprite = frontSprite;
        }
        if (backRenderer != null)
        {
            backRenderer.sprite = backSprite;
        }

        if (frontSprite != null)
        {
            Vector3 boundsSize = frontSprite.bounds.size;
            float targetWidth = 0.06f;
            float targetHeight = 0.09f;

            if (boundsSize.x > 0 && boundsSize.y > 0)
            {
                float scaleX = targetWidth / boundsSize.x;
                float scaleY = targetHeight / boundsSize.y;

                if (frontRenderer != null)
                {
                    frontRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
                if (backRenderer != null)
                {
                    backRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }
        }

        SetFaceUp(card != null);
    }

    public void SetFaceUp(bool faceUp)
    {
        isFaceUp = faceUp;

        if (frontRenderer != null)
        {
            frontRenderer.gameObject.SetActive(faceUp);
        }
        if (backRenderer != null)
        {
            backRenderer.gameObject.SetActive(!faceUp);
        }
    }
}
