using UnityEngine;

public class MobileGameManager : MonoBehaviour
{
    [Header("Mobile Components")]
    public PlayerController playerController;
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public GameManager gameManager;
    public AudioManager audioManager;
    
    [Header("Mobile Settings")]
    public bool enableMobileMode = true;
    
    void Start()
    {
        InitializeMobileGame();
    }
    
    void InitializeMobileGame()
    {
        Debug.Log("=== MOBILE GAME MANAGER INITIALIZATION ===");
        
        // Find components if not assigned
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (answerVerifier == null) answerVerifier = FindObjectOfType<AnswerVerifier>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        
        // Configure for mobile mode
        ConfigureMobileMode();
        
        // Play mobile background music
        if (audioManager != null)
        {
            audioManager.PlayMusic("background_music");
        }
        
        Debug.Log("Mobile Game Manager initialized");
    }
    
    void ConfigureMobileMode()
    {
        Debug.Log("Configuring Mobile Mode");
        
        // Ensure mobile components are enabled
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Configure AnswerVerifier for mobile
        if (answerVerifier != null)
        {
            answerVerifier.playerController = playerController;
            answerVerifier.arPlayerController = null; // Ensure AR controller is null
        }
        
        // Configure UI for mobile
        if (uiManager != null)
        {
            // Mobile-specific UI configuration
            Debug.Log("Mobile UI configured");
        }
    }
    
    public void OnBackToMainMenuClicked()
    {
        Debug.Log("Back to main menu clicked from mobile game");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Load main menu scene
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    public void OnRestartGameClicked()
    {
        Debug.Log("Restart game clicked");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Restart the current scene
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.RestartCurrentScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
} 