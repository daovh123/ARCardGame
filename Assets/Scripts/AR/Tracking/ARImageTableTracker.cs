using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTableTracker : MonoBehaviour
{
    public static event Action<GameObject> OnTableSpawned;

    [Header("AR Tracking")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private Camera arCamera;

    [Header("Table")]
    [SerializeField] private GameObject arTableRootPrefab;
    [SerializeField] private string tableMarkerName = "TableMarker";
    [SerializeField] private bool keepTableAfterFirstDetection = true;

    [Header("Placement")]
    [SerializeField] private bool placeTableAtScreenCenter = true;
    [SerializeField] private bool allowLimitedTrackingToSpawn = true;
    [SerializeField] private float tableWorldScale = 0.62f;
    [SerializeField] private float tableDistanceFromCamera = 0.65f;
    [SerializeField] private Vector2 tableViewportPosition = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector3 tableWorldOffset = new Vector3(0f, -0.035f, 0f);
    [SerializeField] private float tableTiltTowardCameraDegrees = 12f;

    [Header("Marker Fallback")]
    [SerializeField] private bool forceHorizontalTable = true;
    [SerializeField] private Vector3 markerLocalOffset = new Vector3(0f, -0.02f, 0f);

    [Header("Stabilization")]
    [SerializeField] private float stableLockTime = 0.2f;
    [SerializeField] private float positionSmoothing = 16f;
    [SerializeField] private float rotationSmoothing = 16f;

    private GameObject spawnedTable;
    private bool hasDetectedTable;
    private bool hasNotifiedTableSpawned;
    private bool isTablePoseLocked;
    private float stableTrackingTime;
    private Pose lockedTablePose;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        }

        if (arCamera == null)
        {
            arCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
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

        if (!CanUseTrackedImage(trackedImage))
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

    private bool CanUseTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            return true;
        }

        return allowLimitedTrackingToSpawn && trackedImage.trackingState == TrackingState.Limited;
    }

    private void SpawnTable(ARTrackedImage trackedImage)
    {
        if (arTableRootPrefab == null)
        {
            Debug.LogError("[ARImageTableTracker] Missing AR table root prefab.");
            return;
        }

        Pose tablePose = GetStableTablePose(trackedImage);
        lockedTablePose = tablePose;
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
            spawnedTable.transform.SetPositionAndRotation(lockedTablePose.position, lockedTablePose.rotation);
            spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
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
                lockedTablePose = targetPose;
                spawnedTable.transform.SetPositionAndRotation(lockedTablePose.position, lockedTablePose.rotation);
                spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
                Debug.Log("[ARImageTableTracker] AR table pose locked.");
            }
        }
    }

    private Pose GetStableTablePose(ARTrackedImage trackedImage)
    {
        if (placeTableAtScreenCenter && arCamera != null)
        {
            return GetCameraCenteredTablePose();
        }

        return GetMarkerAnchoredTablePose(trackedImage);
    }

    private Pose GetCameraCenteredTablePose()
    {
        Ray centerRay = arCamera.ViewportPointToRay(new Vector3(tableViewportPosition.x, tableViewportPosition.y, 0f));
        Vector3 position = centerRay.origin + centerRay.direction.normalized * tableDistanceFromCamera + tableWorldOffset;

        Vector3 cameraForwardFlat = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up);
        if (cameraForwardFlat.sqrMagnitude < 0.001f)
        {
            cameraForwardFlat = Vector3.ProjectOnPlane(arCamera.transform.up, Vector3.up);
        }

        if (cameraForwardFlat.sqrMagnitude < 0.001f)
        {
            cameraForwardFlat = Vector3.forward;
        }

        Quaternion baseRotation = Quaternion.LookRotation(cameraForwardFlat.normalized, Vector3.up);
        Quaternion tiltedRotation = baseRotation * Quaternion.Euler(-tableTiltTowardCameraDegrees, 0f, 0f);
        return new Pose(position, tiltedRotation);
    }

    private Pose GetMarkerAnchoredTablePose(ARTrackedImage trackedImage)
    {
        Transform imageTransform = trackedImage.transform;
        Vector3 position = imageTransform.TransformPoint(markerLocalOffset);
        Quaternion rotation = imageTransform.rotation;

        if (forceHorizontalTable)
        {
            Vector3 forward = arCamera != null
                ? Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up)
                : Vector3.ProjectOnPlane(imageTransform.forward, Vector3.up);

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            rotation = Quaternion.LookRotation(forward.normalized, Vector3.up) * Quaternion.Euler(-tableTiltTowardCameraDegrees, 0f, 0f);
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