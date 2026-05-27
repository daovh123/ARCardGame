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

    private void Awake()
    {
        SetGameplayVisible(false);

        if (scanMarkerPanel != null)
        {
            scanMarkerPanel.SetActive(true);
        }

        if (unoBridge != null)
        {
            unoBridge.enabled = false;
        }

        if (tienLenBridge != null)
        {
            tienLenBridge.enabled = false;
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

        if (scanMarkerPanel != null)
        {
            scanMarkerPanel.SetActive(false);
        }

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
        if (gameplayCanvasGroup == null)
        {
            return;
        }

        gameplayCanvasGroup.alpha = visible ? 1f : 0f;
        gameplayCanvasGroup.interactable = visible;
        gameplayCanvasGroup.blocksRaycasts = visible;
    }
}
