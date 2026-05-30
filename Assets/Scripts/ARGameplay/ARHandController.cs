using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ARHandController : MonoBehaviour
{
    [Header("Hand Layout")]
    [SerializeField] private Vector3 handRootLocalPosition = new Vector3(0f, -0.10f, 0.62f);
    [SerializeField] private Vector3 handRootLocalEuler = Vector3.zero;
    [SerializeField] private float cardSpacing = 0.052f;
    [SerializeField] private int maxVisibleCards = 25;
    [SerializeField] private float handCardScale = 0.72f;

    [Header("Input")]
    [SerializeField] private float swipeThresholdPixels = 58f;
    [SerializeField] private float raycastDistance = 4f;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<string> lastHandSignature = new List<string>();

    private GameManager gameManager;
    private ARTableController tableController;
    private Camera arCamera;
    private Transform handRoot;
    private ARHandCard selectedCard;
    private ARDrawPileGesture selectedDrawPile;
    private Vector2 pointerStartPosition;
    private float nextRefreshTime;
    private bool initialized;
    private int activePointerId = -1;

    public void Initialize(GameManager manager, ARTableController table)
    {
        gameManager = manager;
        tableController = table;
        arCamera = Camera.main;

        if (arCamera == null)
        {
            arCamera = FindAnyObjectByType<Camera>();
        }

        EnsureHandRoot();
        EnsureDrawPileGesture();
        initialized = gameManager != null && tableController != null && arCamera != null;

        RefreshHand(true);
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        KeepHandAnchoredToCamera();
        HandlePointerInput();

        if (Time.time >= nextRefreshTime)
        {
            nextRefreshTime = Time.time + 0.25f;
            RefreshHand(false);
        }
    }

    public void RefreshHand()
    {
        RefreshHand(true);
    }

    private void RefreshHand(bool force)
    {
        if (gameManager == null || tableController == null || tableController.cardPrefab == null)
        {
            return;
        }

        List<CardData> handCards = gameManager.GetCurrentPlayerHand();
        bool isLocalTurn = gameManager.IsLocalPlayerTurn() && !gameManager.IsGameOver();

        if (!force && IsSameHandState(handCards, isLocalTurn))
        {
            return;
        }

        CaptureHandState(handCards, isLocalTurn);
        ClearSpawnedCards();

        if (handCards == null || handCards.Count == 0)
        {
            return;
        }

        int visibleCount = Mathf.Min(handCards.Count, Mathf.Max(1, maxVisibleCards));
        float center = (visibleCount - 1) * 0.5f;
        float spacing = visibleCount > 1 ? Mathf.Min(cardSpacing, 0.68f / (visibleCount - 1)) : cardSpacing;

        for (int i = 0; i < visibleCount; i++)
        {
            CardData card = handCards[i];
            bool canPlay = isLocalTurn && gameManager.CanPlayHandCard(i);
            Vector3 localPosition = new Vector3((i - center) * spacing, 0f, Mathf.Abs(i - center) * 0.003f);

            GameObject cardObject = Instantiate(tableController.cardPrefab, handRoot);
            cardObject.name = "ARHandCard_" + i;
            cardObject.transform.localPosition = localPosition;
            cardObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            cardObject.transform.localScale = Vector3.one * handCardScale;

            ARCardVisual visual = cardObject.GetComponent<ARCardVisual>();
            if (visual != null)
            {
                visual.Initialize(card);
            }

            ARHandCard handCard = cardObject.GetComponent<ARHandCard>();
            if (handCard == null)
            {
                handCard = cardObject.AddComponent<ARHandCard>();
            }

            handCard.Initialize(this, card, i, canPlay);
            SetSortingOrder(cardObject, 220 + i);
            spawnedCards.Add(cardObject);
        }
    }

    private bool IsSameHandState(List<CardData> handCards, bool isLocalTurn)
    {
        int expectedCount = handCards == null ? 1 : handCards.Count + 1;
        if (lastHandSignature.Count != expectedCount)
        {
            return false;
        }

        if (lastHandSignature[0] != isLocalTurn.ToString())
        {
            return false;
        }

        if (handCards == null)
        {
            return true;
        }

        for (int i = 0; i < handCards.Count; i++)
        {
            string signature = GetCardSignature(handCards[i]) + "|" + gameManager.CanPlayHandCard(i);
            if (lastHandSignature[i + 1] != signature)
            {
                return false;
            }
        }

        return true;
    }

    private void CaptureHandState(List<CardData> handCards, bool isLocalTurn)
    {
        lastHandSignature.Clear();
        lastHandSignature.Add(isLocalTurn.ToString());

        if (handCards == null)
        {
            return;
        }

        for (int i = 0; i < handCards.Count; i++)
        {
            lastHandSignature.Add(GetCardSignature(handCards[i]) + "|" + gameManager.CanPlayHandCard(i));
        }
    }

    private string GetCardSignature(CardData card)
    {
        return card == null ? "null" : card.GetDisplayName();
    }

    private void EnsureHandRoot()
    {
        if (handRoot != null || arCamera == null)
        {
            return;
        }

        GameObject rootObject = new GameObject("ARHandRoot");
        handRoot = rootObject.transform;
        handRoot.SetParent(arCamera.transform, false);
        KeepHandAnchoredToCamera();
    }

    private void KeepHandAnchoredToCamera()
    {
        if (handRoot == null || arCamera == null)
        {
            return;
        }

        if (handRoot.parent != arCamera.transform)
        {
            handRoot.SetParent(arCamera.transform, false);
        }

        handRoot.localPosition = handRootLocalPosition;
        handRoot.localRotation = Quaternion.Euler(handRootLocalEuler);
    }

    private void EnsureDrawPileGesture()
    {
        if (tableController == null || tableController.drawPile == null)
        {
            return;
        }

        ARDrawPileGesture gesture = tableController.drawPile.GetComponent<ARDrawPileGesture>();
        if (gesture == null)
        {
            gesture = tableController.drawPile.gameObject.AddComponent<ARDrawPileGesture>();
        }

        gesture.Initialize(this);
    }

    private void HandlePointerInput()
    {
        if (TryGetPointerDown(out Vector2 downPosition))
        {
            if (IsPointerOverUI())
            {
                selectedCard = null;
                selectedDrawPile = null;
                return;
            }

            pointerStartPosition = downPosition;
            selectedCard = null;
            selectedDrawPile = null;

            if (TryRaycast(downPosition, out RaycastHit hit))
            {
                selectedCard = hit.collider.GetComponentInParent<ARHandCard>();
                selectedDrawPile = hit.collider.GetComponentInParent<ARDrawPileGesture>();

                if (selectedCard != null)
                {
                    selectedCard.SetSelected(true);
                }
            }
        }

        if (TryGetPointerUp(out Vector2 upPosition))
        {
            Vector2 delta = upPosition - pointerStartPosition;
            bool isSwipe = delta.magnitude >= swipeThresholdPixels;

            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);

                if (isSwipe && delta.y > 0f)
                {
                    TryPlayCard(selectedCard);
                }
            }
            else if (selectedDrawPile != null && isSwipe)
            {
                TryDrawCard();
            }

            selectedCard = null;
            selectedDrawPile = null;
            activePointerId = -1;
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return activePointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(activePointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }

    private bool TryRaycast(Vector2 screenPosition, out RaycastHit hit)
    {
        hit = default;

        if (arCamera == null)
        {
            return false;
        }

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out hit, raycastDistance, ~0, QueryTriggerInteraction.Collide);
    }

    private bool TryGetPointerDown(out Vector2 position)
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
        {
            position = touchscreen.primaryTouch.position.ReadValue();
            activePointerId = touchscreen.primaryTouch.touchId.ReadValue();
            return true;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            activePointerId = -1;
            return true;
        }

        position = Vector2.zero;
        return false;
    }

    private bool TryGetPointerUp(out Vector2 position)
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.wasReleasedThisFrame)
        {
            position = touchscreen.primaryTouch.position.ReadValue();
            return true;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
        {
            position = mouse.position.ReadValue();
            return true;
        }

        position = Vector2.zero;
        return false;
    }

    private void TryPlayCard(ARHandCard card)
    {
        if (card == null || gameManager == null || !gameManager.IsLocalPlayerTurn() || gameManager.IsGameOver())
        {
            return;
        }

        if (!gameManager.CanPlayHandCard(card.HandIndex))
        {
            RuntimeSfx.Play(RuntimeSfxType.Error, 0.72f);
            RefreshHand(true);
            return;
        }

        CardData selected = card.Card;
        if (gameManager.RequiresColorChoice(selected))
        {
            GameUIManager uiManager = FindAnyObjectByType<GameUIManager>();
            if (uiManager != null)
            {
                uiManager.RequestColorChoiceFromAR(card.HandIndex);
                return;
            }
        }

        gameManager.PlayCard(card.HandIndex);
        RuntimeSfx.Play(RuntimeSfxType.Play, 0.8f);
        RefreshGameUI();
    }

    private void TryDrawCard()
    {
        if (gameManager == null || !gameManager.IsLocalPlayerTurn() || gameManager.IsGameOver())
        {
            return;
        }

        gameManager.DrawOneCardFromAR();
        RuntimeSfx.Play(RuntimeSfxType.Draw, 0.8f);
        RefreshGameUI();
    }

    private void RefreshGameUI()
    {
        GameUIManager uiManager = FindAnyObjectByType<GameUIManager>();
        if (uiManager != null)
        {
            uiManager.RefreshUI();
        }
        else
        {
            RefreshHand(true);
        }
    }

    private void ClearSpawnedCards()
    {
        foreach (GameObject cardObject in spawnedCards)
        {
            if (cardObject != null)
            {
                Destroy(cardObject);
            }
        }

        spawnedCards.Clear();
    }

    private void SetSortingOrder(GameObject cardObject, int sortingOrder)
    {
        foreach (SpriteRenderer renderer in cardObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingOrder = sortingOrder;
        }
    }
}
