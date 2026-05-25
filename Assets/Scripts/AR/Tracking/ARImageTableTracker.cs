using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTableTracker : MonoBehaviour
{
    public static event Action<GameObject> OnTableSpawned;

    [Header("AR Tracking")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Table")]
    [SerializeField] private GameObject arTableRootPrefab;
    [SerializeField] private string tableMarkerName = "TableMarker";
    [SerializeField] private bool keepTableAfterFirstDetection = true;

    private GameObject spawnedTable;
    private bool hasDetectedTable;
    private bool hasNotifiedTableSpawned;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        }
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }
        else
        {
            Debug.LogError("[ARImageTableTracker] Missing ARTrackedImageManager reference.");
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var removedImage in args.removed)
        {
            HandleRemovedImage(removedImage.Value);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null || trackedImage.referenceImage.name != tableMarkerName)
        {
            return;
        }

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            if (!keepTableAfterFirstDetection && spawnedTable != null)
            {
                spawnedTable.SetActive(false);
            }

            return;
        }

        if (spawnedTable == null)
        {
            SpawnTable(trackedImage);
        }
        else
        {
            UpdateTablePose(trackedImage);
        }

        spawnedTable.SetActive(true);
        hasDetectedTable = true;

        if (!hasNotifiedTableSpawned)
        {
            hasNotifiedTableSpawned = true;
            OnTableSpawned?.Invoke(spawnedTable);
        }
    }

    private void SpawnTable(ARTrackedImage trackedImage)
    {
        if (arTableRootPrefab == null)
        {
            Debug.LogError("[ARImageTableTracker] Missing AR table root prefab.");
            return;
        }

        spawnedTable = Instantiate(
            arTableRootPrefab,
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );

        Debug.Log("[ARImageTableTracker] Spawned AR table on marker: " + trackedImage.referenceImage.name);
    }

    private void UpdateTablePose(ARTrackedImage trackedImage)
    {
        if (spawnedTable == null)
        {
            return;
        }

        if (keepTableAfterFirstDetection && hasDetectedTable)
        {
            return;
        }

        spawnedTable.transform.SetPositionAndRotation(
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );
    }

    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        if (keepTableAfterFirstDetection)
        {
            return;
        }

        if (spawnedTable != null)
        {
            spawnedTable.SetActive(false);
        }
    }
}