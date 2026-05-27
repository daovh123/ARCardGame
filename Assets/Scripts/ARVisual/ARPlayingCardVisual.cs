using UnityEngine;

public class ARPlayingCardVisual : MonoBehaviour
{
    [Header("Visual Renderers")]
    public SpriteRenderer frontRenderer;
    public TextMesh fallbackText;

    private static TienLenCardAssetDatabase assetDatabase;
    private static bool missingDatabaseLogged;

    public void Initialize(PlayingCardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[ARPlayingCardVisual] Initialize called with null card.");
            return;
        }

        Sprite sprite = LoadSprite(card);

        if (sprite != null && frontRenderer != null)
        {
            frontRenderer.sprite = sprite;
            ApplyScale(sprite);
            HideFallbackText();
        }
        else
        {
            ShowFallbackText(card);
        }
    }

    private Sprite LoadSprite(PlayingCardData card)
    {
        Sprite sprite = TienLenCardSpriteDatabase.GetSprite(card);
        if (sprite != null)
        {
            return sprite;
        }

        if (assetDatabase == null && !missingDatabaseLogged)
        {
            assetDatabase = Resources.Load<TienLenCardAssetDatabase>("TienLen/TienLenCardAssetDatabase");
            if (assetDatabase == null)
            {
                missingDatabaseLogged = true;
            }
        }

        if (assetDatabase != null)
        {
            Texture2D texture = assetDatabase.GetTexture(card);
            if (texture != null)
            {
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }

        return null;
    }

    private void ApplyScale(Sprite sprite)
    {
        if (sprite == null || frontRenderer == null)
        {
            return;
        }

        Vector3 boundsSize = sprite.bounds.size;
        float targetWidth = 0.06f;
        float targetHeight = 0.09f;

        if (boundsSize.x > 0 && boundsSize.y > 0)
        {
            float scaleX = targetWidth / boundsSize.x;
            float scaleY = targetHeight / boundsSize.y;
            frontRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    private void ShowFallbackText(PlayingCardData card)
    {
        if (frontRenderer != null)
        {
            frontRenderer.sprite = null;
            frontRenderer.color = Color.white;
        }

        if (fallbackText == null)
        {
            GameObject textObj = new GameObject("FallbackText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one * 0.001f;

            fallbackText = textObj.AddComponent<TextMesh>();
            fallbackText.fontSize = 60;
            fallbackText.anchor = TextAnchor.MiddleCenter;
            fallbackText.alignment = TextAlignment.Center;
        }

        fallbackText.text = card.RankLabel + card.SuitLabel;
        fallbackText.color = GetSuitColor(card.suit);
        fallbackText.gameObject.SetActive(true);
    }

    private void HideFallbackText()
    {
        if (fallbackText != null)
        {
            fallbackText.gameObject.SetActive(false);
        }
    }

    private Color GetSuitColor(PlayingCardSuit suit)
    {
        if (suit == PlayingCardSuit.Hearts || suit == PlayingCardSuit.Diamonds)
        {
            return new Color(0.85f, 0.05f, 0.1f);
        }
        return new Color(0.02f, 0.04f, 0.05f);
    }
}
