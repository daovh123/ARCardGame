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

    [Header("Stabilization")]
    [SerializeField] private bool forceHorizontalTable = true;
    [SerializeField] private float tableWorldScale = 0.75f;
    [SerializeField] private Vector3 tableLocalOffset = new Vector3(0f, -0.02f, 0f);
    [SerializeField] private float stableLockTime = 0.6f;
    [SerializeField] private float positionSmoothing = 10f;
    [SerializeField] private float rotationSmoothing = 10f;

    private GameObject spawnedTable;
    private bool hasDetectedTable;
    private bool hasNotifiedTableSpawned;
    private bool isTablePoseLocked;
    private float stableTrackingTime;

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
            stableTrackingTime = 0f;

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

        if (spawnedTable != null)
        {
            spawnedTable.SetActive(true);
        }

        hasDetectedTable = true;

        if (!hasNotifiedTableSpawned && spawnedTable != null)
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

        Pose tablePose = GetStableTablePose(trackedImage);
        spawnedTable = Instantiate(arTableRootPrefab, tablePose.position, tablePose.rotation);
        spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
        stableTrackingTime = 0f;
        isTablePoseLocked = false;

        Debug.Log("[ARImageTableTracker] Spawned AR table on marker: " + trackedImage.referenceImage.name);
    }

    private void UpdateTablePose(ARTrackedImage trackedImage)
    {
        if (spawnedTable == null)
        {
            return;
        }

        if (keepTableAfterFirstDetection && isTablePoseLocked)
        {
            return;
        }

        Pose targetPose = GetStableTablePose(trackedImage);

        spawnedTable.transform.position = Vector3.Lerp(
            spawnedTable.transform.position,
            targetPose.position,
            Time.deltaTime * positionSmoothing);

        spawnedTable.transform.rotation = Quaternion.Slerp(
            spawnedTable.transform.rotation,
            targetPose.rotation,
            Time.deltaTime * rotationSmoothing);

        spawnedTable.transform.localScale = Vector3.one * tableWorldScale;

        if (keepTableAfterFirstDetection)
        {
            stableTrackingTime += Time.deltaTime;

            if (stableTrackingTime >= stableLockTime)
            {
                isTablePoseLocked = true;
                spawnedTable.transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
                spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
                Debug.Log("[ARImageTableTracker] AR table pose locked.");
            }
        }
    }

    private Pose GetStableTablePose(ARTrackedImage trackedImage)
    {
        Transform imageTransform = trackedImage.transform;
        Vector3 position = imageTransform.TransformPoint(tableLocalOffset);
        Quaternion rotation = imageTransform.rotation;

        if (forceHorizontalTable)
        {
            Vector3 forward = Camera.main != null
                ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up)
                : Vector3.ProjectOnPlane(imageTransform.forward, Vector3.up);

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        return new Pose(position, rotation);
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