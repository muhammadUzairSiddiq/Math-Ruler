using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    public enum GameMode
    {
        Mobile,
        AR
    }
    
    [Header("Mode Settings")]
    public GameMode currentMode = GameMode.Mobile;
    public bool autoDetectARSupport = true;
    public bool allowModeSwitching = true;
    
    [Header("Scene References")]
    public string mobileSceneName = "MobileGame";
    public string arSceneName = "ARGame";
    
    [Header("Components")]
    public PlayerController mobilePlayerController;
    public ARPlayerController arPlayerController;
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public GameManager gameManager;
    public AudioManager audioManager;
    
    // Events
    public System.Action<GameMode> OnGameModeChanged;
    public System.Action<bool> OnARSupportChanged;
    
    private bool arSupported = false;
    private bool initialized = false;
    
    void Start()
    {
        InitializeGameModeManager();
    }
    
    void InitializeGameModeManager()
    {
        Debug.Log("=== GAME MODE MANAGER INITIALIZATION ===");
        
        // Check AR support
        CheckARSupport();
        
        // Find components
        FindComponents();
        
        // Set up initial mode
        SetGameMode(currentMode);
        
        initialized = true;
        Debug.Log($"Game Mode Manager initialized - Current Mode: {currentMode}");
    }
    
    void CheckARSupport()
    {
        // Check if AR Foundation is available
        #if UNITY_XR_ARFOUNDATION
            arSupported = true;
            Debug.Log("AR Foundation detected - AR mode available");
        #else
            arSupported = false;
            Debug.Log("AR Foundation not available - Mobile mode only");
        #endif
        
        OnARSupportChanged?.Invoke(arSupported);
        
        // Auto-switch to mobile if AR not supported
        if (autoDetectARSupport && !arSupported && currentMode == GameMode.AR)
        {
            Debug.Log("AR not supported, switching to mobile mode");
            SetGameMode(GameMode.Mobile);
        }
    }
    
    void FindComponents()
    {
        // Find mobile components
        if (mobilePlayerController == null)
            mobilePlayerController = FindObjectOfType<PlayerController>();
        
        // Find AR components
        if (arPlayerController == null)
            arPlayerController = FindObjectOfType<ARPlayerController>();
        
        // Find shared components
        if (answerVerifier == null)
            answerVerifier = FindObjectOfType<AnswerVerifier>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
    }
    
    public void SetGameMode(GameMode mode)
    {
        if (!allowModeSwitching) return;
        
        // Check if mode is supported
        if (mode == GameMode.AR && !arSupported)
        {
            Debug.LogWarning("AR mode requested but not supported on this device");
            return;
        }
        
        GameMode previousMode = currentMode;
        currentMode = mode;
        
        Debug.Log($"Switching game mode from {previousMode} to {currentMode}");
        
        // Configure components for new mode
        ConfigureForMode(mode);
        
        // Notify other systems
        OnGameModeChanged?.Invoke(mode);
        
        // Update UI
        UpdateUIForMode(mode);
    }
    
    void ConfigureForMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Mobile:
                ConfigureMobileMode();
                break;
            case GameMode.AR:
                ConfigureARMode();
                break;
        }
    }
    
    void ConfigureMobileMode()
    {
        Debug.Log("Configuring Mobile Mode");
        
        // Enable mobile components
        if (mobilePlayerController != null)
        {
            mobilePlayerController.enabled = true;
        }
        
        // Disable AR components
        if (arPlayerController != null)
        {
            arPlayerController.enabled = false;
        }
        
        // Configure AnswerVerifier for mobile
        if (answerVerifier != null)
        {
            answerVerifier.playerController = mobilePlayerController;
            answerVerifier.arPlayerController = null;
        }
        
        // Play mobile-specific audio
        if (audioManager != null)
        {
            audioManager.PlayMusic("background_music");
        }
    }
    
    void ConfigureARMode()
    {
        Debug.Log("Configuring AR Mode");
        
        // Disable mobile components
        if (mobilePlayerController != null)
        {
            mobilePlayerController.enabled = false;
        }
        
        // Enable AR components
        if (arPlayerController != null)
        {
            arPlayerController.enabled = true;
        }
        
        // Configure AnswerVerifier for AR
        if (answerVerifier != null)
        {
            answerVerifier.playerController = null;
            answerVerifier.arPlayerController = arPlayerController;
        }
        
        // Play AR-specific audio
        if (audioManager != null)
        {
            audioManager.PlayMusic("ar_background_music");
        }
    }
    
    void UpdateUIForMode(GameMode mode)
    {
        if (uiManager == null) return;
        
        switch (mode)
        {
            case GameMode.Mobile:
                // Show mobile-specific UI
                Debug.Log("Updated UI for Mobile Mode");
                break;
            case GameMode.AR:
                // Show AR-specific UI
                Debug.Log("Updated UI for AR Mode");
                break;
        }
    }
    
    public void SwitchToMobileMode()
    {
        SetGameMode(GameMode.Mobile);
    }
    
    public void SwitchToARMode()
    {
        if (arSupported)
        {
            SetGameMode(GameMode.AR);
        }
        else
        {
            Debug.LogWarning("Cannot switch to AR mode - AR not supported");
        }
    }
    
    public void ToggleGameMode()
    {
        if (currentMode == GameMode.Mobile)
        {
            SwitchToARMode();
        }
        else
        {
            SwitchToMobileMode();
        }
    }
    
    public bool IsARSupported()
    {
        return arSupported;
    }
    
    public GameMode GetCurrentMode()
    {
        return currentMode;
    }
    
    public bool IsMobileMode()
    {
        return currentMode == GameMode.Mobile;
    }
    
    public bool IsARMode()
    {
        return currentMode == GameMode.AR;
    }
    
    // Scene management methods
    public void LoadMobileScene()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene(mobileSceneName);
        }
        else
        {
            SceneManager.LoadScene(mobileSceneName);
        }
    }
    
    public void LoadARScene()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene(arSceneName);
        }
        else
        {
            SceneManager.LoadScene(arSceneName);
        }
    }
    
    // UI Button methods
    public void OnMobileModeButtonClicked()
    {
        SwitchToMobileMode();
    }
    
    public void OnARModeButtonClicked()
    {
        SwitchToARMode();
    }
    
    public void OnToggleModeButtonClicked()
    {
        ToggleGameMode();
    }
    
    void OnDestroy()
    {
        // Clean up any mode-specific resources
        if (currentMode == GameMode.AR)
        {
            // Clean up AR resources
            Debug.Log("Cleaning up AR resources");
        }
    }
} 