using UnityEngine;

public class ARGameplayOverlayController : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameplayCanvasGroup;
    [SerializeField] private GameObject scanMarkerPanel;
    [SerializeField] private GameManager gameManager;

    private ARHandController arHandController;

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

            EnsureARHandController(table);
        }
    }

    private void EnsureARHandController(ARTableController table)
    {
        if (arHandController == null)
        {
            arHandController = FindAnyObjectByType<ARHandController>();
        }

        if (arHandController == null)
        {
            arHandController = gameObject.AddComponent<ARHandController>();
        }

        arHandController.Initialize(gameManager, table);
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
