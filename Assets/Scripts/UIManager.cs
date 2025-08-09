using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Equation Display")]
    public TextMeshProUGUI equationText;
    public GameObject equationPanel;
    
    [Header("Submit Button")]
    public Button submitButton;
    public TextMeshProUGUI submitButtonText;
    public Image submitButtonImage;
    
    [Header("Result Display")]
    public TextMeshProUGUI resultText;
    public GameObject resultPanel;
    public Image resultPanelBackground;
    public GameObject correctResultImage;
    public GameObject wrongResultImage;
    
    [Header("Game Flow")]
    public Button retryButton;
    public Button nextButton;
    
    [Header("Colors")]
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color wrongColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color neutralColor = new Color(0.2f, 0.2f, 0.8f, 1f);
    
    [Header("Reward Buttons")]
    public Button houseRewardButton;
    public Button petRewardButton;
    public Button carRewardButton;
    public Button treeRewardButton;
    
    private AnswerVerifier answerVerifier;
    // Remove TypewriterEffect reference
    
    void Start()
    {
        answerVerifier = FindObjectOfType<AnswerVerifier>();
        
        // Setup button listeners
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
            retryButton.gameObject.SetActive(false);
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
            nextButton.gameObject.SetActive(false);
        }
        
        // Initialize UI
        InitializeUI();
        
        // Subscribe to GameManager events if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
    }
    
    void HandleGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.Playing:
                EnableGameplayUI();
                break;
            case GameManager.GameState.Paused:
                ShowPauseUI();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOverUI();
                break;
            case GameManager.GameState.Victory:
                ShowVictoryUI();
                break;
        }
    }
    
    void HandleScoreChanged(int newScore)
    {
        // Update score display if available
        Debug.Log($"Score updated: {newScore}");
    }
    
    void EnableGameplayUI()
    {
        if (submitButton != null) submitButton.interactable = true;
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
    }
    
    void ShowPauseUI()
    {
        if (submitButton != null) submitButton.interactable = false;
    }
    
    void ShowGameOverUI()
    {
        if (submitButton != null) submitButton.interactable = false;
        // Add game over specific UI elements
    }
    
    void ShowVictoryUI()
    {
        if (submitButton != null) submitButton.interactable = false;
        // Add victory specific UI elements
    }
    
    void InitializeUI()
    {
        // Show equation panel, hide result panel
        if (equationPanel != null)
            equationPanel.SetActive(true);
            
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            Debug.Log("Result panel hidden initially");
        }
        else
        {
            Debug.LogWarning("Result panel is null in InitializeUI!");
        }
            
        if (resultText != null)
            resultText.gameObject.SetActive(false);
            
        // Enable submit button
        if (submitButton != null)
            submitButton.interactable = true;
    }
    
    public void UpdateEquationDisplay(string equation)
    {
        if (equationText != null)
        {
            equationText.text = equation;
        }
    }
    
    public void ShowResult(bool isCorrect, string message)
    {
        Debug.Log($"ShowResult called - isCorrect: {isCorrect}, message: {message}");
        
        // Show result text if available
        if (resultText != null)
        {
            resultText.text = message;
            resultText.color = isCorrect ? correctColor : wrongColor;
            resultText.gameObject.SetActive(true);
            Debug.Log("Result text activated");
        }
        
        // Show result panel - Force activation
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            Debug.Log("Result panel activated");
            
            // Force activate all children of result panel
            Transform[] children = resultPanel.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != resultPanel.transform)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"Activated child: {child.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Result panel is null!");
        }
        
        // Show appropriate result image
        if (isCorrect)
        {
            if (correctResultImage != null)
            {
                correctResultImage.SetActive(true);
                Debug.Log("Correct result image activated");
            }
            else
            {
                Debug.LogWarning("Correct result image is null!");
            }
            
            if (wrongResultImage != null)
                wrongResultImage.SetActive(false);
        }
        else
        {
            if (wrongResultImage != null)
            {
                wrongResultImage.SetActive(true);
                Debug.Log("Wrong result image activated");
            }
            else
            {
                Debug.LogWarning("Wrong result image is null!");
            }
            
            if (correctResultImage != null)
                correctResultImage.SetActive(false);
        }
        
        // Show appropriate buttons
        if (isCorrect)
        {
            if (nextButton != null)
                nextButton.gameObject.SetActive(true);
            if (retryButton != null)
                retryButton.gameObject.SetActive(false);
        }
        else
        {
            if (retryButton != null)
                retryButton.gameObject.SetActive(true);
            if (nextButton != null)
                nextButton.gameObject.SetActive(false);
        }
        
        // Disable submit button
        if (submitButton != null)
            submitButton.interactable = false;
    }
    
    public void HideResult()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);
            
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        // Hide result images
        if (correctResultImage != null)
            correctResultImage.SetActive(false);
            
        if (wrongResultImage != null)
            wrongResultImage.SetActive(false);
            
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
            
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
            
        // Re-enable submit button
        if (submitButton != null)
            submitButton.interactable = true;
    }
    
    void OnSubmitClicked()
    {
        if (answerVerifier != null)
        {
            answerVerifier.SubmitAnswer();
        }
    }
    
    public void OnRetryClicked()
    {
        if (answerVerifier != null)
        {
            // Reset for retry
            HideResult();
        }
    }
    
    public void OnNextClicked()
    {
        if (answerVerifier != null)
        {
            // Generate new equation
            HideResult();
            // The AnswerVerifier will handle generating new equation
        }
    }
    

    
    public void SetSubmitButtonEnabled(bool enabled)
    {
        if (submitButton != null)
            submitButton.interactable = enabled;
    }
    
    public void UpdateSubmitButtonText(string text)
    {
        if (submitButtonText != null)
            submitButtonText.text = text;
    }
    
    [ContextMenu("Force Activate Result Panel")]
    public void ForceActivateResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            Debug.Log("Result panel force activated");
            
            // Also activate the images
            if (correctResultImage != null)
                correctResultImage.SetActive(true);
            if (wrongResultImage != null)
                wrongResultImage.SetActive(true);
        }
        else
        {
            Debug.LogError("Result panel is null!");
        }
    }
    
    [ContextMenu("Check Result Panel State")]
    public void CheckResultPanelState()
    {
        Debug.Log("=== RESULT PANEL STATE CHECK ===");
        
        if (resultPanel != null)
        {
            Debug.Log($"Result Panel: {resultPanel.name} - Active: {resultPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Result Panel is null!");
        }
        
        if (correctResultImage != null)
        {
            Debug.Log($"Correct Result Image: {correctResultImage.name} - Active: {correctResultImage.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Correct Result Image is null!");
        }
        
        if (wrongResultImage != null)
        {
            Debug.Log($"Wrong Result Image: {wrongResultImage.name} - Active: {wrongResultImage.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Wrong Result Image is null!");
        }
    }
} 