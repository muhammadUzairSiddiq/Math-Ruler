using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// AR Placement Manager - Fixed Version
/// 
/// Key Fixes:
/// 1. Proper plane detection and placement
/// 2. Fixed positioning on detected surface
/// 3. No camera distance logic - cubes stay fixed
/// 4. Proper surface detection and validation
/// 5. Hide plane visualization after placement
/// </summary>
public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARSessionOrigin sessionOrigin;
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;
    
    [Header("Placement Settings")]
    public bool autoPlaceOnDetection = true;
    public float placementHeight = 0.02f; // Small offset above surface
    public float minPlaneSize = 1.0f; // Minimum plane size for placement
    
    [Header("UI Integration")]
    public GameObject placementUI;
    public bool hidePlacementUI = true;
    
    private bool isPlaced = false;
    private GameObject placedNumberLine;
    private ARNumberLineGenerator numberLineGenerator;
    private ARPlane bestPlane;
    
    void Start()
    {
        InitializePlacement();
        
        // Hide placement UI immediately
        Invoke("HideARPlacementUI", 0.1f);
    }
    
    void InitializePlacement()
    {
        Debug.Log("=== AR PLACEMENT MANAGER INITIALIZATION ===");
        
        // Find AR components
        if (sessionOrigin == null) sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (planeManager == null) planeManager = FindObjectOfType<ARPlaneManager>();
        if (raycastManager == null) raycastManager = FindObjectOfType<ARRaycastManager>();
        
        // Find or create number line generator
        numberLineGenerator = FindObjectOfType<ARNumberLineGenerator>();
        if (numberLineGenerator == null)
        {
            Debug.Log("Creating AR Number Line Generator...");
            GameObject generatorGO = new GameObject("AR Number Line Generator");
            numberLineGenerator = generatorGO.AddComponent<ARNumberLineGenerator>();
            Debug.Log($"✅ Created AR Number Line Generator: {numberLineGenerator != null}");
        }
        else
        {
            Debug.Log($"✅ Found existing AR Number Line Generator: {numberLineGenerator.name}");
        }
        
        // Subscribe to plane detection
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("✅ Plane Manager found and subscribed to plane changes");
            
            // Ensure plane detection is enabled
            if (planeManager.requestedDetectionMode == PlaneDetectionMode.None)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                Debug.Log("✅ Enabled horizontal plane detection");
            }
        }
        else
        {
            Debug.LogError("❌ Plane Manager not found!");
        }
        
        Debug.Log("AR Placement Manager initialized");
    }
    
    [ContextMenu("Test Obstacle Detection")]
    void TestObstacleDetection()
    {
        Debug.Log("=== TESTING OBSTACLE DETECTION ===");
        
        if (planeManager == null)
        {
            Debug.LogError("Plane Manager not found!");
            return;
        }
        
        foreach (var plane in planeManager.trackables)
        {
            bool hasObstacles = HasObstaclesOnPlane(plane);
            Debug.Log($"Plane {plane.trackableId}: {(hasObstacles ? "HAS OBSTACLES" : "CLEAR")}");
        }
        
        Debug.Log("=== OBSTACLE DETECTION TEST COMPLETE ===");
    }

    [ContextMenu("Force Hide Plane Visualization")]
    void ForceHidePlaneVisualization()
    {
        Debug.Log("=== FORCING PLANE VISUALIZATION HIDE ===");
        HidePlaneVisualization();
        Debug.Log("=== PLANE VISUALIZATION HIDE COMPLETE ===");
    }

    [ContextMenu("Force Reactivate All UI")]
    void ForceReactivateAllUI()
    {
        Debug.Log("=== FORCING REACTIVATION OF ALL UI ===");
        
        // Find and reactivate all UI elements
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (!canvas.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
                Debug.Log($"✅ Reactivated Canvas: {canvas.name}");
            }
            
            Transform[] children = canvas.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (!child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"✅ Reactivated UI element: {child.name}");
                }
            }
        }
        
        // Also reactivate any deactivated GameObjects with UI components
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeInHierarchy && 
                (obj.GetComponent<UnityEngine.UI.Button>() != null || 
                 obj.GetComponent<UnityEngine.UI.Text>() != null ||
                 obj.GetComponent<TMPro.TextMeshProUGUI>() != null ||
                 obj.GetComponent<UnityEngine.UI.Image>() != null))
            {
                obj.SetActive(true);
                Debug.Log($"✅ Reactivated UI component: {obj.name}");
            }
        }
        
        Debug.Log("=== UI REACTIVATION COMPLETE ===");
    }

    void HideARPlacementUI()
    {
        Debug.Log("Hiding placement UI");
        
        // Find and hide ONLY AR Foundation placement UI elements (be more specific)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Transform[] children = canvas.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                // Only hide specific AR Foundation placement UI elements, not general UI
                if (child.name.Contains("Tap to Place") || child.name.Contains("AR Placement") || 
                    child.name.Contains("AR Instruction") || child.name.Contains("AR Foundation") ||
                    child.name.Contains("Placement UI") || child.name.Contains("AR UI"))
                {
                    child.gameObject.SetActive(false);
                    Debug.Log($"Hidden AR placement UI: {child.name}");
                }
            }
        }
        
        // Also hide any UI elements with specific AR Foundation text
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            var textComponent = obj.GetComponent<UnityEngine.UI.Text>();
            if (textComponent != null)
            {
                string text = textComponent.text.ToLower();
                if (text.Contains("tap to place") || text.Contains("touch surfaces") ||
                    text.Contains("ar foundation") || text.Contains("placement"))
                {
                    obj.SetActive(false);
                    Debug.Log($"Hidden AR text UI: {obj.name} - '{textComponent.text}'");
                }
            }
        }
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log($"Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
        
        if (autoPlaceOnDetection && !isPlaced)
        {
            // Find the best plane for placement
            ARPlane bestPlane = FindBestPlane();
            if (bestPlane != null)
            {
                Debug.Log($"Best plane found: {bestPlane.trackableId} at {bestPlane.center} with size {bestPlane.size}");
                PlaceNumberLineOnPlane(bestPlane);
            }
        }
    }
    
    ARPlane FindBestPlane()
    {
        if (planeManager == null) return null;
        ARPlane bestPlane = null;
        float bestScore = 0f;
        foreach (var plane in planeManager.trackables)
        {
            // Only consider horizontal planes (normal close to Vector3.up)
            float angle = Vector3.Angle(plane.normal, Vector3.up);
            if (angle > 10f) continue; // Not flat enough
            // Set plane mesh/line renderer to red for debugging
            var meshRenderer = plane.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.material.color = Color.red;
            var lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null) lineRenderer.material.color = Color.red;
            float score = CalculatePlaneScore(plane);
            Debug.Log($"Plane {plane.trackableId} score: {score} (size: {plane.size}, angle: {angle})");
            if (score > bestScore)
            {
                bestScore = score;
                bestPlane = plane;
            }
        }
        return bestPlane;
    }
    // TODO: Before placing the number line, raycast upwards from each intended cube position to check for obstacles (shelves, walls).
    
    float CalculatePlaneScore(ARPlane plane)
    {
        if (plane == null) return 0f;
        
        // Check if plane is too small
        if (plane.size.x < minPlaneSize || plane.size.y < minPlaneSize) return 0f;
        
        // Calculate score based on size (prefer larger planes)
        float sizeScore = Mathf.Min(plane.size.x, plane.size.y) / minPlaneSize;
        
        // Prefer larger planes
        return sizeScore;
    }
    
    void PlaceNumberLineOnPlane(ARPlane plane)
    {
        if (isPlaced) return;
        
        Debug.Log($"Placing number line on plane {plane.trackableId} at {plane.center}");
        
        // Store the best plane for reference
        bestPlane = plane;
        
        // Check for obstacles before placement
        if (HasObstaclesOnPlane(plane))
        {
            Debug.Log("Obstacles detected on plane, trying to find alternative placement...");
            ARPlane alternativePlane = FindNextBestPlane(plane);
            if (alternativePlane != null)
            {
                Debug.Log($"Found alternative plane {alternativePlane.trackableId}, placing there instead");
                PlaceNumberLineOnPlane(alternativePlane);
                return;
            }
            else
            {
                Debug.LogWarning("No clear plane found for placement. Number line will not be placed.");
                return;
            }
        }
        
        // Calculate placement position
        Vector3 placementPosition = plane.center;
        placementPosition.y += placementHeight; // Small offset above surface
        
        // Generate the number line
        if (numberLineGenerator != null)
        {
            // Set the position and show the number line
            numberLineGenerator.transform.position = placementPosition;
            numberLineGenerator.ShowNumberLine();
            placedNumberLine = numberLineGenerator.gameObject;
            isPlaced = true;
            
            Debug.Log($"✅ Number line placed at {placementPosition}");
            
            // Hide plane visualization after successful placement
            HidePlaneVisualization();
        }
        else
        {
            Debug.LogError("Number line generator not found!");
        }
    }
    
    // Method to check for obstacles on a plane
    bool HasObstaclesOnPlane(ARPlane plane)
    {
        if (plane == null) return false;
        
        Debug.Log($"Checking for obstacles on plane {plane.trackableId}");
        
        // Simple obstacle detection - check if there are any colliders above the plane
        Vector3 planeCenter = plane.center;
        Vector3 checkPosition = planeCenter + Vector3.up * 0.1f; // Check slightly above plane
        
        // Raycast upward to detect obstacles
        Ray ray = new Ray(checkPosition, Vector3.up);
        if (Physics.Raycast(ray, 2f))
        {
            Debug.Log("Obstacle detected above plane");
            return true;
        }
        
        Debug.Log("No obstacles detected on plane");
        return false;
    }

    // Helper to find the next best plane (excluding the blocked one)
    ARPlane FindNextBestPlane(ARPlane blockedPlane)
    {
        if (planeManager == null) return null;
        ARPlane bestPlane = null;
        float bestScore = 0f;
        foreach (var plane in planeManager.trackables)
        {
            if (plane == blockedPlane) continue;
            float angle = Vector3.Angle(plane.normal, Vector3.up);
            if (angle > 10f) continue;
            float score = CalculatePlaneScore(plane);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlane = plane;
            }
        }
        return bestPlane;
    }

    void HidePlaneVisualization()
    {
        Debug.Log("Hiding plane visualization after placement");
        
        // Disable all ARPlane mesh renderers
        if (planeManager != null)
        {
            foreach (var plane in planeManager.trackables)
            {
                var meshRenderer = plane.GetComponent<MeshRenderer>();
                if (meshRenderer != null) 
                {
                    meshRenderer.enabled = false;
                    Debug.Log($"Hidden mesh renderer for plane {plane.trackableId}");
                }
                var lineRenderer = plane.GetComponent<LineRenderer>();
                if (lineRenderer != null) 
                {
                    lineRenderer.enabled = false;
                    Debug.Log($"Hidden line renderer for plane {plane.trackableId}");
                }
                plane.gameObject.SetActive(false);
                Debug.Log($"Deactivated plane GameObject {plane.trackableId}");
            }
            planeManager.requestedDetectionMode = PlaneDetectionMode.None;
            Debug.Log("✅ Plane detection visualization disabled");
        }
        
        // Also find and hide any plane visualization GameObjects by name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Plane") || obj.name.Contains("Visualization") || 
                obj.name.Contains("Mesh") || obj.name.Contains("Boundary") ||
                obj.name.Contains("ARPlane") || obj.name.Contains("Trackable"))
            {
                obj.SetActive(false);
                Debug.Log($"Hidden plane visualization: {obj.name}");
            }
        }
    }
    
    // Manual placement method (for tap-to-place if needed)
    public void PlaceNumberLineAtTouch(Vector2 touchPosition)
    {
        if (isPlaced) return;
        
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            
            Debug.Log($"Manual placement at touch position: {touchPosition}");
            
            // Get the plane from the hit
            ARPlane plane = planeManager.GetPlane(hit.trackableId);
            if (plane != null)
            {
                PlaceNumberLineOnPlane(plane);
            }
        }
    }
} 