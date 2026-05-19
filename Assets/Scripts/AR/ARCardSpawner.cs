using UnityEngine;

public class ARCardSpawner : MonoBehaviour
{
    private GameObject currentCardVisual;

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
        SpawnPlayedCard(card, playerIndex);
    }

    private void SpawnPlayedCard(CardData card, int playerIndex)
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

        // Tạo cube trực tiếp, không dùng prefab
        currentCardVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentCardVisual.name = "PlayedCard_" + card.GetDisplayName();

        // Đặt ngay giữa màn hình, trước camera 5 đơn vị
        Vector3 spawnPosition = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
        currentCardVisual.transform.position = spawnPosition;

        // Quay mặt về camera
        currentCardVisual.transform.rotation = cam.transform.rotation;

        // Làm to để chắc chắn nhìn thấy
        currentCardVisual.transform.localScale = new Vector3(1.2f, 1.6f, 0.15f);


        Renderer renderer = currentCardVisual.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = GetCardColor(card.color);
        }

        Debug.Log("[AR SPAWNER] Spawned visual card: " + card.GetDisplayName());
        Debug.Log("[AR SPAWNER] Position: " + currentCardVisual.transform.position);
        Debug.Log("[AR SPAWNER] Camera position: " + cam.transform.position);
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