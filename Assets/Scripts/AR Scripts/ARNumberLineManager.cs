using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using TMPro;

public class ARNumberLineManager : MonoBehaviour
{
    [Header("Number Line Settings")]
    public GameObject numberCubePrefab;
    public int minNumber = -20;
    public int maxNumber = 20;
    public int visibleRange = 15; // Show 15 cubes at once
    public float cubeSpacing = 0.3f; // Distance between cubes
    public float cubeScale = 0.15f; // Size of each cube
    
    [Header("Auto Placement")]
    public bool autoPlaceOnFloorDetection = true;
    public float placementHeight = 0.01f; // Slightly above floor
    public Vector3 placementOffset = Vector3.zero;
    
    [Header("Mobile Optimization")]
    public float mobileScreenScale = 0.8f; // Scale for mobile screens
    public bool adaptToScreenSize = true;
    public float minSpacing = 0.2f;
    public float maxSpacing = 0.5f;
    
    [Header("AR Components")]
    public ARPlaneManager planeManager;
    public ARSessionOrigin sessionOrigin;
    public ARPlayerController arPlayerController;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public int currentMinVisible = -7;
    public int currentMaxVisible = 7;
    public int totalCubes = 0;
    public bool isNumberLinePlaced = false;
    
    // Private variables
    private GameObject numberLineParent;
    private Dictionary<int, GameObject> numberCubes = new Dictionary<int, GameObject>();
    private int playerCurrentNumber = 0;
    private ARPlane detectedPlane;
    private Camera arCamera;
    
    // Events
    public System.Action<Vector3> OnNumberLinePlaced;
    public System.Action<int> OnPlayerPositionChanged;
    
    void Start()
    {
        InitializeNumberLine();
    }
    
    void InitializeNumberLine()
    {
        Debug.Log("=== AR NUMBER LINE INITIALIZATION ===");
        
        // Find AR components
        if (planeManager == null) planeManager = FindObjectOfType<ARPlaneManager>();
        if (sessionOrigin == null) sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (arPlayerController == null) arPlayerController = FindObjectOfType<ARPlayerController>();
        arCamera = Camera.main;
        
        // Create number line parent
        CreateNumberLineParent();
        
        // Subscribe to plane detection events
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
        
        // Calculate optimal spacing for mobile
        CalculateMobileSpacing();
        
        Debug.Log($"Number line initialized. Range: {minNumber} to {maxNumber}, Visible: {visibleRange}");
    }
    
    void CalculateMobileSpacing()
    {
        if (adaptToScreenSize && arCamera != null)
        {
            // Calculate spacing based on screen size and camera distance
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float aspectRatio = screenWidth / screenHeight;
            
            // Adjust spacing based on screen size
            if (aspectRatio < 1.5f) // Portrait mode
            {
                cubeSpacing = Mathf.Lerp(minSpacing, maxSpacing, 0.3f);
                cubeScale = 0.12f;
            }
            else // Landscape mode
            {
                cubeSpacing = Mathf.Lerp(minSpacing, maxSpacing, 0.7f);
                cubeScale = 0.15f;
            }
            
            Debug.Log($"Mobile spacing calculated: {cubeSpacing}, Scale: {cubeScale}");
        }
    }
    
    void CreateNumberLineParent()
    {
        if (numberLineParent == null)
        {
            numberLineParent = new GameObject("Number Line Parent");
            Debug.Log("Created Number Line Parent");
        }
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            if (autoPlaceOnFloorDetection && !isNumberLinePlaced)
            {
                detectedPlane = plane;
                PlaceNumberLineOnPlane(plane);
                break;
            }
        }
    }
    
    void PlaceNumberLineOnPlane(ARPlane plane)
    {
        Debug.Log($"Placing number line on detected plane: {plane.trackableId}");
        
        // Calculate placement position
        Vector3 planeCenter = plane.center;
        Vector3 placementPosition = planeCenter + Vector3.up * placementHeight + placementOffset;
        
        // Position number line parent
        numberLineParent.transform.position = placementPosition;
        numberLineParent.transform.rotation = Quaternion.LookRotation(Vector3.forward, plane.normal);
        
        // Create all number cubes (initially hidden)
        CreateAllNumberCubes();
        
        // Show initial visible range
        UpdateVisibleCubes(0);
        
        isNumberLinePlaced = true;
        OnNumberLinePlaced?.Invoke(placementPosition);
        
        Debug.Log($"Number line placed at position: {placementPosition}");
    }
    
    void CreateAllNumberCubes()
    {
        Debug.Log($"Creating {maxNumber - minNumber + 1} number cubes...");
        
        for (int i = minNumber; i <= maxNumber; i++)
        {
            CreateNumberCube(i);
        }
        
        totalCubes = numberCubes.Count;
        Debug.Log($"Created {totalCubes} number cubes");
    }
    
    void CreateNumberCube(int number)
    {
        // Calculate position
        float xPosition = number * cubeSpacing;
        Vector3 position = new Vector3(xPosition, 0, 0);
        
        // Create cube
        GameObject cube = Instantiate(numberCubePrefab, position, Quaternion.identity);
        cube.name = number.ToString();
        cube.transform.SetParent(numberLineParent.transform);
        cube.transform.localPosition = position;
        cube.transform.localScale = Vector3.one * cubeScale;
        
        // Set cube color based on number
        SetCubeColor(cube, number);
        
        // Add number text
        AddNumberText(cube, number);
        
        // Initially hide the cube
        cube.SetActive(false);
        
        // Store in dictionary
        numberCubes[number] = cube;
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
    
    void AddNumberText(GameObject cube, int number)
    {
        // Create text object
        GameObject textGO = new GameObject($"Text_{number}");
        textGO.transform.SetParent(cube.transform);
        textGO.transform.localPosition = Vector3.up * (cubeScale * 0.6f);
        textGO.transform.localRotation = Quaternion.identity;
        
        // Add TextMeshPro component
        TextMeshPro textMesh = textGO.AddComponent<TextMeshPro>();
        textMesh.text = number.ToString();
        textMesh.fontSize = 0.5f;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Make text always face camera
        textGO.AddComponent<Billboard>();
    }
    
    public void UpdatePlayerPosition(int playerNumber)
    {
        if (playerNumber != playerCurrentNumber)
        {
            playerCurrentNumber = playerNumber;
            UpdateVisibleCubes(playerNumber);
            OnPlayerPositionChanged?.Invoke(playerNumber);
            
            if (showDebugInfo)
            {
                Debug.Log($"Player moved to number: {playerNumber}");
            }
        }
    }
    
    void UpdateVisibleCubes(int centerNumber)
    {
        // Calculate visible range
        int halfRange = visibleRange / 2;
        int minVisible = centerNumber - halfRange;
        int maxVisible = centerNumber + halfRange;
        
        // Clamp to available range
        minVisible = Mathf.Max(minVisible, minNumber);
        maxVisible = Mathf.Min(maxVisible, maxNumber);
        
        currentMinVisible = minVisible;
        currentMaxVisible = maxVisible;
        
        // Show/hide cubes
        foreach (var kvp in numberCubes)
        {
            int cubeNumber = kvp.Key;
            GameObject cube = kvp.Value;
            
            bool shouldBeVisible = cubeNumber >= minVisible && cubeNumber <= maxVisible;
            cube.SetActive(shouldBeVisible);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Visible cubes: {minVisible} to {maxVisible} (Center: {centerNumber})");
        }
    }
    
    public Vector3 GetPositionForNumber(int number)
    {
        if (numberCubes.ContainsKey(number))
        {
            return numberCubes[number].transform.position;
        }
        return Vector3.zero;
    }
    
    public int GetNumberAtPosition(Vector3 worldPosition)
    {
        // Convert world position to local position on number line
        Vector3 localPosition = numberLineParent.transform.InverseTransformPoint(worldPosition);
        int number = Mathf.RoundToInt(localPosition.x / cubeSpacing);
        
        // Clamp to valid range
        number = Mathf.Clamp(number, minNumber, maxNumber);
        
        return number;
    }
    
    // PlaceRewardAtNumber method removed - rewards are now activated as children of cube prefabs
    
    public bool IsNumberLinePlaced()
    {
        return isNumberLinePlaced;
    }
    
    public int GetPlayerCurrentNumber()
    {
        return playerCurrentNumber;
    }
    
    public int GetVisibleRange()
    {
        return visibleRange;
    }
    
    // Context menu for testing
    [ContextMenu("Test Number Line")]
    public void TestNumberLine()
    {
        if (!isNumberLinePlaced)
        {
            Debug.Log("Number line not placed yet. Waiting for floor detection...");
            return;
        }
        
        UpdatePlayerPosition(0);
        Debug.Log("Tested number line - showing cubes around 0");
    }
    
    [ContextMenu("Move Player to 10")]
    public void TestMoveTo10()
    {
        UpdatePlayerPosition(10);
    }
    
    [ContextMenu("Move Player to -5")]
    public void TestMoveToNegative5()
    {
        UpdatePlayerPosition(-5);
    }
    
    [ContextMenu("Test Reward Positioning System")]
    public void TestRewardPositioningSystem()
    {
        Debug.Log("=== TESTING REWARD POSITIONING SYSTEM ===");
        
        if (!isNumberLinePlaced)
        {
            Debug.LogError("❌ Number line not placed yet! Wait for floor detection.");
            return;
        }
        
        if (arPlayerController == null)
        {
            Debug.LogError("❌ AR Player Controller not found!");
            return;
        }
        
        // Test reward positioning for different scenarios
        Debug.Log("Testing reward positioning scenarios:");
        
        // Scenario 1: Player on cube 0
        Debug.Log($"\n--- Scenario 1: Player on cube 0 ---");
        TestRewardPositioning(0);
        
        // Scenario 2: Player on cube 5
        Debug.Log($"\n--- Scenario 2: Player on cube 5 ---");
        TestRewardPositioning(5);
        
        // Scenario 3: Player not on any cube
        Debug.Log($"\n--- Scenario 3: Player not on any cube ---");
        TestRewardPositioning(-999);
        
        Debug.Log("=== REWARD POSITIONING SYSTEM TEST COMPLETE ===");
    }
    
    void TestRewardPositioning(int playerCubeNumber)
    {
        // Simulate player being on this cube
        if (playerCubeNumber != -999)
        {
            arPlayerController.OnPlayerEnteredCube(playerCubeNumber);
        }
        else
        {
            arPlayerController.OnPlayerExitedCube();
        }
        
        // Get current state
        int currentCube = arPlayerController.GetCurrentCubeNumber();
        Vector3 playerPosition = Camera.main.transform.position;
        
        Debug.Log($"   Player Cube Number: {currentCube}");
        Debug.Log($"   Player Position: {playerPosition}");
        
        // Test reward positioning logic
        if (numberCubes.ContainsKey(5)) // Use cube 5 as test target
        {
            GameObject testCube = numberCubes[5];
            Vector3 cubePosition = testCube.transform.position;
            
            // Calculate expected spawn position
            Vector3 expectedPosition;
            if (currentCube == 5)
            {
                expectedPosition = cubePosition + Vector3.up * (cubeScale * 0.8f);
                Debug.Log($"   ✅ Player on target cube - Reward will spawn ON TOP at: {expectedPosition}");
            }
            else
            {
                expectedPosition = playerPosition + Vector3.forward * 2f;
                Debug.Log($"   ⚠️ Player not on target cube - Reward will spawn at PLAYER POSITION: {expectedPosition}");
            }
            
            Debug.Log($"   Target Cube Position: {cubePosition}");
            Debug.Log($"   Expected Reward Position: {expectedPosition}");
        }
                 else
         {
             Debug.LogWarning("   Test cube 5 not found!");
         }
     }
 }
 
 // Billboard component moved to ARNumberLineGenerator.cs to avoid duplication