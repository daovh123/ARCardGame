using UnityEngine;

public class GameSceneBootstrapper : MonoBehaviour
{
    [Header("UNO Components")]
    [SerializeField] private GameManager unoGameManager;
    [SerializeField] private GameUIManager unoUIManager;

    [Header("Tien Len Components")]
    [SerializeField] private TienLenGameManager tienLenGameManager;
    [SerializeField] private TienLenUIManager tienLenUIManager;

    [Header("AR Event Bridges")]
    [SerializeField] private UnoARGameEventBridge unoARBridge;
    [SerializeField] private TienLenARGameEventBridge tienLenARBridge;

    private void Awake()
    {
        if (GameModeSelection.CurrentMode == GameMode.Uno)
        {
            SetupUnoMode();
        }
        else if (GameModeSelection.CurrentMode == GameMode.TienLenMienNam)
        {
            SetupTienLenMode();
        }
    }

    private void SetupUnoMode()
    {
        SetActiveSafe(unoGameManager, true);
        SetActiveSafe(unoUIManager, true);
        SetEnabledSafe(unoARBridge, true);

        SetActiveSafe(tienLenGameManager, false);
        SetEnabledSafe(tienLenUIManager, false);
        SetEnabledSafe(tienLenARBridge, false);

        if (tienLenGameManager == null)
        {
            TienLenGameManager[] tienLenManagers = FindObjectsOfType<TienLenGameManager>();
            foreach (TienLenGameManager mgr in tienLenManagers)
            {
                mgr.gameObject.SetActive(false);
            }
        }

        Debug.Log("[GameSceneBootstrapper] UNO mode activated.");
    }

    private void SetupTienLenMode()
    {
        SetActiveSafe(unoGameManager, false);
        SetActiveSafe(unoUIManager, false);
        SetEnabledSafe(unoARBridge, false);

        if (tienLenGameManager == null)
        {
            GameObject runtimeObject = new GameObject("TienLenRuntime");
            tienLenGameManager = runtimeObject.AddComponent<TienLenGameManager>();

            if (tienLenUIManager == null)
            {
                tienLenUIManager = runtimeObject.AddComponent<TienLenUIManager>();
            }
        }

        SetActiveSafe(tienLenGameManager, true);
        SetEnabledSafe(tienLenUIManager, true);
        SetEnabledSafe(tienLenARBridge, true);

        tienLenGameManager.StartGame();
        if (tienLenUIManager != null)
        {
            tienLenUIManager.Initialize(tienLenGameManager);
        }

        Debug.Log("[GameSceneBootstrapper] Tien Len mode activated.");
    }

    private void SetActiveSafe(Component component, bool active)
    {
        if (component != null)
        {
            component.gameObject.SetActive(active);
        }
    }

    private void SetEnabledSafe(Behaviour behaviour, bool enabled)
    {
        if (behaviour != null)
        {
            behaviour.enabled = enabled;
        }
    }
}
