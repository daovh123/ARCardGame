using UnityEngine;

public class ARCardSpawner : MonoBehaviour
{
    private GameObject currentCardVisual;
    private static CardSpriteDatabase cardDatabase;

    private void OnEnable()
    {
        GameEvents.OnCardPlayed += HandleCardPlayed;
    }

    private void OnDisable()
    {
        GameEvents.OnCardPlayed -= HandleCardPlayed;
    }

    private void HandleCardPlayed(CardData card, int playerIndex)
    {
        SpawnPlayedCard(card);
    }

    private void SpawnPlayedCard(CardData card)
    {
        if (currentCardVisual != null)
        {
            Destroy(currentCardVisual);
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("[AR SPAWNER] Main Camera not found. Please set Main Camera tag to MainCamera.");
            return;
        }

        if (cardDatabase == null)
        {
            cardDatabase = Resources.Load<CardSpriteDatabase>("CardSpriteDatabase");
        }

        Sprite sprite = cardDatabase != null ? cardDatabase.GetSprite(card) : null;

        currentCardVisual = new GameObject("PlayedCard_" + card.GetDisplayName());
        currentCardVisual.transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.52f, 4.2f));
        currentCardVisual.transform.rotation = cam.transform.rotation;
        currentCardVisual.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

        SpriteRenderer spriteRenderer = currentCardVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 100;

        if (sprite == null)
        {
            spriteRenderer.color = GetCardColor(card.color);
        }

        Debug.Log("[AR SPAWNER] Spawned visual card: " + card.GetDisplayName());
        Destroy(currentCardVisual, 2.5f);
    }

    private Color GetCardColor(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red:
                return Color.red;

            case CardColor.Blue:
                return Color.blue;

            case CardColor.Green:
                return Color.green;

            case CardColor.Yellow:
                return Color.yellow;

            default:
                return Color.white;
        }
    }
}
