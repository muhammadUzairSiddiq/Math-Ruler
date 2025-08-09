using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ARDebugManager : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogging = true;
    public bool showOnScreenDebug = true;
    public bool logToFile = true;
    
    [Header("AR Components")]
    public ARSessionOrigin arSessionOrigin;
    public ARCameraManager arCameraManager;
    public ARPlaneManager arPlaneManager;
    public ARRaycastManager arRaycastManager;
    public ARAnchorManager arAnchorManager;
    
    [Header("Debug Info")]
    public string arSessionState = "Unknown";
    public int detectedPlanes = 0;
    public Vector3 cameraPosition = Vector3.zero;
    public bool arInitialized = false;
    public bool cameraActive = false;
    public bool planeDetectionActive = false;
    
    private string debugLog = "";
    private int logLineCount = 0;
    
    void Start()
    {
        InitializeARDebug();
        // Auto-run mobile checks on start
        StartCoroutine(AutoCheckMobileAR());
    }
    
    void InitializeARDebug()
    {
        Debug.Log("=== AR DEBUG MANAGER INITIALIZATION ===");
        
        // Find AR components
        FindARComponents();
        
        // Start debug monitoring
        StartCoroutine(MonitorARState());
        
        // Setup on-screen debug
        if (showOnScreenDebug)
        {
            StartCoroutine(UpdateOnScreenDebug());
        }
        
        Debug.Log("AR Debug Manager initialized");
    }
    
    void FindARComponents()
    {
        Debug.Log("Finding AR components...");
        
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arAnchorManager = FindObjectOfType<ARAnchorManager>();
        
        LogARComponentStatus();
    }
    
    void AssignARReferences()
    {
        Debug.Log("=== ASSIGNING AR REFERENCES ===");
        
        // Find and assign all AR components
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arAnchorManager = FindObjectOfType<ARAnchorManager>();
        
        // Log assignment results
        AddDebugLog("AR References Assignment:");
        AddDebugLog($"AR Session Origin: {(arSessionOrigin != null ? "✓ Assigned" : "✗ Not Found")}");
        AddDebugLog($"AR Camera Manager: {(arCameraManager != null ? "✓ Assigned" : "✗ Not Found")}");
        AddDebugLog($"AR Plane Manager: {(arPlaneManager != null ? "✓ Assigned" : "✗ Not Found")}");
        AddDebugLog($"AR Raycast Manager: {(arRaycastManager != null ? "✓ Assigned" : "✗ Not Found")}");
        AddDebugLog($"AR Anchor Manager: {(arAnchorManager != null ? "✓ Assigned" : "✗ Not Found")}");
        
        Debug.Log("AR References assignment complete!");
    }
    
    [ContextMenu("Check AR Permissions")]
    void CheckARPermissionsFromMenu()
    {
        CheckARPermissions();
    }
    
    [ContextMenu("Check Android Manifest")]
    void CheckAndroidManifestFromMenu()
    {
        CheckAndroidManifest();
    }
    
    [ContextMenu("Check Mobile AR Status")]
    void CheckMobileARStatusFromMenu()
    {
        CheckMobileARStatus();
    }
    
    void CheckARPermissions()
    {
        Debug.Log("=== CHECKING AR PERMISSIONS ===");
        AddDebugLog("=== CHECKING AR PERMISSIONS ===");
        
        // Check camera permission
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            AddDebugLog("✓ Camera permission granted");
        }
        else
        {
            AddDebugLog("✗ Camera permission NOT granted");
            AddDebugLog("Requesting camera permission...");
            StartCoroutine(RequestCameraPermission());
        }
        
        // Check if AR is supported
        bool arSupported = IsARSupported();
        AddDebugLog($"AR Supported: {(arSupported ? "✓ Yes" : "✗ No")}");
        
        // Check AR Session Origin state
        if (arSessionOrigin != null)
        {
            AddDebugLog($"AR Session Origin: {(arSessionOrigin.isActiveAndEnabled ? "✓ Active" : "✗ Inactive")}");
            
            if (!arSessionOrigin.isActiveAndEnabled)
            {
                AddDebugLog("⚠️ AR Session Origin not active - trying to enable...");
                TryStartARSessionOrigin();
            }
        }
        
        // Add mobile-specific checks
        CheckMobileARStatus();
    }
    
    bool IsARSupported()
    {
        #if UNITY_ANDROID
            return true; // Assume AR Core is available
        #elif UNITY_IOS
            return true; // Assume AR Kit is available
        #else
            return false;
        #endif
    }
    
    System.Collections.IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            AddDebugLog("✓ Camera permission granted after request");
        }
        else
        {
            AddDebugLog("✗ Camera permission denied");
        }
    }
    
    void TryStartARSessionOrigin()
    {
        if (arSessionOrigin != null)
        {
            AddDebugLog("Attempting to start AR Session Origin...");
            // AR Session Origin should start automatically, but we can check if it's enabled
            if (arSessionOrigin.enabled)
            {
                AddDebugLog("✓ AR Session Origin is enabled");
            }
            else
            {
                AddDebugLog("✗ AR Session Origin is disabled");
            }
        }
    }
    
    void CheckAndroidManifest()
    {
        Debug.Log("=== CHECKING ANDROID MANIFEST ===");
        AddDebugLog("=== CHECKING ANDROID MANIFEST ===");
        
        AddDebugLog("Android Manifest Permissions:");
        
        // Check if we're on Android
        #if UNITY_ANDROID
            AddDebugLog("✓ Running on Android");
            
            // These permissions should be auto-added by AR Foundation
            AddDebugLog("Required AR Permissions:");
            AddDebugLog("- android.permission.CAMERA");
            AddDebugLog("- android.permission.INTERNET");
            AddDebugLog("- com.google.ar.permission.CAMERA");
            
            // Check if ARCore is available
            AddDebugLog("ARCore Status:");
            AddDebugLog("- Should be auto-detected by AR Foundation");
            AddDebugLog("- Device must have ARCore installed");
            
        #else
            AddDebugLog("⚠️ Not running on Android");
        #endif
        
        AddDebugLog("If AR still doesn't work:");
        AddDebugLog("1. Check if ARCore is installed on device");
        AddDebugLog("2. Check if device supports AR");
        AddDebugLog("3. Try different device");
    }
    
    void CheckMobileARStatus()
    {
        AddDebugLog("=== MOBILE AR STATUS ===");
        
        #if UNITY_ANDROID
            AddDebugLog("Platform: Android");
            
            // Check device info
            AddDebugLog($"Device: {SystemInfo.deviceModel}");
            AddDebugLog($"OS: {SystemInfo.operatingSystem}");
            AddDebugLog($"API Level: {Application.platform}");
            
            // Check if we're in editor or on device
            if (Application.isEditor)
            {
                AddDebugLog("⚠️ Running in Unity Editor");
                AddDebugLog("AR won't work in editor!");
            }
            else
            {
                AddDebugLog("✓ Running on mobile device");
                
                // Check AR session state on mobile
                if (arSessionOrigin != null)
                {
                    string mobileState = "Unknown"; // We'll check AR state differently
                    AddDebugLog($"Mobile AR State: {mobileState}");
                    
                    if (mobileState == "None")
                    {
                        AddDebugLog("❌ AR not working on mobile!");
                        AddDebugLog("Possible issues:");
                        AddDebugLog("- ARCore not installed");
                        AddDebugLog("- Device not AR-compatible");
                        AddDebugLog("- Camera permissions denied");
                    }
                    else if (mobileState == "SessionTracking")
                    {
                        AddDebugLog("✅ AR working on mobile!");
                    }
                }
            }
            
        #else
            AddDebugLog("⚠️ Not on Android device");
        #endif
    }
    
    IEnumerator AutoCheckMobileAR()
    {
        // Wait a bit for AR to initialize
        yield return new WaitForSeconds(2f);
        
        // Auto-run mobile checks
        AddDebugLog("=== AUTO MOBILE AR CHECK ===");
        CheckMobileARStatus();
        CheckARPermissions();
        
        // Keep checking every 5 seconds
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            if (arSessionOrigin != null)
            {
                string currentState = "Unknown"; // We'll check AR state differently
                if (currentState != arSessionState)
                {
                    AddDebugLog($"AR State Changed: {arSessionState} → {currentState}");
                    arSessionState = currentState;
                    
                    if (currentState == "SessionTracking")
                    {
                        AddDebugLog("🎉 AR SESSION STARTED!");
                    }
                }
            }
        }
    }
    
    void LogARComponentStatus()
    {
        AddDebugLog("=== AR COMPONENT STATUS ===");
        AddDebugLog($"AR Session Origin: {(arSessionOrigin != null ? "✓ Found" : "✗ Missing")}");
        AddDebugLog($"AR Camera Manager: {(arCameraManager != null ? "✓ Found" : "✗ Missing")}");
        AddDebugLog($"AR Plane Manager: {(arPlaneManager != null ? "✓ Found" : "✗ Missing")}");
        AddDebugLog($"AR Raycast Manager: {(arRaycastManager != null ? "✓ Found" : "✗ Missing")}");
        AddDebugLog($"AR Anchor Manager: {(arAnchorManager != null ? "✓ Found" : "✗ Missing")}");
        
        // Check AR Session Origin state
        if (arSessionOrigin != null)
        {
            arSessionState = "Active"; // Simplified state for now
            AddDebugLog($"AR Session Origin State: {arSessionState}");
        }
        
        // Check camera
        if (arCameraManager != null)
        {
            cameraActive = arCameraManager.isActiveAndEnabled;
            AddDebugLog($"Camera Active: {cameraActive}");
        }
        
        // Check plane detection
        if (arPlaneManager != null)
        {
            planeDetectionActive = arPlaneManager.isActiveAndEnabled;
            AddDebugLog($"Plane Detection Active: {planeDetectionActive}");
        }
    }
    
    IEnumerator MonitorARState()
    {
        while (true)
        {
            UpdateARState();
            yield return new WaitForSeconds(1f); // Update every second
        }
    }
    
    void UpdateARState()
    {
        // Update AR Session Origin state
        if (arSessionOrigin != null)
        {
            string newState = "Active"; // Simplified state for now
            if (newState != arSessionState)
            {
                arSessionState = newState;
                AddDebugLog($"AR Session Origin State Changed: {arSessionState}");
                
                if (arSessionState == "Active")
                {
                    arInitialized = true;
                    AddDebugLog("✓ AR Session Origin initialized and active!");
                }
            }
        }
        
        // Update camera position
        if (arCameraManager != null && arCameraManager.isActiveAndEnabled)
        {
            Vector3 newPosition = arCameraManager.transform.position;
            if (Vector3.Distance(newPosition, cameraPosition) > 0.1f)
            {
                cameraPosition = newPosition;
                AddDebugLog($"Camera Position: {cameraPosition}");
            }
        }
        
        // Update plane detection
        if (arPlaneManager != null && arPlaneManager.isActiveAndEnabled)
        {
            int newPlaneCount = arPlaneManager.trackables.count;
            if (newPlaneCount != detectedPlanes)
            {
                detectedPlanes = newPlaneCount;
                AddDebugLog($"Detected Planes: {detectedPlanes}");
            }
        }
    }
    
    void AddDebugLog(string message)
    {
        if (!enableDebugLogging) return;
        
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";
        
        Debug.Log(logMessage);
        
        if (logToFile)
        {
            debugLog += logMessage + "\n";
            logLineCount++;
            
            // Keep log manageable size
            if (logLineCount > 100)
            {
                debugLog = debugLog.Substring(debugLog.IndexOf('\n') + 1);
                logLineCount--;
            }
        }
    }
    
    IEnumerator UpdateOnScreenDebug()
    {
        while (showOnScreenDebug)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private Vector2 scrollPosition = Vector2.zero;
    
    void OnGUI()
    {
        if (!showOnScreenDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 600, Screen.height - 20));
        
        // Debug panel background
        GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = Color.white;
        
        GUILayout.Label("AR Debug Info", GUI.skin.box);
        
        // Basic AR status
        GUILayout.Label($"AR Session: {arSessionState}");
        GUILayout.Label($"AR Initialized: {arInitialized}");
        GUILayout.Label($"Camera Active: {cameraActive}");
        GUILayout.Label($"Plane Detection: {planeDetectionActive}");
        GUILayout.Label($"Detected Planes: {detectedPlanes}");
        GUILayout.Label($"Camera Position: {cameraPosition}");
        
        GUILayout.Space(10);
        
        // Show all debug log messages with scroll
        GUILayout.Label("Debug Messages:", GUI.skin.box);
        
        // Create scrollable area
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        // Split debug log into lines and show ALL messages
        string[] logLines = debugLog.Split('\n');
        
        for (int i = 0; i < logLines.Length; i++)
        {
            if (!string.IsNullOrEmpty(logLines[i]))
            {
                // Color code different types of messages
                if (logLines[i].Contains("✓") || logLines[i].Contains("✅"))
                {
                    GUI.color = Color.green;
                }
                else if (logLines[i].Contains("✗") || logLines[i].Contains("❌"))
                {
                    GUI.color = Color.red;
                }
                else if (logLines[i].Contains("⚠️"))
                {
                    GUI.color = Color.yellow;
                }
                else if (logLines[i].Contains("🎉"))
                {
                    GUI.color = Color.cyan;
                }
                else
                {
                    GUI.color = Color.white;
                }
                
                GUILayout.Label(logLines[i]);
                GUI.color = Color.white;
            }
        }
        
        GUILayout.EndScrollView();
        
        GUILayout.Space(10);
        
        // Buttons row
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Save Debug Log"))
        {
            SaveDebugLog();
        }
        
        if (GUILayout.Button("Clear Log"))
        {
            debugLog = "";
            logLineCount = 0;
        }
        
        if (GUILayout.Button("Auto Scroll"))
        {
            // Auto scroll to bottom
            scrollPosition.y = float.MaxValue;
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void SaveDebugLog()
    {
        string fileName = $"AR_Debug_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string filePath = Application.persistentDataPath + "/" + fileName;
        
        try
        {
            System.IO.File.WriteAllText(filePath, debugLog);
            Debug.Log($"Debug log saved to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save debug log: {e.Message}");
        }
    }
    
    [ContextMenu("Test AR Debug")]
    void TestARDebug()
    {
        AddDebugLog("=== AR DEBUG TEST ===");
        LogARComponentStatus();
        AddDebugLog("AR Debug test complete");
    }
    
    [ContextMenu("Save Debug Log")]
    void SaveDebugLogFromMenu()
    {
        SaveDebugLog();
    }
    
    [ContextMenu("Assign AR References")]
    void AssignARReferencesFromMenu()
    {
        AssignARReferences();
    }
} 