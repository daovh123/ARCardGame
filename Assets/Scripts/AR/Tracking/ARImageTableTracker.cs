using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTableTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject arTableRootPrefab;
    [SerializeField] private string tableMarkerName = "TableMarker";
    [SerializeField] private bool keepTableAfterFirstDetection = true;

    private GameObject spawnedTable;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();
        }
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            TryUpdateTable(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            TryUpdateTable(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            if (!keepTableAfterFirstDetection && spawnedTable != null)
            {
                spawnedTable.SetActive(false);
            }
        }
    }

    private void TryUpdateTable(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name != tableMarkerName)
        {
            return;
        }

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            return;
        }

        if (arTableRootPrefab == null)
        {
            Debug.LogError("[AR] Missing AR table root prefab.");
            return;
        }

        if (spawnedTable == null)
        {
            spawnedTable = Instantiate(arTableRootPrefab);
        }

        spawnedTable.transform.SetPositionAndRotation(
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );

        spawnedTable.SetActive(true);
    }
}