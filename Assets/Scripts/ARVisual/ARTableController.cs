using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARTableController : MonoBehaviour
{
    [Header("Table Layout Anchors")]
    [Tooltip("The physical table surface GameObject")]
    public Transform tableSurface;
    [Tooltip("Anchor position for the draw pile")]
    public Transform drawPile;
    [Tooltip("Anchor position for the discard pile")]
    public Transform discardPile;
    [Tooltip("Anchors for the 4 player positions (0: South/Bottom, 1: West/Left, 2: North/Top, 3: East/Right)")]
    public Transform[] playerSlots = new Transform[4];
    [Tooltip("Visual indicator for the active player's turn")]
    public Transform turnIndicator;
    [Tooltip("Root anchor for victory and special effects")]
    public Transform effectRoot;

    [Header("Card Prefab & Visuals")]
    [Tooltip("The AR Card Prefab to spawn for played/drawn cards")]
    public GameObject cardPrefab;

    [Header("Animation Settings")]
    [Tooltip("Duration of the card play/discard animation")]
    public float playDuration = 0.6f;
    [Tooltip("Duration of the card draw animation")]
    public float drawDuration = 0.5f;
    [Tooltip("Speed at which the turn indicator transitions between player slots")]
    public float indicatorTransitionSpeed = 8f;
    [Tooltip("Height offset of the turn indicator above player slots")]
    public float turnIndicatorHeightOffset = 0.05f;

    private List<GameObject> activeDiscardedCards = new List<GameObject>();
    private const int MaxDiscardedCards = 8;

    private int activePlayerIndex = -1;
    private Vector3 indicatorTargetPos;
    private bool isIndicatorActive = false;

    private void Start()
    {
        // Hide turn indicator initially if no turn is set
        if (turnIndicator != null && activePlayerIndex == -1)
        {
            turnIndicator.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Smoothly transition the turn indicator position
        if (isIndicatorActive && turnIndicator != null && activePlayerIndex >= 0 && activePlayerIndex < playerSlots.Length)
        {
            Transform slot = playerSlots[activePlayerIndex];
            if (slot != null)
            {
                indicatorTargetPos = slot.position + Vector3.up * turnIndicatorHeightOffset;

                // Add a hover/bobbing offset to make the active slot indicator breathe/hover
                float bobbingOffset = 0.012f * Mathf.Sin(Time.time * 4f);
                indicatorTargetPos.y += bobbingOffset;

                turnIndicator.position = Vector3.Lerp(turnIndicator.position, indicatorTargetPos, Time.deltaTime * indicatorTransitionSpeed);

                // Slow rotation to make the ring feel dynamic and alive
                turnIndicator.Rotate(Vector3.up, Time.deltaTime * 30f, Space.World);
            }
        }
    }

    /// <summary>
    /// Activates and moves the turn indicator to highlight the specified player's slot.
    /// </summary>
    public void ShowTurn(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerSlots.Length)
        {
            Debug.LogWarning($"[ARTableController] ShowTurn called with invalid player index: {playerIndex}");
            return;
        }

        activePlayerIndex = playerIndex;

        if (turnIndicator != null)
        {
            if (!turnIndicator.gameObject.activeSelf)
            {
                // Position it instantly at the start if it was inactive
                Transform slot = playerSlots[playerIndex];
                if (slot != null)
                {
                    turnIndicator.position = slot.position + Vector3.up * turnIndicatorHeightOffset;
                }
                turnIndicator.gameObject.SetActive(true);
            }
            isIndicatorActive = true;
        }
    }

    /// <summary>
    /// Spawns a card visual at the player slot, and animates it flying to the discard pile.
    /// </summary>
    public void ShowPlayedCard(CardData card, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerSlots.Length)
        {
            Debug.LogWarning($"[ARTableController] ShowPlayedCard: Invalid player index: {playerIndex}");
            return;
        }

        Transform spawnSlot = playerSlots[playerIndex];
        if (spawnSlot == null || discardPile == null || cardPrefab == null)
        {
            Debug.LogError("[ARTableController] ShowPlayedCard components/references missing.");
            return;
        }

        StartCoroutine(AnimateCardPlay(card, spawnSlot, discardPile));
    }

    /// <summary>
    /// Spawns a temporary card at the draw pile and animates it flying to the player slot, then fades/destroys it.
    /// </summary>
    public void ShowDrawEffect(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerSlots.Length)
        {
            Debug.LogWarning($"[ARTableController] ShowDrawEffect: Invalid player index: {playerIndex}");
            return;
        }

        Transform targetSlot = playerSlots[playerIndex];
        if (targetSlot == null || drawPile == null || cardPrefab == null)
        {
            Debug.LogError("[ARTableController] ShowDrawEffect components/references missing.");
            return;
        }

        StartCoroutine(AnimateCardDraw(targetSlot, drawPile));
    }

    public void ShowTopDiscardCard(CardData card)
    {
        if (card == null || discardPile == null || cardPrefab == null)
        {
            return;
        }

        GameObject cardInstance = Instantiate(
            cardPrefab,
            discardPile.position,
            discardPile.rotation,
            transform
        );

        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(card);
        }

        activeDiscardedCards.Add(cardInstance);
    }

    /// <summary>
    /// Triggers celebration text/effects when a player wins the game.
    /// </summary>
    public void ShowWinner(string winnerName)
    {
        Transform targetAnchor = effectRoot != null ? effectRoot : transform;

        // Spawn colorful confetti particles
        SpawnVictoryParticles(targetAnchor.position);

        // Instantiate celebration floating text
        GameObject winTextObj = new GameObject("VictoryText");
        winTextObj.transform.position = targetAnchor.position + Vector3.up * 0.1f;
        winTextObj.transform.rotation = Quaternion.Euler(30f, 0f, 0f); // Tilted slightly towards camera
        winTextObj.transform.localScale = Vector3.one * 0.05f;

        TextMesh textMesh = winTextObj.AddComponent<TextMesh>();
        textMesh.text = $"Winner: {winnerName}!";
        textMesh.fontSize = 80;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(1f, 0.85f, 0f); // Premium Gold

        // Add a simple animated floating/pulse effect
        StartCoroutine(AnimateVictoryBillboard(winTextObj));
    }

    private void SpawnVictoryParticles(Vector3 position)
    {
        GameObject particlesObj = new GameObject("VictoryConfettiParticles");
        particlesObj.transform.position = position;

        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        // Configure Particle System
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
        main.gravityModifier = 0.6f;
        main.duration = 4.0f;
        main.loop = true;

        var emission = ps.emission;
        emission.rateOverTime = 60f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.85f, 0f), 0.0f), // Gold
                new GradientColorKey(Color.red, 0.33f), 
                new GradientColorKey(Color.cyan, 0.66f),
                new GradientColorKey(Color.magenta, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.7f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = grad;

        // Configure renderer
        ParticleSystemRenderer psr = particlesObj.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            
            Material particleMat = new Material(shader);
            psr.sharedMaterial = particleMat;
        }

        ps.Play();
        Destroy(particlesObj, 8f);
    }

    private IEnumerator AnimateCardPlay(CardData card, Transform startSlot, Transform targetPile)
    {
        // 1. Instantiate card prefab at player's slot
        GameObject cardInstance = Instantiate(cardPrefab, startSlot.position, startSlot.rotation, transform);
        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(card);
        }

        // Add to active list
        activeDiscardedCards.Add(cardInstance);

        // 2. Animate movement along arc, lật mặt (flip) and rotation to flat position
        Vector3 startPos = startSlot.position;
        // Stack cards slightly to avoid Z-fighting/overlapping perfectly
        float yStackOffset = activeDiscardedCards.Count * 0.001f;
        Vector3 endPos = targetPile.position + Vector3.up * yStackOffset;

        Quaternion startRot = startSlot.rotation;
        // The card should lie flat on the table (euler rotation around Y only)
        float randomYaw = Random.Range(-15f, 15f);
        Quaternion endRot = Quaternion.Euler(0f, randomYaw, 0f);

        float elapsed = 0f;
        while (elapsed < playDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / playDuration;

            // Ease out quad
            t = t * (2f - t);

            // Calculate position with a parabolic arc height
            float arcHeight = 0.08f * Mathf.Sin(t * Mathf.PI);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += arcHeight;

            cardInstance.transform.position = currentPos;
            cardInstance.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Hard set final position/rotation
        cardInstance.transform.position = endPos;
        cardInstance.transform.rotation = endRot;

        // 3. Keep discard pile trimmed
        while (activeDiscardedCards.Count > MaxDiscardedCards)
        {
            GameObject oldestCard = activeDiscardedCards[0];
            activeDiscardedCards.RemoveAt(0);
            if (oldestCard != null)
            {
                Destroy(oldestCard);
            }
        }
    }

    private IEnumerator AnimateCardDraw(Transform targetSlot, Transform startPile)
    {
        // 1. Instantiate face-down card prefab at draw pile
        GameObject cardInstance = Instantiate(cardPrefab, startPile.position, startPile.rotation, transform);
        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(null); // passing null yields face-down representation
        }

        // 2. Animate movement along arc to player's slot
        Vector3 startPos = startPile.position;
        Vector3 endPos = targetSlot.position;
        Quaternion startRot = startPile.rotation;
        Quaternion endRot = targetSlot.rotation;
        Vector3 initialScale = cardInstance.transform.localScale;

        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / drawDuration;

            // Ease in quad
            t = t * t;

            // Parabolic arc
            float arcHeight = 0.05f * Mathf.Sin(t * Mathf.PI);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += arcHeight;

            cardInstance.transform.position = currentPos;
            cardInstance.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            // Scale down to 0 at the end (fade out visual effect)
            if (t > 0.7f)
            {
                float scaleT = (1f - t) / 0.3f;
                cardInstance.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, scaleT);
            }

            yield return null;
        }

        Destroy(cardInstance);
    }

    private IEnumerator AnimateVictoryBillboard(GameObject obj)
    {
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (obj != null)
        {
            elapsed += Time.deltaTime;
            
            // Pulse scale
            float scaleMultiplier = 1f + 0.1f * Mathf.Sin(elapsed * 4f);
            obj.transform.localScale = startScale * scaleMultiplier;

            // Slowly rotate around Y axis
            obj.transform.Rotate(Vector3.up, Time.deltaTime * 15f, Space.World);

            // Slowly hover up and down
            float yOffset = 0.015f * Mathf.Sin(elapsed * 2f);
            Vector3 pos = obj.transform.position;
            pos.y = (effectRoot != null ? effectRoot.position.y : transform.position.y) + 0.1f + yOffset;
            obj.transform.position = pos;

            yield return null;
        }
    }
}
