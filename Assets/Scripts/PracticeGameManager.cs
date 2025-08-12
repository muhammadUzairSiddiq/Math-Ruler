using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PracticeGameManager : MonoBehaviour
{
    [Header("Practice Components")]
    public PlayerController playerController;
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public GameManager gameManager;
    public AudioManager audioManager;
    
    [Header("Practice Settings")]
    public bool enablePracticeMode = true;
    
    [Header("Tutorial System")]
    public bool showTutorial = true;
    public int currentTutorialStep = 0;
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button tutorialNextButton;
    public Button tutorialSkipButton;
    public Button tutorialPreviousButton;
    
    [Header("Practice Mode Features")]
    public bool unlimitedAttempts = true;
    public bool showHints = true;
    public bool showAnswerAfterWrong = false;
    public int maxHintsPerQuestion = 3;
    
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(3, 6)]
        public string description;
        public Sprite tutorialImage;
        public bool requiresAction;
        public string actionDescription;
    }
    
    void Start()
    {
        InitializePracticeGame();
    }
    
    void InitializePracticeGame()
    {
        Debug.Log("=== PRACTICE GAME MANAGER INITIALIZATION ===");
        
        // Find components if not assigned
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (answerVerifier == null) answerVerifier = FindObjectOfType<AnswerVerifier>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        
        // Configure for practice mode
        ConfigurePracticeMode();
        
        // Initialize tutorial system
        InitializeTutorialSystem();
        
        // Play practice background music
        if (audioManager != null)
        {
            audioManager.PlayMusic("practice_music"); // You can use same as mobile or create new
        }
        
        Debug.Log("Practice Game Manager initialized");
    }
    
    void ConfigurePracticeMode()
    {
        Debug.Log("Configuring Practice Mode");
        
        // Configure AnswerVerifier for practice mode (uses mobile mechanics)
        if (answerVerifier != null && playerController != null)
        {
            answerVerifier.playerController = playerController;
            Debug.Log("✅ AnswerVerifier configured for practice mode");
        }
        
        // Configure UI for practice mode
        if (uiManager != null)
        {
            Debug.Log("✅ Practice UI configured");
        }
    }
    
    void InitializeTutorialSystem()
    {
        Debug.Log("Initializing Tutorial System");
        
        // Create default tutorial steps if none exist
        if (tutorialSteps.Count == 0)
        {
            CreateDefaultTutorialSteps();
        }
        
        // Setup tutorial UI
        SetupTutorialUI();
        
        // Show first tutorial step if tutorial is enabled
        if (showTutorial)
        {
            ShowTutorialStep(0);
        }
        else
        {
            HideTutorialPanel();
        }
    }
    
    void CreateDefaultTutorialSteps()
    {
        tutorialSteps.Clear();
        
        // Step 1: Welcome
        tutorialSteps.Add(new TutorialStep
        {
            title = "Welcome to Practice Mode!",
            description = "This is a safe space to learn math without pressure. You can make mistakes and try again as many times as you need.",
            requiresAction = false
        });
        
        // Step 2: How to Play
        tutorialSteps.Add(new TutorialStep
        {
            title = "How to Play",
            description = "1. Read the math equation\n2. Move your character to the number you think is the answer\n3. Click Submit to check your answer\n4. Learn from your mistakes and try again!",
            requiresAction = false
        });
        
        // Step 3: Practice Features
        tutorialSteps.Add(new TutorialStep
        {
            title = "Practice Features",
            description = "• Unlimited attempts - try as many times as you want\n• Hints available - get help when you're stuck\n• No pressure - this is just for learning",
            requiresAction = false
        });
        
        // Step 4: Ready to Start
        tutorialSteps.Add(new TutorialStep
        {
            title = "Ready to Start?",
            description = "Click 'Start Practice' when you're ready to begin your math learning journey!",
            requiresAction = true,
            actionDescription = "Click Start Practice button"
        });
        
        Debug.Log($"Created {tutorialSteps.Count} tutorial steps");
    }
    
    void SetupTutorialUI()
    {
        if (tutorialPanel != null)
        {
            // Setup button listeners
            if (tutorialNextButton != null)
            {
                tutorialNextButton.onClick.AddListener(OnTutorialNextClicked);
            }
            
            if (tutorialPreviousButton != null)
            {
                tutorialPreviousButton.onClick.AddListener(OnTutorialPreviousClicked);
            }
            
            if (tutorialSkipButton != null)
            {
                tutorialSkipButton.onClick.AddListener(OnTutorialSkipClicked);
            }
            
            Debug.Log("Tutorial UI setup complete");
        }
        else
        {
            Debug.LogWarning("Tutorial panel not assigned!");
        }
    }
    
    public void ShowTutorialStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialSteps.Count)
        {
            Debug.LogWarning($"Invalid tutorial step index: {stepIndex}");
            return;
        }
        
        currentTutorialStep = stepIndex;
        TutorialStep step = tutorialSteps[stepIndex];
        
        // Update UI
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (tutorialText != null) tutorialText.text = $"{step.title}\n\n{step.description}";
        
        // Update button states
        if (tutorialPreviousButton != null) tutorialPreviousButton.interactable = (stepIndex > 0);
        if (tutorialNextButton != null) tutorialNextButton.interactable = (stepIndex < tutorialSteps.Count - 1);
        
        Debug.Log($"Showing tutorial step {stepIndex + 1}/{tutorialSteps.Count}: {step.title}");
    }
    
    public void HideTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            Debug.Log("Tutorial panel hidden");
        }
    }
    
    public void OnTutorialNextClicked()
    {
        if (currentTutorialStep < tutorialSteps.Count - 1)
        {
            ShowTutorialStep(currentTutorialStep + 1);
        }
        else
        {
            // Tutorial complete
            CompleteTutorial();
        }
    }
    
    public void OnTutorialPreviousClicked()
    {
        if (currentTutorialStep > 0)
        {
            ShowTutorialStep(currentTutorialStep - 1);
        }
    }
    
    public void OnTutorialSkipClicked()
    {
        Debug.Log("Tutorial skipped by user");
        CompleteTutorial();
    }
    
    void CompleteTutorial()
    {
        showTutorial = false;
        HideTutorialPanel();
        Debug.Log("Tutorial completed - starting practice mode");
        
        // You can add any post-tutorial logic here
        // For example, show a "Let's Begin!" message
    }
    
    // Practice mode specific methods
    public void ShowHint()
    {
        if (!showHints) return;
        
        // Get current equation info from AnswerVerifier
        if (answerVerifier != null)
        {
            string hint = GenerateHint(answerVerifier.firstNumber, answerVerifier.secondNumber, answerVerifier.GetOperator());
            ShowHintMessage(hint);
        }
    }
    
    string GenerateHint(int first, int second, string op)
    {
        switch (op)
        {
            case "+":
                return $"Hint: {first} + {second} = ?\nThink: Start with {first}, then count up {second} more!";
            case "-":
                return $"Hint: {first} - {second} = ?\nThink: Start with {first}, then count down {second}!";
            case "*":
                return $"Hint: {first} × {second} = ?\nThink: {first} groups of {second}!";
            case "/":
                return $"Hint: {first} ÷ {second} = ?\nThink: How many {second}s make {first}?";
            default:
                return "Hint: Read the equation carefully and think about what operation to use!";
        }
    }
    
    void ShowHintMessage(string hint)
    {
        // You can implement this to show hints in the UI
        Debug.Log($"Showing hint: {hint}");
        
        // For now, just log it. You can add UI elements later
        if (uiManager != null)
        {
            // uiManager.ShowHint(hint);
        }
    }
    
    // Simple scene navigation methods
    public void OnBackToMainMenuClicked()
    {
        Debug.Log("Back to main menu clicked from practice game");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Load main menu scene directly
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    // Context menu for testing
    [ContextMenu("Test Tutorial System")]
    void TestTutorialSystem()
    {
        Debug.Log("=== TESTING TUTORIAL SYSTEM ===");
        
        // Reset tutorial
        showTutorial = true;
        currentTutorialStep = 0;
        
        // Show first step
        ShowTutorialStep(0);
        
        Debug.Log("Tutorial system test started - check the tutorial panel!");
    }
    
    [ContextMenu("Complete Tutorial")]
    void TestCompleteTutorial()
    {
        Debug.Log("=== COMPLETING TUTORIAL ===");
        CompleteTutorial();
        Debug.Log("Tutorial completed - check that tutorial panel is hidden!");
    }
    
    [ContextMenu("Show Hint")]
    void TestShowHint()
    {
        Debug.Log("=== TESTING HINT SYSTEM ===");
        ShowHint();
        Debug.Log("Hint should be displayed in console!");
    }
    
    // Auto-assignment context menus
    [ContextMenu("🔧 Auto-Assign All References")]
    void AutoAssignAllReferences()
    {
        Debug.Log("=== AUTO-ASSIGNING ALL REFERENCES ===");
        
        // Assign practice components
        AutoAssignPracticeComponents();
        
        // Assign tutorial UI components
        AutoAssignTutorialUI();
        
        Debug.Log("✅ All references auto-assigned! Check the Inspector.");
    }
    
    [ContextMenu("🔧 Auto-Assign Practice Components")]
    void AutoAssignPracticeComponents()
    {
        Debug.Log("=== AUTO-ASSIGNING PRACTICE COMPONENTS ===");
        
        // Find and assign PlayerController
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                Debug.Log("✅ PlayerController assigned: " + playerController.name);
            else
                Debug.LogWarning("❌ PlayerController not found in scene!");
        }
        
        // Find and assign AnswerVerifier
        if (answerVerifier == null)
        {
            answerVerifier = FindObjectOfType<AnswerVerifier>();
            if (answerVerifier != null)
                Debug.Log("✅ AnswerVerifier assigned: " + answerVerifier.name);
            else
                Debug.LogWarning("❌ AnswerVerifier not found in scene!");
        }
        
        // Find and assign UIManager
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
                Debug.Log("✅ UIManager assigned: " + uiManager.name);
            else
                Debug.LogWarning("❌ UIManager not found in scene!");
        }
        
        // Find and assign GameManager
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
                Debug.Log("✅ GameManager assigned: " + gameManager.name);
            else
                Debug.LogWarning("❌ GameManager not found in scene!");
        }
        
        // Find and assign AudioManager
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
                Debug.Log("✅ AudioManager assigned: " + audioManager.name);
            else
                Debug.LogWarning("❌ AudioManager not found in scene!");
        }
        
        Debug.Log("=== PRACTICE COMPONENTS ASSIGNMENT COMPLETE ===");
    }
    
    [ContextMenu("🔧 Auto-Assign Tutorial UI")]
    void AutoAssignTutorialUI()
    {
        Debug.Log("=== AUTO-ASSIGNING TUTORIAL UI COMPONENTS ===");
        
        // Find Tutorial Panel in UI Canvas
        if (tutorialPanel == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                Transform tutorialPanelTransform = canvas.transform.Find("Tutorial Panel");
                if (tutorialPanelTransform != null)
                {
                    tutorialPanel = tutorialPanelTransform.gameObject;
                    Debug.Log("✅ Tutorial Panel found in canvas: " + canvas.name);
                    break;
                }
            }
            
            if (tutorialPanel == null)
                Debug.LogWarning("❌ Tutorial Panel not found! Make sure it's named 'Tutorial Panel'");
        }
        
        // Find Tutorial Text
        if (tutorialText == null && tutorialPanel != null)
        {
            TextMeshProUGUI[] texts = tutorialPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.ToLower().Contains("text") || text.name.ToLower().Contains("description"))
                {
                    tutorialText = text;
                    Debug.Log("✅ Tutorial Text assigned: " + text.name);
                    break;
                }
            }
            
            if (tutorialText == null)
                Debug.LogWarning("❌ Tutorial Text not found! Add a TextMeshPro component to Tutorial Panel");
        }
        
        // Find Tutorial Buttons
        if (tutorialNextButton == null && tutorialPanel != null)
        {
            Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.name.ToLower().Contains("next"))
                {
                    tutorialNextButton = button;
                    Debug.Log("✅ Next Button assigned: " + button.name);
                }
                else if (button.name.ToLower().Contains("previous"))
                {
                    tutorialPreviousButton = button;
                    Debug.Log("✅ Previous Button assigned: " + button.name);
                }
                else if (button.name.ToLower().Contains("skip"))
                {
                    tutorialSkipButton = button;
                    Debug.Log("✅ Skip Button assigned: " + button.name);
                }
            }
        }
        
        Debug.Log("=== TUTORIAL UI ASSIGNMENT COMPLETE ===");
    }
    
    [ContextMenu("🔍 Check Reference Status")]
    void CheckReferenceStatus()
    {
        Debug.Log("=== REFERENCE STATUS CHECK ===");
        
        // Check Practice Components
        Debug.Log("Practice Components:");
        Debug.Log($"  PlayerController: {(playerController != null ? "✅ " + playerController.name : "❌ Missing")}");
        Debug.Log($"  AnswerVerifier: {(answerVerifier != null ? "✅ " + answerVerifier.name : "❌ Missing")}");
        Debug.Log($"  UIManager: {(uiManager != null ? "✅ " + uiManager.name : "❌ Missing")}");
        Debug.Log($"  GameManager: {(gameManager != null ? "✅ " + gameManager.name : "❌ Missing")}");
        Debug.Log($"  AudioManager: {(audioManager != null ? "✅ " + audioManager.name : "❌ Missing")}");
        
        // Check Tutorial UI
        Debug.Log("Tutorial UI:");
        Debug.Log($"  Tutorial Panel: {(tutorialPanel != null ? "✅ " + tutorialPanel.name : "❌ Missing")}");
        Debug.Log($"  Tutorial Text: {(tutorialText != null ? "✅ " + tutorialText.name : "❌ Missing")}");
        Debug.Log($"  Next Button: {(tutorialNextButton != null ? "✅ " + tutorialNextButton.name : "❌ Missing")}");
        Debug.Log($"  Previous Button: {(tutorialPreviousButton != null ? "✅ " + tutorialPreviousButton.name : "❌ Missing")}");
        Debug.Log($"  Tutorial Skip Button: {(tutorialSkipButton != null ? "✅ " + tutorialSkipButton.name : "❌ Missing")}");
        
        Debug.Log("=== REFERENCE STATUS CHECK COMPLETE ===");
    }
}
