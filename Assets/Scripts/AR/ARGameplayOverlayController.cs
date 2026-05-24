using UnityEngine;

public class ARGameplayOverlayController : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameplayCanvasGroup;
    [SerializeField] private GameObject scanMarkerPanel;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        SetGameplayVisible(false);

        if (scanMarkerPanel != null)
        {
            scanMarkerPanel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        ARImageTableTracker.OnTableSpawned += HandleTableSpawned;
    }

    private void OnDisable()
    {
        ARImageTableTracker.OnTableSpawned -= HandleTableSpawned;
    }

    private void HandleTableSpawned(GameObject tableObject)
    {
        if (scanMarkerPanel != null)
        {
            scanMarkerPanel.SetActive(false);
        }

        SetGameplayVisible(true);

        ARTableController table = tableObject.GetComponent<ARTableController>();
        if (table != null && gameManager != null)
        {
            table.ShowTurn(gameManager.GetCurrentPlayerIndex());

            CardData topCard = gameManager.GetTopDiscardCard();
            if (topCard != null)
            {
                table.ShowTopDiscardCard(topCard);
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