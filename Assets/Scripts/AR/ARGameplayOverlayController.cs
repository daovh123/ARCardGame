using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARGameplayOverlayController : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameplayCanvasGroup;
    [SerializeField] private GameObject scanMarkerPanel;
    [SerializeField] private GameManager gameManager;

    [Header("AR Event Bridges")]
    [SerializeField] private UnoARGameEventBridge unoBridge;
    [SerializeField] private TienLenARGameEventBridge tienLenBridge;

    private ARTableController currentTableController;
    private RectTransform gameplayRoot;
    private CanvasGroup gameplayRootGroup;
    private bool tableReady;

    private void Awake()
    {
        PrepareGameplayRoot();
        SetGameplayVisible(false);
        SetScanMarkerVisible(true);

        if (unoBridge != null)
        {
            unoBridge.enabled = false;
        }

        if (tienLenBridge != null)
        {
            tienLenBridge.enabled = false;
        }
    }

    private IEnumerator Start()
    {
        // GameUIManager creates part of the HUD in Start, so regroup once more
        // after the first frame before keeping gameplay hidden during scanning.
        yield return null;

        PrepareGameplayRoot();

        if (!tableReady)
        {
            SetGameplayVisible(false);
            SetScanMarkerVisible(true);
        }
    }

    private void OnEnable()
    {
        ARImageTableTracker.OnTableControllerReady += HandleTableControllerReady;
    }

    private void OnDisable()
    {
        ARImageTableTracker.OnTableControllerReady -= HandleTableControllerReady;
    }

    private void HandleTableControllerReady(ARTableController controller)
    {
        currentTableController = controller;
        tableReady = true;

        PrepareGameplayRoot();
        SetScanMarkerVisible(false);
        SetGameplayVisible(true);
        ConnectBridges(controller);
        InitializeTableState(controller);
    }

    private void ConnectBridges(ARTableController controller)
    {
        if (unoBridge != null)
        {
            unoBridge.arTableController = controller;
            unoBridge.enabled = GameModeSelection.CurrentMode == GameMode.Uno;
        }

        if (tienLenBridge != null)
        {
            tienLenBridge.arTableController = controller;
            tienLenBridge.enabled = GameModeSelection.CurrentMode == GameMode.TienLenMienNam;
        }
    }

    private void InitializeTableState(ARTableController controller)
    {
        if (controller == null)
        {
            return;
        }

        if (GameModeSelection.CurrentMode == GameMode.Uno && gameManager != null)
        {
            controller.ShowTurn(gameManager.GetCurrentPlayerIndex());

            CardData topCard = gameManager.GetTopDiscardCard();
            if (topCard != null)
            {
                controller.ShowTopDiscardCard(topCard);
            }
        }
    }

    private void SetGameplayVisible(bool visible)
    {
        CanvasGroup targetGroup = gameplayRootGroup != null ? gameplayRootGroup : gameplayCanvasGroup;
        if (targetGroup == null)
        {
            return;
        }

        targetGroup.alpha = visible ? 1f : 0f;
        targetGroup.interactable = visible;
        targetGroup.blocksRaycasts = visible;
    }

    private void SetScanMarkerVisible(bool visible)
    {
        if (scanMarkerPanel != null)
        {
            scanMarkerPanel.SetActive(visible);
            scanMarkerPanel.transform.SetAsLastSibling();
        }
    }

    private void PrepareGameplayRoot()
    {
        if (gameplayCanvasGroup == null || scanMarkerPanel == null)
        {
            return;
        }

        Transform canvasTransform = gameplayCanvasGroup.transform;
        if (!scanMarkerPanel.transform.IsChildOf(canvasTransform))
        {
            return;
        }

        if (gameplayRoot == null)
        {
            GameObject rootObject = new GameObject("Runtime_ARGameplayRoot", typeof(RectTransform), typeof(CanvasGroup));
            gameplayRoot = rootObject.GetComponent<RectTransform>();
            gameplayRoot.SetParent(canvasTransform, false);
            gameplayRoot.anchorMin = Vector2.zero;
            gameplayRoot.anchorMax = Vector2.one;
            gameplayRoot.offsetMin = Vector2.zero;
            gameplayRoot.offsetMax = Vector2.zero;
            gameplayRoot.pivot = new Vector2(0.5f, 0.5f);
            gameplayRoot.SetAsFirstSibling();

            gameplayRootGroup = rootObject.GetComponent<CanvasGroup>();
        }

        gameplayCanvasGroup.alpha = 1f;
        gameplayCanvasGroup.interactable = true;
        gameplayCanvasGroup.blocksRaycasts = true;

        List<Transform> childrenToMove = new List<Transform>();
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child == gameplayRoot || child == scanMarkerPanel.transform)
            {
                continue;
            }

            childrenToMove.Add(child);
        }

        foreach (Transform child in childrenToMove)
        {
            child.SetParent(gameplayRoot, false);
        }
    }
}
