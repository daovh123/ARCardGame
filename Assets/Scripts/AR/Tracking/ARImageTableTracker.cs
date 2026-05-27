using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTableTracker : MonoBehaviour
{
    public static event Action<GameObject> OnTableSpawned;
    public static event Action<ARTableController> OnTableControllerReady;

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
    [SerializeField] private float tableWorldScale = 0.86f;
    [SerializeField] private float tableDistanceFromCamera = 0.82f;
    [SerializeField] private Vector2 tableViewportPosition = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector3 tableWorldOffset = new Vector3(0f, -0.035f, 0f);
    [SerializeField] private float tableTiltTowardCameraDegrees = 20f;

    [Header("Marker Fallback")]
    [SerializeField] private bool forceHorizontalTable = true;
    [SerializeField] private Vector3 markerLocalOffset = new Vector3(0f, -0.02f, 0f);

    [Header("Stabilization")]
    [SerializeField] private float stableLockTime = 0.2f;
    [SerializeField] private float positionSmoothing = 16f;
    [SerializeField] private float rotationSmoothing = 16f;
    [SerializeField] private bool recenterWhenCameraSettles = true;
    [SerializeField] private float cameraIdleTimeToRecenter = 0.65f;
    [SerializeField] private float recenterDuration = 0.22f;
    [SerializeField] private float cameraMoveThreshold = 0.008f;
    [SerializeField] private float cameraAngleThreshold = 0.8f;
    [SerializeField] private float recenterPositionThreshold = 0.12f;
    [SerializeField] private float recenterAngleThreshold = 10f;

    private GameObject spawnedTable;
    private ARTableController tableController;
    private bool hasDetectedTable;
    private bool hasNotifiedTableSpawned;
    private bool isTablePoseLocked;
    private float stableTrackingTime;
    private Pose lockedTablePose;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private Vector3 recenterReferenceCameraPosition;
    private Quaternion recenterReferenceCameraRotation;
    private float cameraIdleTimer;
    private bool hasCameraPose;
    private bool cameraMovedSinceLastRecenter;
    private bool isRecentering;
    private float recenterElapsed;
    private Pose recenterStartPose;
    private Pose recenterTargetPose;

    public ARTableController TableController
    {
        get { return tableController; }
    }

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


    private void Update()
    {
        if (!recenterWhenCameraSettles || spawnedTable == null || arCamera == null)
        {
            return;
        }

        UpdateCameraSettleRecenter();
        UpdateRecenterAnimation();
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

            if (tableController != null)
            {
                tableController.Initialize();
                OnTableControllerReady?.Invoke(tableController);
            }
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
        CaptureCameraPoseBaseline();

        tableController = spawnedTable.GetComponentInChildren<ARTableController>();
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
            if (!isRecentering)
            {
                spawnedTable.transform.SetPositionAndRotation(lockedTablePose.position, lockedTablePose.rotation);
                spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
            }
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


    private void CaptureCameraPoseBaseline()
    {
        if (arCamera == null)
        {
            hasCameraPose = false;
            return;
        }

        lastCameraPosition = arCamera.transform.position;
        lastCameraRotation = arCamera.transform.rotation;
        recenterReferenceCameraPosition = lastCameraPosition;
        recenterReferenceCameraRotation = lastCameraRotation;
        cameraIdleTimer = 0f;
        hasCameraPose = true;
        cameraMovedSinceLastRecenter = false;
    }

    private void UpdateCameraSettleRecenter()
    {
        if (isRecentering)
        {
            return;
        }

        if (!hasCameraPose)
        {
            CaptureCameraPoseBaseline();
            return;
        }

        Vector3 currentPosition = arCamera.transform.position;
        Quaternion currentRotation = arCamera.transform.rotation;
        float frameMoveDelta = Vector3.Distance(currentPosition, lastCameraPosition);
        float frameAngleDelta = Quaternion.Angle(currentRotation, lastCameraRotation);
        bool handIsMoving = frameMoveDelta >= cameraMoveThreshold || frameAngleDelta >= cameraAngleThreshold;

        lastCameraPosition = currentPosition;
        lastCameraRotation = currentRotation;

        float totalMoveDelta = Vector3.Distance(currentPosition, recenterReferenceCameraPosition);
        float totalAngleDelta = Quaternion.Angle(currentRotation, recenterReferenceCameraRotation);
        bool needsRecenter = totalMoveDelta >= recenterPositionThreshold || totalAngleDelta >= recenterAngleThreshold;

        if (!needsRecenter)
        {
            cameraIdleTimer = 0f;
            cameraMovedSinceLastRecenter = false;
            KeepLockedTablePose();
            return;
        }

        cameraMovedSinceLastRecenter = true;

        if (handIsMoving)
        {
            cameraIdleTimer = 0f;
            KeepLockedTablePose();
            return;
        }

        cameraIdleTimer += Time.deltaTime;
        if (cameraIdleTimer >= cameraIdleTimeToRecenter)
        {
            BeginRecenterToCamera();
        }
        else
        {
            KeepLockedTablePose();
        }
    }

    private void KeepLockedTablePose()
    {
        if (isTablePoseLocked && spawnedTable != null)
        {
            spawnedTable.transform.SetPositionAndRotation(lockedTablePose.position, lockedTablePose.rotation);
            spawnedTable.transform.localScale = Vector3.one * tableWorldScale;
        }
    }
    private void BeginRecenterToCamera()
    {
        recenterStartPose = new Pose(spawnedTable.transform.position, spawnedTable.transform.rotation);
        recenterTargetPose = GetCameraCenteredTablePose();
        recenterElapsed = 0f;
        isRecentering = true;
        cameraMovedSinceLastRecenter = false;
        cameraIdleTimer = 0f;
    }

    private void UpdateRecenterAnimation()
    {
        if (!isRecentering || spawnedTable == null)
        {
            return;
        }

        recenterElapsed += Time.deltaTime;
        float t = recenterDuration <= 0f ? 1f : Mathf.Clamp01(recenterElapsed / recenterDuration);
        float easedT = Mathf.SmoothStep(0f, 1f, t);

        spawnedTable.transform.position = Vector3.Lerp(recenterStartPose.position, recenterTargetPose.position, easedT);
        spawnedTable.transform.rotation = Quaternion.Slerp(recenterStartPose.rotation, recenterTargetPose.rotation, easedT);
        spawnedTable.transform.localScale = Vector3.one * tableWorldScale;

        if (t >= 1f)
        {
            isRecentering = false;
            isTablePoseLocked = true;
            lockedTablePose = recenterTargetPose;
            CaptureCameraPoseBaseline();
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
