using UnityEngine;

public class SpecialEffectSpawner : MonoBehaviour
{
    private GameObject currentEffect;

    private void OnEnable()
    {
        GameEvents.OnSpecialCardPlayed += HandleSpecialCardPlayed;
        GameEvents.OnCardDrawn += HandleCardDrawn;
        GameEvents.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnSpecialCardPlayed -= HandleSpecialCardPlayed;
        GameEvents.OnCardDrawn -= HandleCardDrawn;
        GameEvents.OnTurnChanged -= HandleTurnChanged;
    }

    private void HandleSpecialCardPlayed(CardData card, int playerIndex)
    {
        switch (card.type)
        {

            case CardType.Block:
                SpawnTextEffect("BLOCK", Color.red);
                break;

            case CardType.Reverse:
                SpawnTextEffect("REVERSE", Color.cyan);
                break;

            case CardType.DrawTwo:
                SpawnTextEffect("+2", Color.yellow);
                break;

            case CardType.DrawFour:
                SpawnTextEffect("+4", Color.magenta);
                break;

            case CardType.ChangeColor:
                SpawnTextEffect("CHANGE COLOR", Color.green);
                break;
        }
    }

    private void HandleCardDrawn(int playerIndex)
    {
        SpawnTextEffect("DRAW", Color.yellow);
    }

    private void HandleTurnChanged(int currentPlayerIndex)
    {
        Debug.Log("[SPECIAL EFFECT] Turn highlight player " + (currentPlayerIndex + 1));
    }

    private void SpawnTextEffect(string text, Color color)
    {
        if (currentEffect != null)
        {
            Destroy(currentEffect);
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found.");
            return;
        }

        currentEffect = new GameObject("Effect_" + text);

        TextMesh textMesh = currentEffect.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 80;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        currentEffect.transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.65f, 4f));
        currentEffect.transform.rotation = cam.transform.rotation;
        currentEffect.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        Debug.Log("[SPECIAL EFFECT] " + text);

        Destroy(currentEffect, 2f);
    }
}