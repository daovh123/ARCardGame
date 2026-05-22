using UnityEngine;

public class ARCardVisual : MonoBehaviour
{
    [Header("Visual Renderers")]
    [Tooltip("Renderer for the front side of the card (facing up)")]
    public SpriteRenderer frontRenderer;
    [Tooltip("Renderer for the back side of the card (facing down)")]
    public SpriteRenderer backRenderer;

    private static CardSpriteDatabase cardDatabase;

    /// <summary>
    /// Initializes the card's visual texture/sprite and scales it to exactly 0.06m x 0.09m.
    /// </summary>
    /// <param name="card">The card data to display. If null, displays the card back on the front side as well.</param>
    public void Initialize(CardData card)
    {
        // Load the database if not already loaded
        if (cardDatabase == null)
        {
            cardDatabase = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
            if (cardDatabase == null)
            {
                Debug.LogError("[ARCardVisual] Failed to load CardSpriteDatabase from Resources.");
                return;
            }
        }

        // Retrieve appropriate sprites
        Sprite frontSprite = card != null ? cardDatabase.GetSprite(card) : cardDatabase.card_back;
        Sprite backSprite = cardDatabase.card_back;

        // Assign sprites to the renderers
        if (frontRenderer != null)
        {
            frontRenderer.sprite = frontSprite;
        }
        if (backRenderer != null)
        {
            backRenderer.sprite = backSprite;
        }

        // Apply scale to match exactly 0.06m x 0.09m
        if (frontSprite != null)
        {
            Vector3 boundsSize = frontSprite.bounds.size;
            
            // Calculate scale factor relative to target sizes (0.06m wide, 0.09m high)
            float targetWidth = 0.06f;
            float targetHeight = 0.09f;

            if (boundsSize.x > 0 && boundsSize.y > 0)
            {
                float scaleX = targetWidth / boundsSize.x;
                float scaleY = targetHeight / boundsSize.y;

                // Scale the renderers (assuming they are direct children of the card prefab root)
                if (frontRenderer != null)
                {
                    frontRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
                if (backRenderer != null)
                {
                    backRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }
            else
            {
                Debug.LogWarning($"[ARCardVisual] Sprite bounds size is invalid: {boundsSize}");
            }
        }
    }
}
