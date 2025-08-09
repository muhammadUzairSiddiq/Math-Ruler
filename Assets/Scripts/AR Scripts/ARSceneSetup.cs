using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSceneSetup : MonoBehaviour
{
    [Header("AR Components")]
    public ARSession arSession;
    public ARSessionOrigin arSessionOrigin;
    public ARCameraManager arCameraManager;
    public ARPlaneManager arPlaneManager;
    public ARRaycastManager arRaycastManager;
    public ARAnchorManager arAnchorManager;
    
    [Header("Game Components")]
    public ARNumberLineGenerator numberLineGenerator;
    public ARPlayerController playerController;
    public ARGameManager gameManager;
    
    [Header("Number Line Settings")]
    public GameObject numberCubePrefab;
    public bool autoSetupNumberLine = true;
    
    [Header("UI Components")]
    public Canvas worldSpaceCanvas;
    public Canvas screenSpaceCanvas;
    
    [ContextMenu("Setup AR Scene")]
    void SetupARSceneFromMenu()
    {
        SetupARScene();
    }
    
    [ContextMenu("Setup Cube Position UI")]
    void SetupCubePositionUIMenu()
    {
        SetupCubePositionUI();
    }
    
    void SetupARScene()
    {
        Debug.Log("=== AR SCENE SETUP ===");
        
        // Create AR Session if not exists
        if (arSession == null)
        {
            GameObject sessionGO = new GameObject("AR Session");
            arSession = sessionGO.AddComponent<ARSession>();
            Debug.Log("Created AR Session");
        }
        
        // Create AR Session Origin if not exists
        if (arSessionOrigin == null)
        {
            GameObject originGO = new GameObject("AR Session Origin");
            arSessionOrigin = originGO.AddComponent<ARSessionOrigin>();
            Debug.Log("Created AR Session Origin");
        }
        
        // Check for deprecation warning
        Debug.LogWarning("ARSessionOrigin is deprecated. Consider using XROrigin for newer projects.");
        
        // Setup AR Camera
        SetupARCamera();
        
        // Setup AR Managers
        SetupARManagers();
        
        // Setup Game Components
        SetupGameComponents();
        
        // Setup Number Line
        if (autoSetupNumberLine)
        {
            SetupNumberLine();
        }
        
        // Setup UI
        SetupUI();
        
        // Setup Debug UI
        SetupDebugUI();
        
        Debug.Log("AR Scene setup complete!");
    }
    
    void SetupARCamera()
    {
        if (arCameraManager == null)
        {
            // Find existing AR Camera
            Camera arCamera = FindObjectOfType<Camera>();
            if (arCamera == null)
            {
                GameObject cameraGO = new GameObject("AR Camera");
                arCamera = cameraGO.AddComponent<Camera>();
                cameraGO.AddComponent<AudioListener>();
            }
            
            // Check if ARCameraManager already exists
            arCameraManager = arCamera.gameObject.GetComponent<ARCameraManager>();
            if (arCameraManager == null)
            {
                // Add AR Camera Manager only if it doesn't exist
            arCameraManager = arCamera.gameObject.AddComponent<ARCameraManager>();
                Debug.Log("Added AR Camera Manager");
            }
            else
            {
                Debug.Log("AR Camera Manager already exists");
            }
            
            // Parent to AR Session Origin
            arCamera.transform.SetParent(arSessionOrigin.transform);
            
            Debug.Log("AR Camera setup complete");
        }
    }
    
    void SetupARManagers()
    {
        // Create AR Plane Manager
        if (arPlaneManager == null)
        {
            GameObject planeManagerGO = new GameObject("AR Plane Manager");
            planeManagerGO.transform.SetParent(arSessionOrigin.transform);
            arPlaneManager = planeManagerGO.AddComponent<ARPlaneManager>();
            Debug.Log("Created AR Plane Manager");
        }
        
        // Create AR Raycast Manager
        if (arRaycastManager == null)
        {
            GameObject raycastManagerGO = new GameObject("AR Raycast Manager");
            raycastManagerGO.transform.SetParent(arSessionOrigin.transform);
            arRaycastManager = raycastManagerGO.AddComponent<ARRaycastManager>();
            Debug.Log("Created AR Raycast Manager");
        }
        
        // Create AR Anchor Manager
        if (arAnchorManager == null)
        {
            GameObject anchorManagerGO = new GameObject("AR Anchor Manager");
            anchorManagerGO.transform.SetParent(arSessionOrigin.transform);
            arAnchorManager = anchorManagerGO.AddComponent<ARAnchorManager>();
            Debug.Log("Created AR Anchor Manager");
        }
    }
    
    void SetupGameComponents()
    {
        // Create AR Game Manager if not exists
        if (gameManager == null)
        {
            GameObject gameManagerGO = new GameObject("AR Game Manager");
            gameManager = gameManagerGO.AddComponent<ARGameManager>();
            Debug.Log("Created AR Game Manager");
        }
        
        // Create AR Player Controller if not exists
        if (playerController == null)
        {
            GameObject playerControllerGO = new GameObject("AR Player Controller");
            playerController = playerControllerGO.AddComponent<ARPlayerController>();
            Debug.Log("Created AR Player Controller");
        }
        
        // Create AR Placement Manager
        if (FindObjectOfType<ARPlacementManager>() == null)
        {
            GameObject placementManagerGO = new GameObject("AR Placement Manager");
            placementManagerGO.AddComponent<ARPlacementManager>();
            Debug.Log("Created AR Placement Manager");
        }
        
        // Create AR Number Line Generator
        if (FindObjectOfType<ARNumberLineGenerator>() == null)
        {
            GameObject generatorGO = new GameObject("AR Number Line Generator");
            generatorGO.AddComponent<ARNumberLineGenerator>();
            Debug.Log("Created AR Number Line Generator");
        }
        
        // Setup Cube Position UI
        SetupCubePositionUI();
    }
    
    void SetupNumberLine()
    {
        // Create AR Number Line Generator if not exists
        if (numberLineGenerator == null)
        {
            GameObject numberLineGeneratorGO = new GameObject("AR Number Line Generator");
            numberLineGenerator = numberLineGeneratorGO.AddComponent<ARNumberLineGenerator>();
            Debug.Log("Created AR Number Line Generator");
        }
        
        // Find number cube prefab if not assigned
        if (numberCubePrefab == null)
        {
            numberCubePrefab = Resources.Load<GameObject>("NumberCubePrefab");
            if (numberCubePrefab == null)
            {
                // Try to find it in the project
                numberCubePrefab = FindObjectOfType<GameObject>();
                if (numberCubePrefab != null)
                {
                    Debug.Log("Found number cube prefab in scene");
                }
            }
        }
        
        // Assign prefab to number line generator
        if (numberLineGenerator != null && numberCubePrefab != null)
        {
            numberLineGenerator.cubePrefab = numberCubePrefab;
            Debug.Log("Assigned number cube prefab to number line manager");
        }
        
        // Connect number line manager to player controller
        if (playerController != null && numberLineGenerator != null)
        {
            playerController.numberLineGenerator = numberLineGenerator;
            Debug.Log("Connected number line manager to player controller");
        }
    }
    
    void SetupCubePositionUI()
    {
        Debug.Log("=== SETTING UP CUBE POSITION UI ===");
        
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("Creating Canvas for cube position UI...");
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create UI Panel for cube position
        GameObject uiPanel = GameObject.Find("Cube Position Panel");
        if (uiPanel == null)
        {
            Debug.Log("Creating Cube Position Panel...");
            uiPanel = new GameObject("Cube Position Panel");
            uiPanel.transform.SetParent(canvas.transform, false);
            
            // Add RectTransform
            RectTransform rectTransform = uiPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.9f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(300, 80);
            
            // Add Image component for background
            UnityEngine.UI.Image background = uiPanel.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(0, 0, 0, 0.8f);
            
            // Create Text GameObject
            GameObject textGO = new GameObject("Position Text");
            textGO.transform.SetParent(uiPanel.transform, false);
            
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Add TextMeshProUGUI component
            TMPro.TextMeshProUGUI textComponent = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = "Not on any cube";
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Add CubePositionUI component
            CubePositionUI cubePositionUI = uiPanel.AddComponent<CubePositionUI>();
            cubePositionUI.positionText = textComponent;
            cubePositionUI.positionPanel = uiPanel;
            
            Debug.Log("✅ Cube Position UI created successfully");
        }
        else
        {
            Debug.Log("✅ Cube Position Panel already exists");
        }
    }
    
    void SetupUI()
    {
        // Create world space canvas if needed
        if (worldSpaceCanvas == null)
        {
            GameObject worldCanvasGO = new GameObject("World Space Canvas");
            worldSpaceCanvas = worldCanvasGO.AddComponent<Canvas>();
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            worldSpaceCanvas.worldCamera = Camera.main;
            Debug.Log("Created World Space Canvas");
        }
        
        // Create screen space canvas if needed
        if (screenSpaceCanvas == null)
        {
            GameObject screenCanvasGO = new GameObject("Screen Space Canvas");
            screenSpaceCanvas = screenCanvasGO.AddComponent<Canvas>();
            screenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Debug.Log("Created Screen Space Canvas");
        }
    }
    
    [ContextMenu("Check AR Setup")]
    void CheckARSetup()
    {
        Debug.Log("=== AR SETUP CHECK ===");
        
        bool allComponentsFound = true;
        
        // Check AR components
        if (arSession == null) { Debug.LogError("AR Session missing!"); allComponentsFound = false; }
        if (arSessionOrigin == null) { Debug.LogError("AR Session Origin missing!"); allComponentsFound = false; }
        if (arCameraManager == null) { Debug.LogError("AR Camera Manager missing!"); allComponentsFound = false; }
        if (arPlaneManager == null) { Debug.LogError("AR Plane Manager missing!"); allComponentsFound = false; }
        if (arRaycastManager == null) { Debug.LogError("AR Raycast Manager missing!"); allComponentsFound = false; }
        if (arAnchorManager == null) { Debug.LogError("AR Anchor Manager missing!"); allComponentsFound = false; }
        
        // Check game components
        if (numberLineGenerator == null) { Debug.LogError("AR Number Line Generator missing!"); allComponentsFound = false; }
        if (playerController == null) { Debug.LogError("AR Player Controller missing!"); allComponentsFound = false; }
        if (gameManager == null) { Debug.LogError("AR Game Manager missing!"); allComponentsFound = false; }
        
        // Check prefabs
        if (numberCubePrefab == null) { Debug.LogError("Number Cube Prefab missing!"); allComponentsFound = false; }
        
        if (allComponentsFound)
        {
            Debug.Log("✅ All AR components found and ready!");
        }
        else
        {
            Debug.LogError("❌ Some AR components are missing. Run Setup AR Scene to fix.");
        }
    }
    
    void SetupDebugUI()
    {
        // Debug UI removed - no longer needed
    }
    
    [ContextMenu("Test Number Line")]
    void TestNumberLine()
    {
        if (numberLineGenerator != null)
        {
            numberLineGenerator.GenerateNumberLine();
        }
        else
        {
            Debug.LogError("Number Line Generator not found!");
        }
    }

    [ContextMenu("Setup Game Components")]
    void SetupGameComponentsFromMenu()
    {
        SetupGameComponents();
    }
} 