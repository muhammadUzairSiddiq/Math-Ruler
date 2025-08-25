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
    
    [Header("Improved Placement")]
    public float maxDistanceFromUser = 3.0f; // Maximum distance from user for placement
    public float preferredDistanceFromUser = 1.5f; // Preferred distance from user
    public float directionConsistencyWeight = 0.3f; // Weight for direction consistency
    public float proximityWeight = 0.4f; // Weight for proximity to user
    public float sizeWeight = 0.3f; // Weight for plane size
    public Vector3 preferredDirection = Vector3.forward; // Preferred forward direction for consistency
    
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
        
        // Update preferred direction based on user's initial orientation
        UpdatePreferredDirection();
    }
    
    void UpdatePreferredDirection()
    {
        if (Camera.main != null)
        {
            // Use camera's forward direction as preferred direction
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // Keep it horizontal
            cameraForward.Normalize();
            
            preferredDirection = cameraForward;
            Debug.Log($"Updated preferred direction to: {preferredDirection}");
        }
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
        if (hidePlacementUI && placementUI != null)
        {
            placementUI.SetActive(false);
            Debug.Log("AR Placement UI hidden");
        }
    }

    [ContextMenu("Test Improved Placement")]
    public void TestImprovedPlacement()
    {
        Debug.Log("=== TESTING IMPROVED PLACEMENT SYSTEM ===");
        
        if (isPlaced)
        {
            Debug.Log("Number line already placed. Reset first to test again.");
            return;
        }
        
        // Update preferred direction
        UpdatePreferredDirection();
        
        // Find and evaluate planes
        ARPlane bestPlane = FindBestPlane();
        if (bestPlane != null)
        {
            Debug.Log($"Testing placement on plane: {bestPlane.trackableId}");
            PlaceNumberLineOnPlane(bestPlane);
        }
        else
        {
            Debug.LogWarning("No suitable plane found for testing");
        }
    }

    [ContextMenu("Reset Placement")]
    public void ResetPlacement()
    {
        Debug.Log("=== RESETTING PLACEMENT ===");
        
        isPlaced = false;
        if (placedNumberLine != null)
        {
            DestroyImmediate(placedNumberLine);
            placedNumberLine = null;
        }
        
        if (numberLineGenerator != null)
        {
            numberLineGenerator.transform.position = Vector3.zero;
        }
        
        Debug.Log("Placement reset complete");
    }

    [ContextMenu("Show Placement Settings")]
    public void ShowPlacementSettings()
    {
        Debug.Log("=== CURRENT PLACEMENT SETTINGS ===");
        Debug.Log($"Max Distance from User: {maxDistanceFromUser}");
        Debug.Log($"Preferred Distance from User: {preferredDistanceFromUser}");
        Debug.Log($"Direction Consistency Weight: {directionConsistencyWeight}");
        Debug.Log($"Proximity Weight: {proximityWeight}");
        Debug.Log($"Size Weight: {sizeWeight}");
        Debug.Log($"Preferred Direction: {preferredDirection}");
        Debug.Log($"Min Plane Size: {minPlaneSize}");
        Debug.Log($"Placement Height: {placementHeight}");
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!autoPlaceOnDetection || isPlaced) return;
        
        Debug.Log($"Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
        
        // Find the best plane for placement
        ARPlane bestPlane = FindBestPlane();
        if (bestPlane != null)
        {
            Debug.Log($"Found best plane: {bestPlane.trackableId} at {bestPlane.center}");
            PlaceNumberLineOnPlane(bestPlane);
        }
        else
        {
            Debug.Log("No suitable plane found for placement");
        }
    }
    
    ARPlane FindBestPlane()
    {
        if (planeManager == null) return null;
        
        ARPlane bestPlane = null;
        float bestScore = 0f;
        int totalPlanes = 0;
        int rejectedPlanes = 0;
        
        Debug.Log("=== EVALUATING PLANES FOR PLACEMENT ===");
        
        foreach (var plane in planeManager.trackables)
        {
            totalPlanes++;
            float score = CalculatePlaneScore(plane);
            
            if (score > 0)
            {
                Debug.Log($"Plane {plane.trackableId}: Score = {score:F3}");
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlane = plane;
                }
            }
            else
            {
                rejectedPlanes++;
                Debug.Log($"Plane {plane.trackableId}: Rejected (Score = 0)");
            }
        }
        
        Debug.Log($"Plane evaluation complete: {totalPlanes} total, {rejectedPlanes} rejected, {totalPlanes - rejectedPlanes} considered");
        
        if (bestPlane != null)
        {
            Debug.Log($"✅ Best plane selected: {bestPlane.trackableId} with score {bestScore:F3}");
        }
        else
        {
            Debug.LogWarning("❌ No suitable plane found for placement");
        }
        
        return bestPlane;
    }
    // TODO: Before placing the number line, raycast upwards from each intended cube position to check for obstacles (shelves, walls).
    
    float CalculatePlaneScore(ARPlane plane)
    {
        if (plane == null) return 0f;
        
        // Check if plane is too small
        if (plane.size.x < minPlaneSize || plane.size.y < minPlaneSize) return 0f;
        
        // Get user position (camera position in AR)
        Vector3 userPosition = Camera.main.transform.position;
        Vector3 planeCenter = plane.center;
        
        // Calculate distance from user
        float distanceFromUser = Vector3.Distance(userPosition, planeCenter);
        
        // Reject planes that are too far from user
        if (distanceFromUser > maxDistanceFromUser) return 0f;
        
        // Calculate proximity score (closer is better)
        float proximityScore = 1f - Mathf.Clamp01(distanceFromUser / maxDistanceFromUser);
        
        // Calculate direction consistency score
        float directionScore = 0f;
        if (plane.normal.y > 0.8f) // Only consider relatively flat planes
        {
            // Calculate how well the plane aligns with preferred direction
            Vector3 planeForward = Vector3.Cross(plane.normal, Vector3.up);
            float alignment = Vector3.Dot(planeForward, preferredDirection);
            directionScore = (alignment + 1f) * 0.5f; // Convert from [-1,1] to [0,1]
        }
        
        // Calculate size score (larger is better)
        float sizeScore = Mathf.Min(plane.size.x, plane.size.y) / minPlaneSize;
        sizeScore = Mathf.Clamp01(sizeScore / 5f); // Normalize to reasonable range
        
        // Calculate weighted final score
        float finalScore = (proximityScore * proximityWeight) + 
                          (directionScore * directionConsistencyWeight) + 
                          (sizeScore * sizeWeight);
        
        Debug.Log($"Plane {plane.trackableId} scoring - Distance: {distanceFromUser:F2}, Proximity: {proximityScore:F2}, Direction: {directionScore:F2}, Size: {sizeScore:F2}, Final: {finalScore:F2}");
        
        return finalScore;
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
        
        // Adjust position to be closer to user while staying on the plane
        Vector3 userPosition = Camera.main.transform.position;
        Vector3 userToPlane = plane.center - userPosition;
        userToPlane.y = 0; // Keep on same Y level
        
        // If plane is too far, move it closer to user (but still on the plane)
        float currentDistance = Vector3.Distance(userPosition, plane.center);
        if (currentDistance > preferredDistanceFromUser)
        {
            Vector3 directionToUser = userToPlane.normalized;
            float moveDistance = currentDistance - preferredDistanceFromUser;
            Vector3 adjustedPosition = plane.center - (directionToUser * moveDistance);
            
            // Ensure the adjusted position is still on the plane
            adjustedPosition.y = plane.center.y + placementHeight;
            placementPosition = adjustedPosition;
            
            Debug.Log($"Adjusted placement from {plane.center} to {placementPosition} to be closer to user");
        }
        
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