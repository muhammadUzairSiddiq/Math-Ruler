using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARGameManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARNumberLineManager numberLineManager;
    public ARPlayerController arPlayerController;
    public ARSession arSession;
    
    [Header("Game Integration")]
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public GameManager gameManager;
    public AudioManager audioManager;
    
    [Header("AR Game Settings")]
    public bool enableARMode = true;
    public float arMovementSensitivity = 0.5f;
    public bool showARInstructions = true;
    
    [Header("UI Elements")]
    public GameObject arInstructionsPanel;
    public GameObject arStatusPanel;
    public TMPro.TextMeshProUGUI arStatusText;
    public Button backToMainMenuButton;
    
    // Private variables
    private bool arInitialized = false;
    private bool gameStarted = false;
    
    void Start()
    {
        InitializeARGame();
    }
    
    void InitializeARGame()
    {
        Debug.Log("=== AR GAME MANAGER INITIALIZATION ===");
        
        // Find AR components
        if (numberLineManager == null) numberLineManager = FindObjectOfType<ARNumberLineManager>();
        if (arPlayerController == null) arPlayerController = FindObjectOfType<ARPlayerController>();
        if (arSession == null) arSession = FindObjectOfType<ARSession>();
        
        // Find game components
        if (answerVerifier == null) answerVerifier = FindObjectOfType<AnswerVerifier>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize AR session
        if (arSession != null)
        {
            ARSession.stateChanged += OnARSessionStateChanged;
        }
        
        // Setup UI buttons
        SetupUIButtons();
        
        // Show AR instructions
        if (showARInstructions && arInstructionsPanel != null)
        {
            arInstructionsPanel.SetActive(true);
        }
        
        // Play AR background music
        if (audioManager != null)
        {
            audioManager.PlayMusic("ar_background_music");
        }
        
        arInitialized = true;
        Debug.Log("AR Game Manager initialized");
    }
    
    void SetupUIButtons()
    {
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);
        }
    }
    
    void SubscribeToEvents()
    {
        // Subscribe to number line events
        if (numberLineManager != null)
        {
            numberLineManager.OnNumberLinePlaced += OnNumberLinePlaced;
            numberLineManager.OnPlayerPositionChanged += OnPlayerPositionChanged;
        }
        
        // Subscribe to player events
        if (arPlayerController != null)
        {
            arPlayerController.OnNumberChanged += OnPlayerNumberChanged;
            arPlayerController.OnPlayerMoved += OnPlayerMoved;
        }
        
        // Subscribe to game events
        if (answerVerifier != null)
        {
            answerVerifier.OnAnswerChecked += OnAnswerChecked;
        }
    }
    
    void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        Debug.Log($"AR Session State: {args.state}");
        
        switch (args.state)
        {
            case ARSessionState.None:
                UpdateARStatus("AR Session not ready");
                break;
            case ARSessionState.Unsupported:
                UpdateARStatus("AR not supported on this device");
                break;
            case ARSessionState.CheckingAvailability:
                UpdateARStatus("Checking AR availability...");
                break;
            case ARSessionState.NeedsInstall:
                UpdateARStatus("AR needs installation");
                break;
            case ARSessionState.Installing:
                UpdateARStatus("Installing AR...");
                break;
            case ARSessionState.Ready:
                UpdateARStatus("AR ready - Move device to detect floor");
                break;
            case ARSessionState.SessionInitializing:
                UpdateARStatus("Initializing AR session...");
                break;
            case ARSessionState.SessionTracking:
                UpdateARStatus("AR tracking active");
                break;
        }
    }
    
    void OnNumberLinePlaced(Vector3 position)
    {
        Debug.Log("Number line placed in AR!");
        UpdateARStatus("Number line placed! Start walking to solve equations.");
        
        // Hide instructions
        if (arInstructionsPanel != null)
        {
            arInstructionsPanel.SetActive(false);
        }
        
        // Start the game
        if (!gameStarted)
        {
            StartARGame();
        }
    }
    
    void OnPlayerPositionChanged(int number)
    {
        // This is called when the player is near a number on the line
        Debug.Log($"Player near number: {number}");
        
        // Update UI if needed
        if (uiManager != null)
        {
            // Update equation display with current position
            if (answerVerifier != null)
            {
                answerVerifier.UpdateEquationDisplay();
            }
        }
    }
    
    void OnPlayerNumberChanged(int newNumber)
    {
        Debug.Log($"Player moved to number: {newNumber}");
        
        // Update the answer verifier with new position
        if (answerVerifier != null && arPlayerController != null)
        {
            // Update the AR player controller in answer verifier
            answerVerifier.arPlayerController = arPlayerController;
            
            // Update equation display
            answerVerifier.UpdateEquationDisplay();
        }
        
        // Play movement sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("movement");
        }
    }
    
    void OnPlayerMoved(Vector3 movement)
    {
        // Player moved in AR space
        Debug.Log($"Player moved: {movement}");
        
        // Update UI or provide feedback
        if (arStatusText != null)
        {
            arStatusText.text = $"Moving... Current position: {arPlayerController.GetCurrentNumber()}";
        }
    }
    
    void OnAnswerChecked(bool isCorrect)
    {
        Debug.Log($"Answer checked in AR: {(isCorrect ? "Correct" : "Wrong")}");
        
        // Handle AR-specific feedback
        if (isCorrect)
        {
            // Place reward in AR space
            if (numberLineManager != null && answerVerifier != null)
            {
                int correctAnswer = answerVerifier.correctAnswer;
                GameObject rewardPrefab = GetRewardPrefab();
                if (rewardPrefab != null)
                {
                    numberLineManager.PlaceRewardAtNumber(correctAnswer, rewardPrefab);
                }
            }
            
            // Play success sound
            if (audioManager != null)
            {
                audioManager.PlaySFX("reward");
            }
        }
        else
        {
            // Play error sound
            if (audioManager != null)
            {
                audioManager.PlaySFX("wrong");
            }
        }
    }
    
    void StartARGame()
    {
        gameStarted = true;
        Debug.Log("AR Game started!");
        
        // Initialize game systems
        if (answerVerifier != null)
        {
            answerVerifier.GenerateNewEquation();
        }
        
        // Update status
        UpdateARStatus("Game started! Walk to the correct answer.");
    }
    
    void UpdateARStatus(string status)
    {
        Debug.Log($"AR Status: {status}");
        
        if (arStatusText != null)
        {
            arStatusText.text = status;
        }
    }
    
    GameObject GetRewardPrefab()
    {
        // Get reward prefab based on current streak or random selection
        if (answerVerifier != null)
        {
            // Access correct answers through public method
            int correctAnswers = answerVerifier.GetCorrectAnswersInRow();
            switch (correctAnswers)
            {
                case 1: return answerVerifier.housePrefab;
                case 2: return answerVerifier.petPrefab;
                case 3: return answerVerifier.carPrefab;
                case 4: return answerVerifier.treePrefab;
                default: return answerVerifier.housePrefab;
            }
        }
        return null;
    }
    
    public void ResetARGame()
    {
        gameStarted = false;
        
        // Reset player position
        if (arPlayerController != null)
        {
            arPlayerController.ResetPosition();
        }
        
        // Reset number line
        if (numberLineManager != null)
        {
            // This would require recreating the number line
            Debug.Log("Reset AR Game");
        }
        
        UpdateARStatus("AR Game reset");
    }
    
    public void OnBackToMainMenuClicked()
    {
        Debug.Log("Back to main menu clicked from AR game");
        
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
    
    public void OnRestartARGameClicked()
    {
        Debug.Log("Restart AR game clicked");
        
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
    
    public bool IsARReady()
    {
        return arInitialized && numberLineManager.IsNumberLinePlaced();
    }
    
    public bool IsGameStarted()
    {
        return gameStarted;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (arSession != null)
        {
            ARSession.stateChanged -= OnARSessionStateChanged;
        }
    }
} 