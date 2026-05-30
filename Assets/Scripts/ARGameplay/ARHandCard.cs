using UnityEngine;

public class ARHandCard : MonoBehaviour
{
    private const float CardWidth = 0.12f;
    private const float CardHeight = 0.17f;
    private const float CardThickness = 0.025f;

    private ARHandController controller;
    private Vector3 baseScale;
    private bool isSelected;

    public CardData Card { get; private set; }
    public int HandIndex { get; private set; }
    public bool IsPlayable { get; private set; }

    public void Initialize(ARHandController owner, CardData card, int handIndex, bool isPlayable)
    {
        controller = owner;
        Card = card;
        HandIndex = handIndex;
        IsPlayable = isPlayable;
        baseScale = transform.localScale;

        EnsureCollider();
        ApplyVisualState();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyVisualState();
    }

    private void EnsureCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.center = Vector3.zero;
        box.size = new Vector3(CardWidth, CardHeight, CardThickness);
    }

    private void ApplyVisualState()
    {
        float scale = isSelected ? 1.18f : IsPlayable ? 1.08f : 1f;
        transform.localScale = baseScale * scale;

        Color color = IsPlayable
            ? Color.white
            : new Color(0.44f, 0.44f, 0.44f, 0.88f);

        if (isSelected)
        {
            color = new Color(1f, 0.94f, 0.45f, 1f);
        }

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.color = color;
        }
    }
}
