using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlayerController : MonoBehaviour
{
    [Header("AR Components")]
    public ARCameraManager cameraManager;
    public ARNumberLineGenerator numberLineGenerator;
    public Transform playerTransform;
    
    [Header("Player Settings")]
    public float movementThreshold = 0.1f; // Minimum distance to trigger movement
    public float positionUpdateRate = 0.1f; // How often to update position
    public bool enableHapticFeedback = true;
    public float stepDistance = 0.3f; // Distance for one step on number line
    
    [Header("Movement Tracking")]
    public bool enableCubeDetection = true;
    public Vector3 lastPosition;
    public int currentNumber = 0;
    public bool isMoving = false;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showMovementTrail = false;
    
    [Header("Cube Detection")]
    public bool enableCubeDetectionUI = true;
    public int currentCubeNumber = -999; // -999 means not on any cube
    
    // Events
    public System.Action<int> OnNumberChanged;
    public System.Action<Vector3> OnPlayerMoved;
    
    private float lastUpdateTime;
    private Vector3 initialPosition;
    private bool initialized = false;
    private Vector3 lastValidPosition;
    private int lastValidNumber = 0;
    private Camera arCamera;
    
    void Start()
    {
        InitializePlayerController();
        
        // Subscribe to cube detection events
        if (numberLineGenerator != null)
        {
            numberLineGenerator.OnPlayerEnteredCube += OnPlayerEnteredCube;
            numberLineGenerator.OnPlayerExitedCube += OnPlayerExitedCube;
        }
    }
    
    void InitializePlayerController()
    {
        Debug.Log("=== AR PLAYER CONTROLLER INITIALIZATION ===");
        
        // Find number line generator
        if (numberLineGenerator == null)
        {
            numberLineGenerator = FindObjectOfType<ARNumberLineGenerator>();
        }
        
        if (numberLineGenerator != null)
        {
            Debug.Log($"✅ Found AR Number Line Generator: {numberLineGenerator.name}");
        }
        else
        {
            Debug.LogError("❌ AR Number Line Generator not found!");
        }
        
        // Find AR camera
        arCamera = Camera.main;
        if (arCamera != null)
        {
            Debug.Log($"✅ Found AR Camera: {arCamera.name}");
        }
        else
        {
            Debug.LogError("❌ AR Camera not found!");
        }
        
        // Set up player transform
        if (playerTransform == null)
        {
            // Create a virtual player at camera position
            GameObject playerGO = new GameObject("ARPlayer");
            playerTransform = playerGO.transform;
            playerGO.transform.position = Camera.main.transform.position;
        }
        
        initialPosition = playerTransform.position;
        lastPosition = initialPosition;
        lastValidPosition = initialPosition;
        
        initialized = true;
        Debug.Log("AR Player Controller initialized");
    }
    
    void Update()
    {
        if (!initialized) return;
        
        // Wait for number line to be placed
        if (numberLineGenerator != null && !numberLineGenerator.IsNumberLineVisible())
        {
            if (showDebugInfo)
            {
                Debug.Log("Waiting for number line to be placed...");
            }
            return;
        }
        
        // Update player position based on camera movement
        UpdatePlayerPosition();
        
        // Check for movement
        CheckForMovement();
    }
    
    void UpdatePlayerPosition()
    {
        if (Time.time - lastUpdateTime < positionUpdateRate) return;
        
        // Use camera position as player position in AR
        Vector3 cameraPosition = Camera.main.transform.position;
        playerTransform.position = cameraPosition;
        
        lastUpdateTime = Time.time;
    }
    
    void CheckForMovement()
    {
        Vector3 currentPosition = playerTransform.position;
        float distanceMoved = Vector3.Distance(currentPosition, lastPosition);
        
        if (distanceMoved > movementThreshold)
        {
            // Calculate which cube the player is standing on based on position
            if (numberLineGenerator != null && numberLineGenerator.IsNumberLineVisible())
            {
                // Get the number line's position and direction
                Vector3 numberLinePosition = numberLineGenerator.transform.position;
                Vector3 numberLineDirection = numberLineGenerator.transform.right; // X-axis direction
                
                // Calculate player's position relative to the number line
                Vector3 relativePosition = currentPosition - numberLinePosition;
                float distanceAlongLine = Vector3.Dot(relativePosition, numberLineDirection);
                
                // Convert to cube number (each cube is cubeScale units apart)
                int newCubeNumber = Mathf.RoundToInt(distanceAlongLine / numberLineGenerator.CubeScale);
                
                // Clamp to valid range
                newCubeNumber = Mathf.Clamp(newCubeNumber, -20, 20);
                
                if (newCubeNumber != currentNumber)
                {
                    Debug.Log($"[MOVEMENT] Player moved from cube {currentNumber} to cube {newCubeNumber}");
                    MoveToNumber(newCubeNumber);
                }
            }
            
            lastPosition = currentPosition;
            OnPlayerMoved?.Invoke(currentPosition - lastPosition);
        }
    }
    
    Vector3 GetNumberLineDirection()
    {
        if (numberLineGenerator != null && numberLineGenerator.IsNumberLineVisible())
        {
            // Get the forward direction of the number line
            return numberLineGenerator.transform.forward;
        }
        return Vector3.forward; // Default direction
    }
    
    void MoveToNumber(int targetNumber)
    {
        if (targetNumber == currentNumber) return;
        
        currentNumber = targetNumber;
        lastValidNumber = targetNumber;
        lastValidPosition = playerTransform.position;
        
        // Update number line generator
        if (numberLineGenerator != null)
        {
            numberLineGenerator.UpdatePlayerPosition(targetNumber);
            numberLineGenerator.UpdateVisibleCubes(targetNumber); // Ensure cubes update as you walk
        }
        
        // Trigger events
        OnNumberChanged?.Invoke(targetNumber);
        
        if (showDebugInfo)
        {
            Debug.Log($"[POSITION] Player is now at cube number: {targetNumber}");
        }
        
        // Haptic feedback
        if (enableHapticFeedback)
        {
            // Add haptic feedback here if needed
        }
    }
    
    void OnNumberLinePlaced(Vector3 position)
    {
        Debug.Log($"Number line placed at {position}. Player can now move.");
        
        // Initialize player at position 0
        currentNumber = 0;
        lastValidNumber = 0;
        
        if (numberLineGenerator != null)
        {
            numberLineGenerator.UpdatePlayerPosition(0);
        }
    }
    
    void OnPlayerPositionChanged(int number)
    {
        // This is called when the number line manager updates the visible cubes
        if (showDebugInfo)
        {
            Debug.Log($"Number line updated to show cubes around: {number}");
        }
    }
    
    void OnPlayerEnteredCube(int cubeNumber)
    {
        currentCubeNumber = cubeNumber;
        
        if (showDebugInfo)
        {
            Debug.Log($"🎯 Player is now standing on cube: {cubeNumber}");
        }
        
        Debug.Log($"[CUBE] You are standing on cube: {cubeNumber}"); // Always log for on-screen console
        DisplayCurrentCubeNumber(cubeNumber);
    }
    
    void OnPlayerExitedCube()
    {
        currentCubeNumber = -999; // Not on any cube
        
        if (showDebugInfo)
        {
            Debug.Log("🚶 Player is no longer on any cube");
        }
        
        // Clear the display
        DisplayCurrentCubeNumber(-999);
    }
    
    void DisplayCurrentCubeNumber(int cubeNumber)
    {
        if (cubeNumber == -999)
        {
            // Player is not on any cube
            Debug.Log("📍 Current Position: Not on any cube");
        }
        else
        {
            // Player is on a specific cube
            Debug.Log($"📍 Current Position: Standing on cube {cubeNumber}");
        }
    }
    
    // Public methods for external control
    public void SetCurrentNumber(int number)
    {
        if (number >= -20 && number <= 20)
        {
            MoveToNumber(number);
        }
    }
    
    public int GetCurrentNumber()
    {
        return currentNumber;
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public Vector3 GetPlayerPosition()
    {
        return playerTransform.position;
    }
    
    public void ResetPosition()
    {
        currentNumber = 0;
        lastValidNumber = 0;
        playerTransform.position = initialPosition;
        lastPosition = initialPosition;
        
        if (numberLineGenerator != null)
        {
            numberLineGenerator.UpdatePlayerPosition(0);
        }
    }
    
    // Manual movement methods for testing
    public void MoveLeft()
    {
        int newNumber = currentNumber - 1;
        if (newNumber >= -20)
        {
            MoveToNumber(newNumber);
        }
    }
    
    public void MoveRight()
    {
        int newNumber = currentNumber + 1;
        if (newNumber <= 20)
        {
            MoveToNumber(newNumber);
        }
    }
    

    
    // Context menu for testing
    [ContextMenu("Test Move to 5")]
    void TestMoveTo5()
    {
        SetCurrentNumber(5);
    }
    
    [ContextMenu("Test Move to -3")]
    void TestMoveToNegative3()
    {
        SetCurrentNumber(-3);
    }
    
    [ContextMenu("Reset Position")]
    void TestResetPosition()
    {
        ResetPosition();
        Debug.Log("Position reset to 0");
    }
    
    [ContextMenu("Test Complete Movement System")]
    public void TestCompleteMovementSystem()
    {
        Debug.Log("=== TESTING COMPLETE MOVEMENT SYSTEM ===");
        
        if (numberLineGenerator == null)
            numberLineGenerator = FindObjectOfType<ARNumberLineGenerator>();
        
        if (numberLineGenerator == null)
        {
            Debug.LogError("❌ Number Line Generator not found!");
            return;
        }
        
        Debug.Log($"✅ Number Line Generator: {numberLineGenerator.name}");
        Debug.Log($"✅ Number Line Visible: {numberLineGenerator.IsNumberLineVisible()}");
        Debug.Log($"✅ Current Player Number: {currentNumber}");
        Debug.Log($"✅ Current Cube Number: {currentCubeNumber}");
        
        // Test movement sequence
        int[] testMovements = { -10, -5, 0, 5, 10, 15 };
        
        foreach (int targetNumber in testMovements)
        {
            Debug.Log($"\n--- Moving to number {targetNumber} ---");
            
            // Update player position
            SetCurrentNumber(targetNumber);
            
            // Update number line visibility
            if (numberLineGenerator != null)
            {
                numberLineGenerator.UpdatePlayerPosition(targetNumber);
                numberLineGenerator.UpdateVisibleCubes(targetNumber);
            }
            
            // Simulate cube detection
            OnPlayerEnteredCube(targetNumber);
            
            Debug.Log($"✅ Moved to {targetNumber}");
            Debug.Log($"✅ Cube detection: {currentCubeNumber}");
            
            // Wait a moment to see the effect
            System.Threading.Thread.Sleep(100);
        }
        
        Debug.Log("\n=== COMPLETE MOVEMENT SYSTEM TEST FINISHED ===");
    }

    [ContextMenu("Test Player Movement")]
    public void TestPlayerMovement()
    {
        Debug.Log("=== TESTING PLAYER MOVEMENT ===");
        
        if (numberLineGenerator != null)
        {
            Debug.Log($"Number Line Generator: {(numberLineGenerator.IsNumberLineVisible() ? "Visible" : "Hidden")}");
            Debug.Log($"Current Number: {currentNumber}");
            
            // Test moving to different numbers
            SetCurrentNumber(5);
            Debug.Log("Moved to 5");
            
            SetCurrentNumber(-3);
            Debug.Log("Moved to -3");
            
            SetCurrentNumber(0);
            Debug.Log("Moved back to 0");
        }
        else
        {
            Debug.LogError("Number Line Generator not found!");
        }
    }
    
    [ContextMenu("Test Cube Detection")]
    public void TestCubeDetection()
    {
        Debug.Log("=== TESTING CUBE DETECTION ===");
        
        if (numberLineGenerator != null)
        {
            Debug.Log($"Current Cube Number: {currentCubeNumber}");
            Debug.Log($"Cube Detection Enabled: {enableCubeDetection}");
            
            // Simulate entering different cubes
            OnPlayerEnteredCube(5);
            Debug.Log("Simulated entering cube 5");
            
            OnPlayerEnteredCube(-3);
            Debug.Log("Simulated entering cube -3");
            
            OnPlayerExitedCube();
            Debug.Log("Simulated exiting cube");
        }
        else
        {
            Debug.LogError("Number Line Generator not found!");
        }
    }
    
    [ContextMenu("Setup Editor Test")]
    void SetupEditorTest()
    {
        Debug.Log("=== SETTING UP EDITOR TEST ===");
        
        // Create AR Number Line Generator if it doesn't exist
        if (numberLineGenerator == null)
        {
            Debug.Log("Creating AR Number Line Generator for editor testing...");
            GameObject generatorGO = new GameObject("AR Number Line Generator");
            numberLineGenerator = generatorGO.AddComponent<ARNumberLineGenerator>();
            
            // Generate the number line
            numberLineGenerator.GenerateNumberLine();
            numberLineGenerator.ShowNumberLine();
            
            Debug.Log("✅ AR Number Line Generator created and number line generated");
        }
        else
        {
            Debug.Log("✅ AR Number Line Generator already exists");
        }
        
        // Now test the movement
        TestPlayerMovement();
        TestCubeDetection();
    }
} 