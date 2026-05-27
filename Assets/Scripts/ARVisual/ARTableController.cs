using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARTableController : MonoBehaviour
{
    [Header("Table Layout Anchors")]
    public Transform tableSurface;
    public Transform drawPilePoint;
    public Transform discardPilePoint;
    public Transform centerPlayPoint;
    public Transform[] playerSlots = new Transform[4];
    public Transform turnIndicator;
    public Transform effectRoot;
    public Transform victoryEffectPoint;

    [Header("Card Prefabs")]
    public GameObject unoCardPrefab;
    public GameObject tienLenCardPrefab;

    [Header("Animation Settings")]
    public float playDuration = 0.6f;
    public float drawDuration = 0.5f;
    public float indicatorTransitionSpeed = 8f;
    public float turnIndicatorHeightOffset = 0.05f;

    private List<GameObject> activeDiscardedCards = new List<GameObject>();
    private List<GameObject> activeCenterCards = new List<GameObject>();
    private const int MaxDiscardedCards = 8;
    private const int MaxCenterCards = 13;

    private int activePlayerIndex = -1;
    private Vector3 indicatorTargetPos;
    private bool isIndicatorActive = false;
    private bool initialized = false;

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        ClearAllVisuals();

        if (turnIndicator != null)
        {
            turnIndicator.gameObject.SetActive(false);
        }

        isIndicatorActive = false;
        activePlayerIndex = -1;
        initialized = true;

        Debug.Log("[ARTableController] Initialized.");
    }

    private void Update()
    {
        if (!isIndicatorActive || turnIndicator == null)
        {
            return;
        }

        if (activePlayerIndex < 0 || activePlayerIndex >= playerSlots.Length)
        {
            return;
        }

        Transform slot = playerSlots[activePlayerIndex];
        if (slot == null)
        {
            return;
        }

        indicatorTargetPos = slot.position + Vector3.up * turnIndicatorHeightOffset;
        float bobbingOffset = 0.012f * Mathf.Sin(Time.time * 4f);
        indicatorTargetPos.y += bobbingOffset;

        turnIndicator.position = Vector3.Lerp(turnIndicator.position, indicatorTargetPos, Time.deltaTime * indicatorTransitionSpeed);
        turnIndicator.Rotate(Vector3.up, Time.deltaTime * 30f, Space.World);
    }

    public void ShowTurn(int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            return;
        }

        activePlayerIndex = playerIndex;

        if (turnIndicator != null)
        {
            if (!turnIndicator.gameObject.activeSelf)
            {
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

    public void ShowPlayedUnoCard(CardData card, int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            return;
        }

        Transform spawnSlot = playerSlots[playerIndex];
        Transform target = discardPilePoint != null ? discardPilePoint : centerPlayPoint;

        if (spawnSlot == null || target == null || unoCardPrefab == null)
        {
            Debug.LogError("[ARTableController] ShowPlayedUnoCard: missing references.");
            return;
        }

        StartCoroutine(AnimateUnoCardPlay(card, spawnSlot, target));
    }

    public void ShowDrawEffect(int playerIndex, int count)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            return;
        }

        Transform targetSlot = playerSlots[playerIndex];
        Transform source = drawPilePoint != null ? drawPilePoint : centerPlayPoint;

        if (targetSlot == null || source == null || unoCardPrefab == null)
        {
            Debug.LogError("[ARTableController] ShowDrawEffect: missing references.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float delay = i * 0.15f;
            StartCoroutine(AnimateCardDraw(targetSlot, source, delay));
        }
    }

    public void ShowUnoSpecialEffect(CardData card, int playerIndex)
    {
        if (card == null)
        {
            return;
        }

        Vector3 spawnPos = GetEffectSpawnPosition(playerIndex);

        switch (card.type)
        {
            case CardType.Block:
                SpawnTextEffect("BLOCK", spawnPos, Color.red);
                break;
            case CardType.Reverse:
                SpawnTextEffect("REVERSE", spawnPos, Color.cyan);
                break;
            case CardType.DrawTwo:
                SpawnTextEffect("+2", spawnPos, Color.yellow);
                break;
            case CardType.DrawFour:
                SpawnTextEffect("+4", spawnPos, Color.magenta);
                break;
            case CardType.ChangeColor:
                SpawnTextEffect("COLOR", spawnPos, Color.green);
                break;
        }
    }

    public void ShowTienLenCards(List<PlayingCardData> cards, int playerIndex)
    {
        if (cards == null || cards.Count == 0)
        {
            return;
        }

        if (!IsValidPlayerIndex(playerIndex))
        {
            return;
        }

        Transform spawnSlot = playerSlots[playerIndex];
        Transform target = centerPlayPoint != null ? centerPlayPoint : discardPilePoint;

        if (spawnSlot == null || target == null)
        {
            Debug.LogError("[ARTableController] ShowTienLenCards: missing references.");
            return;
        }

        ClearCenterCards();
        StartCoroutine(AnimateTienLenCards(cards, playerIndex, spawnSlot, target));
    }

    public void ShowTienLenPass(int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            return;
        }

        Vector3 spawnPos = GetEffectSpawnPosition(playerIndex);
        SpawnTextEffect("PASS", spawnPos, new Color(0.2f, 0.5f, 1f));
    }

    public void ShowTienLenSpecialEffect(string effectName, int playerIndex)
    {
        Vector3 spawnPos = centerPlayPoint != null ? centerPlayPoint.position : transform.position + Vector3.up * 0.1f;

        switch (effectName.ToUpperInvariant())
        {
            case "BOMB":
                SpawnTextEffect("BOMB!", spawnPos, Color.red);
                SpawnPulseEffect(spawnPos, new Color(1f, 0.3f, 0.1f, 0.6f));
                break;
            case "CHOP":
                SpawnTextEffect("CHOP!", spawnPos, new Color(1f, 0.5f, 0f));
                SpawnPulseEffect(spawnPos, new Color(1f, 0.6f, 0.1f, 0.6f));
                break;
            case "INSTANT WIN":
                SpawnTextEffect("INSTANT WIN!", spawnPos, new Color(1f, 0.85f, 0f));
                SpawnVictoryParticles(spawnPos);
                break;
            default:
                SpawnTextEffect(effectName, spawnPos, Color.white);
                break;
        }
    }

    public void ShowWinner(string winnerName)
    {
        Transform anchor = victoryEffectPoint != null ? victoryEffectPoint : (effectRoot != null ? effectRoot : transform);
        Vector3 pos = anchor.position;

        SpawnVictoryParticles(pos);

        GameObject winTextObj = new GameObject("VictoryText");
        winTextObj.transform.SetParent(effectRoot != null ? effectRoot : transform);
        winTextObj.transform.position = pos + Vector3.up * 0.1f;
        winTextObj.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        winTextObj.transform.localScale = Vector3.one * 0.05f;

        TextMesh textMesh = winTextObj.AddComponent<TextMesh>();
        textMesh.text = "Winner: " + winnerName + "!";
        textMesh.fontSize = 80;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(1f, 0.85f, 0f);

        StartCoroutine(AnimateVictoryBillboard(winTextObj));
    }

    public void ShowTopDiscardCard(CardData card)
    {
        if (card == null || discardPilePoint == null || unoCardPrefab == null)
        {
            return;
        }

        GameObject cardInstance = Instantiate(unoCardPrefab, discardPilePoint.position, discardPilePoint.rotation, transform);
        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(card);
        }

        activeDiscardedCards.Add(cardInstance);
    }

    public void ClearAllVisuals()
    {
        ClearDiscardedCards();
        ClearCenterCards();
        DestroyEffectObjects();
    }

    private void ClearDiscardedCards()
    {
        foreach (GameObject card in activeDiscardedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        activeDiscardedCards.Clear();
    }

    private void ClearCenterCards()
    {
        foreach (GameObject card in activeCenterCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        activeCenterCards.Clear();
    }

    private void DestroyEffectObjects()
    {
        if (effectRoot == null)
        {
            return;
        }

        for (int i = effectRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(effectRoot.GetChild(i).gameObject);
        }
    }

    private bool IsValidPlayerIndex(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < playerSlots.Length && playerSlots[playerIndex] != null;
    }

    private Vector3 GetEffectSpawnPosition(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            return playerSlots[playerIndex].position + Vector3.up * 0.08f;
        }

        return (effectRoot != null ? effectRoot.position : transform.position) + Vector3.up * 0.08f;
    }

    // ==================== UNO Animations ====================

    private IEnumerator AnimateUnoCardPlay(CardData card, Transform startSlot, Transform targetPile)
    {
        GameObject cardInstance = Instantiate(unoCardPrefab, startSlot.position, startSlot.rotation, transform);
        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(card);
        }

        activeDiscardedCards.Add(cardInstance);

        Vector3 startPos = startSlot.position;
        float yStackOffset = activeDiscardedCards.Count * 0.001f;
        Vector3 endPos = targetPile.position + Vector3.up * yStackOffset;

        Quaternion startRot = startSlot.rotation;
        float randomYaw = Random.Range(-15f, 15f);
        Quaternion endRot = Quaternion.Euler(0f, randomYaw, 0f);

        yield return AnimateCardMovement(cardInstance, startPos, endPos, startRot, endRot, playDuration);

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

    private IEnumerator AnimateCardDraw(Transform targetSlot, Transform startPile, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        GameObject cardInstance = Instantiate(unoCardPrefab, startPile.position, startPile.rotation, transform);
        ARCardVisual cardVisual = cardInstance.GetComponent<ARCardVisual>();
        if (cardVisual != null)
        {
            cardVisual.Initialize(null);
        }

        Vector3 startPos = startPile.position;
        Vector3 endPos = targetSlot.position;
        Quaternion startRot = startPile.rotation;
        Quaternion endRot = targetSlot.rotation;
        Vector3 initialScale = cardInstance.transform.localScale;

        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / drawDuration);
            float eased = t * t;

            float arcHeight = 0.05f * Mathf.Sin(eased * Mathf.PI);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, eased);
            currentPos.y += arcHeight;

            cardInstance.transform.position = currentPos;
            cardInstance.transform.rotation = Quaternion.Slerp(startRot, endRot, eased);

            if (eased > 0.7f)
            {
                float scaleT = (1f - eased) / 0.3f;
                cardInstance.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, scaleT);
            }

            yield return null;
        }

        Destroy(cardInstance);
    }

    // ==================== Tien Len Animations ====================

    private IEnumerator AnimateTienLenCards(List<PlayingCardData> cards, int playerIndex, Transform startSlot, Transform targetPoint)
    {
        GameObject prefab = tienLenCardPrefab != null ? tienLenCardPrefab : unoCardPrefab;
        if (prefab == null)
        {
            Debug.LogError("[ARTableController] No card prefab assigned for Tien Len.");
            yield break;
        }

        float spacing = cards.Count > 5 ? 0.025f : 0.035f;
        float totalWidth = (cards.Count - 1) * spacing;
        Vector3 centerOffset = targetPoint.position;

        for (int i = 0; i < cards.Count; i++)
        {
            float delay = i * 0.05f;
            StartCoroutine(AnimateSingleTienLenCard(cards[i], startSlot.position, centerOffset, spacing, i, cards.Count, delay, prefab));
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator AnimateSingleTienLenCard(PlayingCardData card, Vector3 startPos, Vector3 centerPos, float spacing, int index, int totalCount, float delay, GameObject prefab)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float totalWidth = (totalCount - 1) * spacing;
        Vector3 offset = new Vector3(-totalWidth * 0.5f + index * spacing, 0f, 0f);
        Vector3 endPos = centerPos + offset;

        GameObject cardInstance = Instantiate(prefab, startPos, Quaternion.identity, transform);

        ARPlayingCardVisual tienLenVisual = cardInstance.GetComponent<ARPlayingCardVisual>();
        if (tienLenVisual != null)
        {
            tienLenVisual.Initialize(card);
        }
        else
        {
            ARCardVisual unoVisual = cardInstance.GetComponent<ARCardVisual>();
            if (unoVisual != null)
            {
                CardData fallbackCard = new CardData(card.cardId, CardColor.Wild, CardType.Number, card.rank);
                unoVisual.Initialize(fallbackCard);
            }
        }

        activeCenterCards.Add(cardInstance);

        Quaternion startRot = Quaternion.LookRotation(startPos - centerPos);
        Quaternion endRot = Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);

        yield return AnimateCardMovement(cardInstance, startPos, endPos, startRot, endRot, playDuration);

        TrimCenterCards();
    }

    private void TrimCenterCards()
    {
        while (activeCenterCards.Count > MaxCenterCards)
        {
            GameObject oldest = activeCenterCards[0];
            activeCenterCards.RemoveAt(0);
            if (oldest != null)
            {
                Destroy(oldest);
            }
        }
    }

    // ==================== Shared Animations ====================

    private IEnumerator AnimateCardMovement(GameObject cardInstance, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * (2f - t);

            float arcHeight = 0.08f * Mathf.Sin(eased * Mathf.PI);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, eased);
            currentPos.y += arcHeight;

            cardInstance.transform.position = currentPos;
            cardInstance.transform.rotation = Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        cardInstance.transform.position = endPos;
        cardInstance.transform.rotation = endRot;
    }

    private void SpawnTextEffect(string text, Vector3 position, Color color)
    {
        if (effectRoot == null)
        {
            Debug.LogWarning("[ARTableController] effectRoot is null, cannot spawn text effect.");
            return;
        }

        GameObject effectObj = new GameObject("Effect_" + text);
        effectObj.transform.SetParent(effectRoot);
        effectObj.transform.position = position;
        effectObj.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        effectObj.transform.localScale = Vector3.one * 0.04f;

        TextMesh textMesh = effectObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 80;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        StartCoroutine(AnimateTextEffect(effectObj));
    }

    private IEnumerator AnimateTextEffect(GameObject effectObj)
    {
        if (effectObj == null)
        {
            yield break;
        }

        Vector3 startScale = effectObj.transform.localScale;
        float duration = 1.8f;
        float elapsed = 0f;

        while (elapsed < duration && effectObj != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scaleMultiplier = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
            effectObj.transform.localScale = startScale * scaleMultiplier;

            effectObj.transform.Rotate(Vector3.up, Time.deltaTime * 20f, Space.World);

            TextMesh textMesh = effectObj.GetComponent<TextMesh>();
            if (textMesh != null && t > 0.7f)
            {
                Color c = textMesh.color;
                c.a = 1f - (t - 0.7f) / 0.3f;
                textMesh.color = c;
            }

            yield return null;
        }

        if (effectObj != null)
        {
            Destroy(effectObj);
        }
    }

    private void SpawnPulseEffect(Vector3 position, Color color)
    {
        if (effectRoot == null)
        {
            return;
        }

        GameObject pulseObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulseObj.name = "PulseEffect";
        pulseObj.transform.SetParent(effectRoot);
        pulseObj.transform.position = position;
        pulseObj.transform.localScale = Vector3.one * 0.01f;

        Collider collider = pulseObj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = pulseObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            Material mat = new Material(shader);
            mat.color = color;
            renderer.sharedMaterial = mat;
        }

        StartCoroutine(AnimatePulseEffect(pulseObj));
    }

    private IEnumerator AnimatePulseEffect(GameObject pulseObj)
    {
        if (pulseObj == null)
        {
            yield break;
        }

        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.01f;
        Vector3 endScale = Vector3.one * 0.2f;

        while (elapsed < duration && pulseObj != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            pulseObj.transform.localScale = Vector3.Lerp(startScale, endScale, eased);

            Renderer renderer = pulseObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.sharedMaterial;
                if (mat != null)
                {
                    Color c = mat.color;
                    c.a = 1f - t;
                    mat.color = c;
                }
            }

            yield return null;
        }

        if (pulseObj != null)
        {
            Destroy(pulseObj);
        }
    }

    private void SpawnVictoryParticles(Vector3 position)
    {
        if (effectRoot == null)
        {
            return;
        }

        GameObject particlesObj = new GameObject("VictoryParticles");
        particlesObj.transform.SetParent(effectRoot);
        particlesObj.transform.position = position;

        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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
                new GradientColorKey(new Color(1f, 0.85f, 0f), 0.0f),
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

    private IEnumerator AnimateVictoryBillboard(GameObject obj)
    {
        if (obj == null)
        {
            yield break;
        }

        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (obj != null)
        {
            elapsed += Time.deltaTime;

            float scaleMultiplier = 1f + 0.1f * Mathf.Sin(elapsed * 4f);
            obj.transform.localScale = startScale * scaleMultiplier;

            obj.transform.Rotate(Vector3.up, Time.deltaTime * 15f, Space.World);

            float yOffset = 0.015f * Mathf.Sin(elapsed * 2f);
            Vector3 pos = obj.transform.position;
            float baseY = (effectRoot != null ? effectRoot.position.y : transform.position.y) + 0.1f;
            pos.y = baseY + yOffset;
            obj.transform.position = pos;

            yield return null;
        }
    }
}
