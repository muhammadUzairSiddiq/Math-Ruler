using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Added for CanvasScaler and GraphicRaycaster

/// <summary>
/// AR Number Line Generator - Enhanced Version
/// 
/// Key Features:
/// 1. Horizontal layout (X-axis) for better visibility
/// 2. Touch-to-expand functionality - touch first/last cube to add more
/// 3. Dynamic cube visibility (8 cubes at a time)
/// 4. Improved cube detection and positioning
/// 5. Physical interaction for expanding number line
/// 
/// Touch-to-Expand:
/// - Touch the first visible cube (-4) to expand backwards (-5, -6, etc.)
/// - Touch the last visible cube (+3) to expand forwards (+4, +5, etc.)
/// - Automatic cube addition based on touch
/// - Visual feedback for touchable cubes
/// </summary>
public class ARNumberLineGenerator : MonoBehaviour
{
    [Header("Number Line Settings")]
    public GameObject cubePrefab;
    public int minNumber = -20;
    public int maxNumber = 20;
    public int initialVisibleRange = 8; // Show 8 cubes at a time
    public float cubeSpacing = 0f; // No spacing between cubes
    public float cubeScale = 0.3f; // Doubled the size
    
    // Public property to access cubeScale
    public float CubeScale => cubeScale;
    
    [Header("Layout Settings")]
    public bool useHorizontalLayout = true; // Changed to horizontal layout (X-axis)
    public float verticalSpacing = 0.4f;
    public float horizontalSpacing = 0.3f;
    
    [Header("Touch-to-Expand Settings")]
    public bool enableTouchToExpand = true;
    public int expandStep = 1; // Number of cubes to add per touch
    
    [Header("AR Integration")]
    public bool autoShowOnFloorDetection = true;
    public float placementHeight = 0.02f; // Increased height
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public int currentCenterNumber = 0;
    public int currentMinVisible = -10;
    public int currentMaxVisible = 10;
    
    // Private fields
    private Dictionary<int, GameObject> numberCubes = new Dictionary<int, GameObject>();
    private GameObject numberLineParent;
    private bool isInitialized = false;
    private bool isNumberLineVisible = false;
    private ARPlaneManager planeManager;
    private Camera arCamera;
    
    // Events
    public System.Action<int> OnPlayerEnteredCube;
    public System.Action OnPlayerExitedCube;
    public System.Action<int> OnNumberLineExpanded;
    public System.Action<Vector3> OnNumberLinePlaced;
    public System.Action<int> OnPlayerPositionChanged;
    
    void Start()
    {
        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("[Init] Created EventSystem at startup");
        }
        // OnScreenConsole removed - using CubePositionUI instead
        InitializeNumberLine();
    }
    
    void InitializeNumberLine()
    {
        Debug.Log("=== AR NUMBER LINE GENERATOR INITIALIZATION ===");
        
        // Find AR components
        planeManager = FindObjectOfType<ARPlaneManager>();
        arCamera = Camera.main;
        
        // Create number line parent
        CreateNumberLineParent();
        
        // Subscribe to plane detection events
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
        
        // Pre-generate all cubes (initially hidden)
        GenerateAllCubes();
        
        Debug.Log($"Number line generator initialized. Range: {minNumber} to {maxNumber}, Initial visible: {initialVisibleRange}");
    }
    
    void CreateNumberLineParent()
    {
        if (numberLineParent == null)
        {
            // Use this GameObject as the parent
            numberLineParent = gameObject;
            Debug.Log("Using this GameObject as Number Line Parent");
        }
    }
    
    [ContextMenu("Generate All Cubes")]
    public void GenerateAllCubes()
    {
        Debug.Log("=== GENERATING ALL CUBES ===");
        
        // Clear existing cubes
        ClearExistingCubes();
        
        // Create parent for all cubes
        if (numberLineParent == null)
        {
            numberLineParent = new GameObject("NumberLineCubes");
            numberLineParent.transform.SetParent(transform);
            numberLineParent.transform.localPosition = Vector3.zero;
        }
        
        // Generate all cubes from minNumber to maxNumber
        for (int i = minNumber; i <= maxNumber; i++)
        {
            GameObject cube = CreateNumberCube(i);
            if (cube != null)
            {
                // Store in dictionary
                numberCubes[i] = cube;
                
                // Initially hide the cube
                cube.SetActive(false);
                
                Debug.Log($"Generated cube {i}: {cube.name} at {cube.transform.localPosition}");
            }
        }
        
        Debug.Log($"✅ Generated {numberCubes.Count} cubes total");
        Debug.Log($"Cubes range: {minNumber} to {maxNumber}");
        
        // Set initial visible range
        currentMinVisible = -4;
        currentMaxVisible = 3;
        currentCenterNumber = 0;
        
        Debug.Log($"Initial visible range: {currentMinVisible} to {currentMaxVisible}");
        isInitialized = true;
    }
    
    GameObject CreateNumberCube(int number)
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube prefab is null! Please assign a prefab.");
            return null;
        }
        // Create cube
        GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity);
        cube.name = number.ToString();
        // Set parent
        if (numberLineParent != null)
        {
            cube.transform.SetParent(numberLineParent.transform);
        }
        else
        {
            cube.transform.SetParent(transform);
        }
        // Set scale
        cube.transform.localScale = Vector3.one * cubeScale;
        // Place cubes at Y=0 (flush with parent/plane)
        float xPosition = number * cubeScale;
        cube.transform.localPosition = new Vector3(xPosition, 0f, 0f);
        // Set color based on number
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (number == 0) renderer.material.color = Color.yellow;
            else if (number > 0) renderer.material.color = Color.green;
            else renderer.material.color = Color.red;
        }
        // Set up text display (always visible)
        SetupCubeText(cube, number);
        // Add triggers for detection and touch
        AddCubeTrigger(cube, number);
        // Log if this cube is touchable
        if (IsCubeTouchable(number))
        {
            Debug.Log($"[Create] Cube {number} is touchable (first/last visible)");
        }
        Debug.Log($"[Create] Created cube {number} at position {cube.transform.localPosition}");
        return cube;
    }

    void SetupCubeText(GameObject cube, int number)
    {
        // Find existing TextMeshProUGUI component
        TMPro.TextMeshProUGUI textComponent = cube.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent == null)
        {
            // Create a new text object as child
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(cube.transform);
            textObject.transform.localPosition = new Vector3(0, cubeScale * 0.6f, 0); // Slightly above the cube
            textObject.transform.localRotation = Quaternion.identity;
            // Add Canvas for world space UI
            Canvas canvas = textObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            // Add CanvasScaler
            CanvasScaler scaler = textObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            // Add GraphicRaycaster
            textObject.AddComponent<GraphicRaycaster>();
            
            // Create background circle for better text visibility
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(textObject.transform);
            backgroundObject.transform.localPosition = Vector3.zero;
            backgroundObject.transform.localScale = Vector3.one * 1.2f; // Slightly larger than text
            
            // Add Image component for background
            UnityEngine.UI.Image backgroundImage = backgroundObject.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = new Color(1f, 1f, 1f, 0.8f); // Semi-transparent white
            
            // Set background size
            RectTransform bgRectTransform = backgroundObject.GetComponent<RectTransform>();
            bgRectTransform.sizeDelta = new Vector2(1.2f, 1.2f);
            bgRectTransform.anchoredPosition = Vector2.zero;
            
            // Create text component
            textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
        }
        
        // Configure text component with enhanced styling
        textComponent.text = number.ToString();
        textComponent.color = Color.black;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        textComponent.fontSize = 0.25f; // Smaller font size for better readability
        
        // Add text outline for better visibility
        textComponent.outlineWidth = 0.1f;
        textComponent.outlineColor = Color.white;
        
        // Set text object size
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(1f, 1f);
        }
        
        Debug.Log($"Set up enhanced text with background for cube {number}: '{textComponent.text}'");
    }

    // Method to get the current scale for UI display
    public float GetCurrentScale()
    {
        return cubeScale;
    }
    
    // Touch-to-expand methods
    public void OnCubeTouched(GameObject cube)
    {
        if (!enableTouchToExpand) return;
        // Get the cube number from the cube name
        if (int.TryParse(cube.name, out int cubeNumber))
        {
            Debug.Log($"[TOUCH] Cube touched: {cube.name} (Number: {cubeNumber})");
            if (cubeNumber == currentMinVisible)
            {
                Debug.Log($"[TOUCH] Expanding backwards from cube {cubeNumber}");
                ExpandBackwards();
            }
            else if (cubeNumber == currentMaxVisible)
            {
                Debug.Log($"[TOUCH] Expanding forwards from cube {cubeNumber}");
                ExpandForwards();
            }
            else
            {
                Debug.Log($"[TOUCH] Cube {cubeNumber} touched but not expandable (not first/last visible cube)");
            }
        }
    }
    
    void ExpandForwards()
    {
        // Slide the visible window forward by expandStep
        int newCenter = currentCenterNumber + expandStep;
        // Clamp so we don't go past maxNumber
        int halfRange = initialVisibleRange / 2;
        int maxAllowedCenter = maxNumber - halfRange;
        newCenter = Mathf.Min(newCenter, maxAllowedCenter);
        if (newCenter != currentCenterNumber)
        {
            currentCenterNumber = newCenter;
            UpdateVisibleCubes(currentCenterNumber);
            Debug.Log($"✅ Expanded forwards: now showing cubes {currentMinVisible} to {currentMaxVisible}");
            OnNumberLineExpanded?.Invoke(currentMaxVisible);
        }
        else
        {
            Debug.Log("[Expand] Already at max range, cannot expand forwards further.");
        }
    }

    void ExpandBackwards()
    {
        // Slide the visible window backward by expandStep
        int newCenter = currentCenterNumber - expandStep;
        // Clamp so we don't go past minNumber
        int halfRange = initialVisibleRange / 2;
        int minAllowedCenter = minNumber + halfRange;
        newCenter = Mathf.Max(newCenter, minAllowedCenter);
        if (newCenter != currentCenterNumber)
        {
            currentCenterNumber = newCenter;
            UpdateVisibleCubes(currentCenterNumber);
            Debug.Log($"✅ Expanded backwards: now showing cubes {currentMinVisible} to {currentMaxVisible}");
            OnNumberLineExpanded?.Invoke(currentMinVisible);
        }
        else
        {
            Debug.Log("[Expand] Already at min range, cannot expand backwards further.");
        }
    }
    
    // Method to check if a cube is touchable (first or last visible cube)
    public bool IsCubeTouchable(int cubeNumber)
    {
        return enableTouchToExpand && (cubeNumber == currentMinVisible || cubeNumber == currentMaxVisible);
    }
    
    void SetCubeColor(GameObject cube, int number)
    {
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (number == 0)
            {
                renderer.material.color = Color.yellow; // Zero is special
            }
            else if (number > 0)
            {
                renderer.material.color = Color.green; // Positive numbers
            }
            else
            {
                renderer.material.color = Color.red; // Negative numbers
            }
        }
    }
    
    void UpdateCubeText(GameObject cube, int number)
    {
        // Find existing TextMeshProUGUI in the cube or its children
        TextMeshProUGUI textMesh = cube.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = number.ToString();
            Debug.Log($"Updated text for cube {number}: {textMesh.text}");
        }
        else
        {
            Debug.Log($"No TextMeshProUGUI found in cube {number}");
        }
        
        // Remove any TextMeshPro components that might be created at runtime
        TextMeshPro[] textMeshPros = cube.GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro tmp in textMeshPros)
        {
            // Remove ALL TextMeshPro components to prevent floating white text
            Debug.Log($"Removing TextMeshPro from cube {number}: {tmp.gameObject.name}");
            DestroyImmediate(tmp.gameObject);
        }
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (autoShowOnFloorDetection && !isNumberLineVisible)
        {
            Debug.Log($"Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
            
            // Find the best plane for placement
            ARPlane bestPlane = FindBestPlane();
            if (bestPlane != null)
            {
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
            float score = CalculatePlaneScore(plane);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlane = plane;
            }
        }
        
        return bestPlane;
    }
    
    float CalculatePlaneScore(ARPlane plane)
    {
        if (plane == null) return 0f;
        
        // Check if plane is too small
        if (plane.size.x < 1.0f || plane.size.y < 1.0f) return 0f;
        
        // Calculate score based on size (prefer larger planes)
        float sizeScore = Mathf.Min(plane.size.x, plane.size.y) / 1.0f;
        
        return sizeScore;
    }
    
    void PlaceNumberLineOnPlane(ARPlane plane)
    {
        if (plane == null || isNumberLineVisible) return;
        
        Debug.Log($"=== PLACING NUMBER LINE ON PLANE ===");
        Debug.Log($"Plane ID: {plane.trackableId}");
        Debug.Log($"Plane Center: {plane.center}");
        Debug.Log($"Plane Size: {plane.size}");
        
        // Calculate placement position
        Vector3 placementPosition = plane.center + (plane.normal * placementHeight);
        
        // Position the number line generator
        transform.position = placementPosition;
        
        // Set rotation to align with the plane normal and forward direction
        Vector3 forwardDirection = Vector3.right; // Use X-axis for horizontal layout
        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, plane.normal);
        transform.rotation = targetRotation;
        
        // Show the number line
        ShowNumberLine();
        
        // Notify placement
        OnNumberLinePlaced?.Invoke(placementPosition);
        
        Debug.Log($"✅ Number line successfully placed at: {placementPosition}");
        isNumberLineVisible = true;
    }
    
    public void ShowNumberLine()
    {
        if (!isInitialized)
        {
            Debug.Log("Number line not initialized. Generating cubes first...");
            GenerateAllCubes();
        }
        
        Debug.Log("=== SHOWING NUMBER LINE ===");
        
        // Show initial 8 cubes around center (0)
        UpdateVisibleCubes(0);
        
        Debug.Log($"✅ Number line shown with {currentMaxVisible - currentMinVisible + 1} cubes visible");
        Debug.Log($"Visible range: {currentMinVisible} to {currentMaxVisible}");
        isNumberLineVisible = true;
    }
    
    void SetAllCubesVisibility(bool visible)
    {
        foreach (var kvp in numberCubes)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(visible);
            }
        }
    }
    
    public void UpdatePlayerPosition(int playerNumber)
    {
        Debug.Log($"Player position updated to: {playerNumber}");
        // Only update visible cubes and state, do NOT move the number line transform
        UpdateVisibleCubes(playerNumber);
        OnPlayerPositionChanged?.Invoke(playerNumber);
    }

    public void UpdateVisibleCubes(int centerNumber)
    {
        currentCenterNumber = centerNumber;
        int halfRange = initialVisibleRange / 2;
        int newMinVisible = centerNumber - halfRange;
        int newMaxVisible = centerNumber + halfRange;
        newMinVisible = Mathf.Max(newMinVisible, minNumber);
        newMaxVisible = Mathf.Min(newMaxVisible, maxNumber);
        currentMinVisible = newMinVisible;
        currentMaxVisible = newMaxVisible;
        Debug.Log($"Updating visible cubes: center={centerNumber}, range={currentMinVisible} to {currentMaxVisible}");
        foreach (var kvp in numberCubes)
        {
            int cubeNumber = kvp.Key;
            GameObject cube = kvp.Value;
            bool shouldBeVisible = (cubeNumber >= currentMinVisible && cubeNumber <= currentMaxVisible);
            if (cube != null)
            {
                cube.SetActive(shouldBeVisible);
                if (shouldBeVisible)
                {
                    Debug.Log($"Showing cube {cubeNumber}");
                }
            }
        }
        Debug.Log($"✅ Updated visibility: {currentMaxVisible - currentMinVisible + 1} cubes visible ({currentMinVisible} to {currentMaxVisible})");
    }
    
    public bool IsNumberLineVisible()
    {
        return isNumberLineVisible;
    }
    
    public bool IsNumberLinePlaced()
    {
        return isInitialized && numberCubes.Count > 0;
    }
    
    public int GetPlayerCurrentNumber()
    {
        return currentCenterNumber;
    }
    
    [ContextMenu("Test Dynamic Cube Visibility")]
    public void TestDynamicCubeVisibility()
    {
        Debug.Log("=== TESTING DYNAMIC CUBE VISIBILITY ===");
        
        if (numberCubes.Count == 0)
        {
            Debug.LogError("❌ No cubes generated! Run 'Generate All Cubes' first.");
            return;
        }
        
        Debug.Log($"Total cubes available: {numberCubes.Count} ({minNumber} to {maxNumber})");
        Debug.Log($"Initial visible range: {currentMinVisible} to {currentMaxVisible}");
        Debug.Log($"Initial center: {currentCenterNumber}");
        
        // Test moving player to different positions
        int[] testPositions = { -15, -10, -5, 0, 5, 10, 15 };
        
        foreach (int position in testPositions)
        {
            Debug.Log($"\n--- Moving player to position {position} ---");
            UpdatePlayerPosition(position);
            
            // Count visible cubes
            int visibleCount = 0;
            foreach (var kvp in numberCubes)
            {
                if (kvp.Value.activeInHierarchy)
                {
                    visibleCount++;
                }
            }
            
            Debug.Log($"✅ Position {position}: {visibleCount} cubes visible ({currentMinVisible} to {currentMaxVisible})");
            
            // List which cubes are visible
            string visibleCubes = "";
            for (int i = currentMinVisible; i <= currentMaxVisible; i++)
            {
                if (numberCubes.ContainsKey(i) && numberCubes[i].activeInHierarchy)
                {
                    visibleCubes += i + " ";
                }
            }
            Debug.Log($"Visible cubes: {visibleCubes}");
        }
        
        Debug.Log("\n=== DYNAMIC VISIBILITY TEST COMPLETE ===");
    }

    [ContextMenu("Test Touch-to-Expand")]
    public void TestTouchToExpand()
    {
        Debug.Log("=== TESTING TOUCH-TO-EXPAND ===");
        Debug.Log($"Current visible range: {currentMinVisible} to {currentMaxVisible}");
        Debug.Log($"Touchable cubes: {currentMinVisible} and {currentMaxVisible}");
        
        // Test expanding forwards
        Debug.Log("Testing expand forwards...");
        ExpandForwards();
        
        // Test expanding backwards
        Debug.Log("Testing expand backwards...");
        ExpandBackwards();
        
        Debug.Log($"New visible range: {currentMinVisible} to {currentMaxVisible}");
    }
    
    [ContextMenu("Test Expand Forwards")]
    public void TestExpandForwards()
    {
        Debug.Log("=== TESTING EXPAND FORWARDS ===");
        Debug.Log($"Before: {currentMinVisible} to {currentMaxVisible}");
        ExpandForwards();
        Debug.Log($"After: {currentMinVisible} to {currentMaxVisible}");
    }
    
    [ContextMenu("Test Expand Backwards")]
    public void TestExpandBackwards()
    {
        Debug.Log("=== TESTING EXPAND BACKWARDS ===");
        Debug.Log($"Before: {currentMinVisible} to {currentMaxVisible}");
        ExpandBackwards();
        Debug.Log($"After: {currentMinVisible} to {currentMaxVisible}");
    }
    
    [ContextMenu("Generate Number Line")]
    public void GenerateNumberLine()
    {
        Debug.Log("=== GENERATING NUMBER LINE FROM CONTEXT MENU ===");
        GenerateAllCubes();
        Debug.Log("✅ Number line generated successfully");
    }
    
    [ContextMenu("Show Number Line")]
    public void ShowNumberLineFromMenu()
    {
        Debug.Log("=== SHOWING NUMBER LINE FROM CONTEXT MENU ===");
        ShowNumberLine();
    }
    
    [ContextMenu("Hide Number Line")]
    public void HideNumberLineFromMenu()
    {
        Debug.Log("=== HIDING NUMBER LINE FROM CONTEXT MENU ===");
        SetAllCubesVisibility(false);
        isNumberLineVisible = false;
        Debug.Log("✅ Number line hidden");
    }
    
    [ContextMenu("Hide All Cubes")]
    public void HideAllCubes()
    {
        Debug.Log("=== HIDING ALL CUBES ===");
        SetAllCubesVisibility(false);
        Debug.Log("✅ All cubes hidden");
    }
    
    [ContextMenu("Delete All Cubes")]
    public void DeleteAllCubes()
    {
        Debug.Log("=== DELETING ALL CUBES ===");
        ClearExistingCubes();
        Debug.Log("✅ All cubes deleted");
    }
    
    public void ClearExistingCubes()
    {
        Debug.Log("Clearing existing cubes...");
        
        // Destroy all cube GameObjects
        foreach (var kvp in numberCubes)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        
        // Clear the dictionary
        numberCubes.Clear();
        
        Debug.Log($"Cleared {numberCubes.Count} cubes");
    }
    
    // Cube Detection System
    void AddCubeTrigger(GameObject cube, int number)
    {
        // Add BoxCollider as trigger
        BoxCollider collider = cube.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = cube.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = Vector3.one * 0.8f; // Slightly smaller than cube for better detection
        
        // Add CubeDetector for player detection
        CubeDetector detector = cube.GetComponent<CubeDetector>();
        if (detector == null)
        {
            detector = cube.AddComponent<CubeDetector>();
        }
        detector.Initialize(number, this);
        
        // Add CubeTouchDetector for touch-to-expand functionality
        CubeTouchDetector touchDetector = cube.GetComponent<CubeTouchDetector>();
        if (touchDetector == null)
        {
            touchDetector = cube.AddComponent<CubeTouchDetector>();
        }
        
        // Ensure EventSystem exists for UI interactions
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for touch detection");
        }
        
        Debug.Log($"Added triggers to cube {number}: Collider, CubeDetector, CubeTouchDetector");
    }
    
    // Called by CubeDetector when player enters a cube
    public void HandlePlayerEnteredCube(int cubeNumber)
    {
        Debug.Log($"Player entered cube: {cubeNumber}");
        OnPlayerEnteredCube?.Invoke(cubeNumber);
    }
    
    // Called by CubeDetector when player exits a cube
    public void HandlePlayerExitedCube()
    {
        Debug.Log("Player exited cube");
        OnPlayerExitedCube?.Invoke();
    }
}

// Billboard component to make text always face camera
public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
    }
} 