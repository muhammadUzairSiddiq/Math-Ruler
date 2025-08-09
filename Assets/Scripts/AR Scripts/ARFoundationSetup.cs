using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ARFoundationSetup : MonoBehaviour
{
    [Header("AR Core Components")]
    public ARSession arSession;
    public ARSessionOrigin arSessionOrigin;
    public ARCameraManager arCameraManager;
    public ARPlaneManager arPlaneManager;
    public ARRaycastManager arRaycastManager;
    public ARAnchorManager arAnchorManager;
    
    [Header("Setup Settings")]
    public bool autoSetupOnStart = true;
    public bool enablePlaneDetection = true;
    public bool enableRaycasting = true;
    public bool enableAnchors = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool logARState = true;
    
    private bool isSetupComplete = false;
    private bool isARSessionActive = false;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            StartCoroutine(SetupARFoundation());
        }
    }
    
    [ContextMenu("Setup AR Foundation")]
    public void SetupARFoundationFromMenu()
    {
        StartCoroutine(SetupARFoundation());
    }
    
    IEnumerator SetupARFoundation()
    {
        Debug.Log("=== AR FOUNDATION SETUP STARTED ===");
        
        // Step 1: Create or find AR Session
        yield return StartCoroutine(SetupARSession());
        
        // Step 2: Create or find AR Session Origin
        yield return StartCoroutine(SetupARSessionOrigin());
        
        // Step 3: Setup AR Camera
        yield return StartCoroutine(SetupARCamera());
        
        // Step 4: Setup AR Managers
        yield return StartCoroutine(SetupARManagers());
        
        // Step 5: Verify setup
        yield return StartCoroutine(VerifyARSetup());
        
        // Step 6: Start AR Session
        yield return StartCoroutine(StartARSession());
        
        Debug.Log("=== AR FOUNDATION SETUP COMPLETE ===");
        isSetupComplete = true;
    }
    
    IEnumerator SetupARSession()
    {
        Debug.Log("Setting up AR Session...");
        
        // Find existing AR Session
        arSession = FindObjectOfType<ARSession>();
        
        if (arSession == null)
        {
            Debug.Log("Creating new AR Session...");
            GameObject sessionGO = new GameObject("AR Session");
            arSession = sessionGO.AddComponent<ARSession>();
        }
        else
        {
            Debug.Log("Found existing AR Session");
        }
        
        // Subscribe to session state changes
        if (arSession != null)
        {
            ARSession.stateChanged += OnARSessionStateChanged;
            Debug.Log("✅ AR Session setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to create AR Session");
        }
        
        yield return null;
    }
    
    IEnumerator SetupARSessionOrigin()
    {
        Debug.Log("Setting up AR Session Origin...");
        
        // Find existing AR Session Origin
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        
        if (arSessionOrigin == null)
        {
            Debug.Log("Creating new AR Session Origin...");
            GameObject originGO = new GameObject("AR Session Origin");
            arSessionOrigin = originGO.AddComponent<ARSessionOrigin>();
        }
        else
        {
            Debug.Log("Found existing AR Session Origin");
        }
        
        if (arSessionOrigin != null)
        {
            Debug.Log("✅ AR Session Origin setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to create AR Session Origin");
        }
        
        yield return null;
    }
    
    IEnumerator SetupARCamera()
    {
        Debug.Log("Setting up AR Camera...");
        
        // Find existing camera
        Camera arCamera = FindObjectOfType<Camera>();
        if (arCamera == null)
        {
            Debug.Log("Creating new AR Camera...");
            GameObject cameraGO = new GameObject("AR Camera");
            arCamera = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
        }
        
        // Setup AR Camera Manager
        arCameraManager = arCamera.GetComponent<ARCameraManager>();
        if (arCameraManager == null)
        {
            Debug.Log("Adding AR Camera Manager...");
            arCameraManager = arCamera.gameObject.AddComponent<ARCameraManager>();
        }
        
        // Parent camera to AR Session Origin
        if (arSessionOrigin != null)
        {
            arCamera.transform.SetParent(arSessionOrigin.transform);
            Debug.Log("✅ Camera parented to AR Session Origin");
        }
        else
        {
            Debug.LogError("❌ AR Session Origin not found - cannot parent camera");
        }
        
        // Configure camera for AR
        if (arCamera != null)
        {
            arCamera.clearFlags = CameraClearFlags.SolidColor;
            arCamera.backgroundColor = Color.black;
            arCamera.nearClipPlane = 0.1f;
            arCamera.farClipPlane = 1000f;
            Debug.Log("✅ Camera configured for AR");
        }
        
        if (arCameraManager != null)
        {
            Debug.Log("✅ AR Camera setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to setup AR Camera");
        }
        
        yield return null;
    }
    
    IEnumerator SetupARManagers()
    {
        Debug.Log("Setting up AR Managers...");
        
        // Setup AR Plane Manager
        yield return StartCoroutine(SetupARPlaneManager());
        
        // Setup AR Raycast Manager
        yield return StartCoroutine(SetupARRaycastManager());
        
        // Setup AR Anchor Manager
        yield return StartCoroutine(SetupARAnchorManager());
        
        Debug.Log("✅ AR Managers setup complete");
    }
    
    IEnumerator SetupARPlaneManager()
    {
        Debug.Log("Setting up AR Plane Manager...");
        
        // Find existing AR Plane Manager
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        
        if (arPlaneManager == null)
        {
            Debug.Log("Creating new AR Plane Manager...");
            GameObject planeManagerGO = new GameObject("AR Plane Manager");
            
            if (arSessionOrigin != null)
            {
                planeManagerGO.transform.SetParent(arSessionOrigin.transform);
            }
            
            arPlaneManager = planeManagerGO.AddComponent<ARPlaneManager>();
        }
        
        if (arPlaneManager != null)
        {
            // Configure plane detection
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            Debug.Log("✅ AR Plane Manager setup complete");
            Debug.Log($"Plane Detection Mode: {arPlaneManager.requestedDetectionMode}");
        }
        else
        {
            Debug.LogError("❌ Failed to setup AR Plane Manager");
        }
        
        yield return null;
    }
    
    IEnumerator SetupARRaycastManager()
    {
        Debug.Log("Setting up AR Raycast Manager...");
        
        // Find existing AR Raycast Manager
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        
        if (arRaycastManager == null)
        {
            Debug.Log("Creating new AR Raycast Manager...");
            GameObject raycastManagerGO = new GameObject("AR Raycast Manager");
            
            if (arSessionOrigin != null)
            {
                raycastManagerGO.transform.SetParent(arSessionOrigin.transform);
            }
            
            arRaycastManager = raycastManagerGO.AddComponent<ARRaycastManager>();
        }
        
        if (arRaycastManager != null)
        {
            Debug.Log("✅ AR Raycast Manager setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to setup AR Raycast Manager");
        }
        
        yield return null;
    }
    
    IEnumerator SetupARAnchorManager()
    {
        Debug.Log("Setting up AR Anchor Manager...");
        
        // Find existing AR Anchor Manager
        arAnchorManager = FindObjectOfType<ARAnchorManager>();
        
        if (arAnchorManager == null)
        {
            Debug.Log("Creating new AR Anchor Manager...");
            GameObject anchorManagerGO = new GameObject("AR Anchor Manager");
            
            if (arSessionOrigin != null)
            {
                anchorManagerGO.transform.SetParent(arSessionOrigin.transform);
            }
            
            arAnchorManager = anchorManagerGO.AddComponent<ARAnchorManager>();
        }
        
        if (arAnchorManager != null)
        {
            Debug.Log("✅ AR Anchor Manager setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to setup AR Anchor Manager");
        }
        
        yield return null;
    }
    
    IEnumerator VerifyARSetup()
    {
        Debug.Log("=== VERIFYING AR SETUP ===");
        
        bool allComponentsFound = true;
        
        // Check AR components
        if (arSession == null) { Debug.LogError("❌ AR Session missing!"); allComponentsFound = false; }
        if (arSessionOrigin == null) { Debug.LogError("❌ AR Session Origin missing!"); allComponentsFound = false; }
        if (arCameraManager == null) { Debug.LogError("❌ AR Camera Manager missing!"); allComponentsFound = false; }
        if (arPlaneManager == null) { Debug.LogError("❌ AR Plane Manager missing!"); allComponentsFound = false; }
        if (arRaycastManager == null) { Debug.LogError("❌ AR Raycast Manager missing!"); allComponentsFound = false; }
        if (arAnchorManager == null) { Debug.LogError("❌ AR Anchor Manager missing!"); allComponentsFound = false; }
        
        if (allComponentsFound)
        {
            Debug.Log("✅ All AR components found!");
        }
        else
        {
            Debug.LogError("❌ Some AR components are missing!");
        }
        
        yield return null;
    }
    
    IEnumerator StartARSession()
    {
        Debug.Log("Starting AR Session...");
        
        if (arSession != null)
        {
            // Start AR Session
            arSession.enabled = true;
            
            // Wait for session to initialize
            yield return new WaitForSeconds(1f);
            
            Debug.Log($"AR Session State: {ARSession.state}");
            
            if (ARSession.state == ARSessionState.SessionTracking)
            {
                isARSessionActive = true;
                Debug.Log("✅ AR Session is active and tracking!");
            }
            else
            {
                Debug.LogWarning($"⚠️ AR Session state: {ARSession.state}");
            }
        }
        else
        {
            Debug.LogError("❌ Cannot start AR Session - not found!");
        }
    }
    
    void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        if (logARState)
        {
            Debug.Log($"AR Session State Changed: {args.state}");
        }
        
        switch (args.state)
        {
            case ARSessionState.None:
                Debug.Log("AR Session: None");
                break;
            case ARSessionState.Unsupported:
                Debug.LogError("AR Session: Unsupported on this device");
                break;
            case ARSessionState.CheckingAvailability:
                Debug.Log("AR Session: Checking availability...");
                break;
            case ARSessionState.NeedsInstall:
                Debug.Log("AR Session: Needs installation");
                break;
            case ARSessionState.Installing:
                Debug.Log("AR Session: Installing...");
                break;
            case ARSessionState.Ready:
                Debug.Log("✅ AR Session: Ready");
                break;
            case ARSessionState.SessionInitializing:
                Debug.Log("AR Session: Initializing...");
                break;
            case ARSessionState.SessionTracking:
                Debug.Log("✅ AR Session: Tracking active");
                isARSessionActive = true;
                break;
        }
    }
    
    [ContextMenu("Check AR Status")]
    public void CheckARStatus()
    {
        Debug.Log("=== AR STATUS CHECK ===");
        
        // Check AR Session
        if (arSession != null)
        {
            Debug.Log($"AR Session: {(arSession.enabled ? "✓ Enabled" : "✗ Disabled")}");
            Debug.Log($"AR Session State: {ARSession.state}");
        }
        else
        {
            Debug.LogError("❌ AR Session not found");
        }
        
        // Check AR Session Origin
        if (arSessionOrigin != null)
        {
            Debug.Log($"AR Session Origin: {(arSessionOrigin.enabled ? "✓ Enabled" : "✗ Disabled")}");
        }
        else
        {
            Debug.LogError("❌ AR Session Origin not found");
        }
        
        // Check AR Camera Manager
        if (arCameraManager != null)
        {
            Debug.Log($"AR Camera Manager: {(arCameraManager.enabled ? "✓ Enabled" : "✗ Disabled")}");
        }
        else
        {
            Debug.LogError("❌ AR Camera Manager not found");
        }
        
        // Check AR Plane Manager
        if (arPlaneManager != null)
        {
            Debug.Log($"AR Plane Manager: {(arPlaneManager.enabled ? "✓ Enabled" : "✗ Disabled")}");
            Debug.Log($"Plane Detection Mode: {arPlaneManager.requestedDetectionMode}");
            Debug.Log($"Current Planes: {arPlaneManager.trackables.count}");
        }
        else
        {
            Debug.LogError("❌ AR Plane Manager not found");
        }
        
        // Check AR Raycast Manager
        if (arRaycastManager != null)
        {
            Debug.Log($"AR Raycast Manager: {(arRaycastManager.enabled ? "✓ Enabled" : "✗ Disabled")}");
        }
        else
        {
            Debug.LogError("❌ AR Raycast Manager not found");
        }
        
        // Check AR Anchor Manager
        if (arAnchorManager != null)
        {
            Debug.Log($"AR Anchor Manager: {(arAnchorManager.enabled ? "✓ Enabled" : "✗ Disabled")}");
        }
        else
        {
            Debug.LogError("❌ AR Anchor Manager not found");
        }
    }
    
    [ContextMenu("Test Plane Detection")]
    public void TestPlaneDetection()
    {
        Debug.Log("=== TESTING PLANE DETECTION ===");
        
        if (arPlaneManager != null)
        {
            var planes = arPlaneManager.trackables;
            Debug.Log($"Detected planes: {planes.count}");
            
            foreach (var plane in planes)
            {
                Debug.Log($"Plane ID: {plane.trackableId}, Center: {plane.center}, Size: {plane.size}");
            }
        }
        else
        {
            Debug.LogError("❌ AR Plane Manager not found");
        }
    }
    
    [ContextMenu("Force AR Restart")]
    public void ForceARRestart()
    {
        Debug.Log("=== FORCING AR RESTART ===");
        
        if (arSession != null)
        {
            arSession.enabled = false;
            StartCoroutine(RestartARSession());
        }
        else
        {
            Debug.LogError("❌ AR Session not found");
        }
    }
    
    [ContextMenu("Fix XROrigin Camera Issue")]
    public void FixXROriginCameraIssue()
    {
        Debug.Log("=== FIXING XR ORIGIN CAMERA ISSUE ===");
        
        // Find any GameObject with "XR" or "Origin" in the name that might be the XR Origin
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        GameObject xrOrigin = null;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("XR") && obj.name.Contains("Origin") || 
                obj.name.Contains("Origin") && obj.name.Contains("AR"))
            {
                xrOrigin = obj;
                Debug.Log($"Found potential XR Origin: {obj.name}");
                break;
            }
        }
        
        if (xrOrigin != null)
        {
            Debug.Log("Found XR Origin, checking camera setup...");
            
            // Find camera under XR Origin
            Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (xrCamera != null)
            {
                Debug.Log("Found camera under XR Origin");
                
                // Add AR Camera Manager if missing
                var cameraManager = xrCamera.GetComponent<ARCameraManager>();
                if (cameraManager == null)
                {
                    Debug.Log("Adding AR Camera Manager to XR Camera...");
                    cameraManager = xrCamera.gameObject.AddComponent<ARCameraManager>();
                }
                
                // Update our reference
                arCameraManager = cameraManager;
                
                Debug.Log("✅ XR Origin camera issue fixed");
            }
            else
            {
                Debug.LogError("❌ No camera found under XR Origin");
            }
        }
        else
        {
            Debug.Log("No XR Origin found, using standard AR setup");
        }
    }
    
    IEnumerator RestartARSession()
    {
        Debug.Log("Restarting AR Session...");
        
        // Wait a moment
        yield return new WaitForSeconds(0.5f);
        
        // Re-enable AR Session
        if (arSession != null)
        {
            arSession.enabled = true;
            Debug.Log("✅ AR Session restarted");
        }
    }
    
    void Update()
    {
        if (showDebugInfo && isARSessionActive)
        {
            // Monitor plane detection
            if (arPlaneManager != null)
            {
                var planes = arPlaneManager.trackables;
                if (planes.count > 0)
                {
                    Debug.Log($"Planes detected: {planes.count}");
                }
            }
        }
    }
} 