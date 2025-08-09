using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AnswerVerifier : MonoBehaviour
{
    [Header("Equation Settings")]
    public int firstNumber = 7;
    public int secondNumber = 8;
    public int correctAnswer;
    
    [Header("UI References")]
    public UIManager uiManager;
    public TextMeshProUGUI equationText;
    public Button submitButton;
    public TextMeshProUGUI submitButtonText;
    public TextMeshProUGUI resultText;
    public GameObject resultPanel;
    
    [Header("Player Reference")]
    public PlayerController playerController;
    public ARPlayerController arPlayerController; // For AR mode
    
    [Header("Game Settings")]
    public float resultDisplayTime = 3f;
    public Color correctAnswerColor = Color.green;
    public Color wrongAnswerColor = Color.red;
    
    [Header("Reward System")]
    public GameObject housePrefab;
    public GameObject petPrefab;
    public GameObject carPrefab;
    public GameObject treePrefab;
    public Vector3 rewardOffset = new Vector3(0, 3.5f, 0); // Offset from number sprite
    public TextMeshProUGUI correctAnswerCountText;
    
    private bool gameActive = true;
    private bool answerSubmitted = false;
    private int correctAnswersInRow = 0;
    private List<GameObject> spawnedRewards = new List<GameObject>();
    private string currentOperator = "+"; // Track current operator
    
    // Add references to reward buttons
    public Button houseRewardButton;
    public Button petRewardButton;
    public Button carRewardButton;
    public Button treeRewardButton;
    
    public event System.Action<bool> OnAnswerChecked;
    
    private FeedbackDisplay feedbackDisplay;
    
    [ContextMenu("Test AR Reward Sizing")]
    void TestARRewardSizing()
    {
        Debug.Log("=== TESTING AR REWARD SIZING ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! This test is for AR mode only.");
            return;
        }
        
        // Test spawning all reward types to check sizing
        if (housePrefab != null)
        {
            Debug.Log("Testing house reward...");
            SpawnReward(housePrefab, "Test House");
        }
        
        if (petPrefab != null)
        {
            Debug.Log("Testing pet reward...");
            SpawnReward(petPrefab, "Test Pet");
        }
        
        if (carPrefab != null)
        {
            Debug.Log("Testing car reward...");
            SpawnReward(carPrefab, "Test Car");
        }
        
        if (treePrefab != null)
        {
            Debug.Log("Testing tree reward...");
            SpawnReward(treePrefab, "Test Tree");
        }
        
        Debug.Log($"Total rewards spawned: {spawnedRewards.Count}");
        Debug.Log("Check that rewards are small (0.1f scale) and positioned on cube tops!");
    }

    [ContextMenu("Check Reward Status")]
    void CheckRewardStatus()
    {
        Debug.Log("=== REWARD STATUS CHECK ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Mode: {(isARMode ? "AR" : "Mobile")}");
        Debug.Log($"Correct answers in row: {correctAnswersInRow}");
        Debug.Log($"Total rewards spawned: {spawnedRewards.Count}");
        
        for (int i = 0; i < spawnedRewards.Count; i++)
        {
            if (spawnedRewards[i] != null)
            {
                Debug.Log($"Reward {i + 1}: {spawnedRewards[i].name} at {spawnedRewards[i].transform.position}");
            }
            else
            {
                Debug.Log($"Reward {i + 1}: NULL (destroyed)");
            }
        }
        
        Debug.Log("=== REWARD STATUS COMPLETE ===");
    }

    [ContextMenu("Test Reward System")]
    void TestRewardSystem()
    {
        Debug.Log("=== TESTING REWARD SYSTEM ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        // Test spawning a house reward
        if (housePrefab != null)
        {
            Debug.Log("Testing house reward spawn...");
            SpawnReward(housePrefab, "Test House");
        }
        else
        {
            Debug.LogError("House prefab is null!");
        }
        
        Debug.Log($"Total rewards spawned: {spawnedRewards.Count}");
    }
    
    [ContextMenu("Destroy All Rewards")]
    void TestDestroyAllRewards()
    {
        Debug.Log("=== TESTING DESTROY ALL REWARDS ===");
        DestroyAllRewards();
    }

    void Start()
    {
        Debug.Log("=== ANSWER VERIFIER START ===");
        
        // Validate required components
        if (!ValidateComponents())
        {
            Debug.LogError("AnswerVerifier: Required components missing! Check console for details.");
            return;
        }
        
        // Generate initial random equation
        GenerateNewEquation();
        
        // Initialize UI
        UpdateEquationDisplay();
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitAnswer);
            if (submitButtonText != null)
                submitButtonText.text = "Submit Answer";
        }
        
        // Hide result panel initially
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        // Hide result text initially
        if (resultText != null)
            resultText.gameObject.SetActive(false);

        // Get reward buttons from UIManager (set up in editor)
        if (uiManager != null)
        {
            houseRewardButton = uiManager.houseRewardButton;
            petRewardButton = uiManager.petRewardButton;
            carRewardButton = uiManager.carRewardButton;
            treeRewardButton = uiManager.treeRewardButton;
            Debug.Log("Got reward buttons from UIManager");
        }
        else
        {
            Debug.LogWarning("UIManager is null in Start!");
        }
        
        // Set all reward buttons inactive at start
        SetAllRewardButtonsActive(false);
        
        // Add listeners for reward buttons
        AddRewardButtonListeners();

        feedbackDisplay = FindObjectOfType<FeedbackDisplay>();
        if (feedbackDisplay != null) feedbackDisplay.HideAll();
        
        Debug.Log($"AnswerVerifier Start completed - UIManager: {(uiManager != null ? "Found" : "Null")}");
        Debug.Log($"Reward buttons - House: {(houseRewardButton != null ? "Found" : "Null")}, Pet: {(petRewardButton != null ? "Found" : "Null")}, Car: {(carRewardButton != null ? "Found" : "Null")}, Tree: {(treeRewardButton != null ? "Found" : "Null")}");
    }
    
    bool ValidateComponents()
    {
        bool isValid = true;
        
        if (playerController == null)
        {
            Debug.LogError("AnswerVerifier: PlayerController reference is missing!");
            isValid = false;
        }
        
        if (submitButton == null)
        {
            Debug.LogError("AnswerVerifier: Submit button reference is missing!");
            isValid = false;
        }
        
        if (equationText == null)
        {
            Debug.LogError("AnswerVerifier: Equation text reference is missing!");
            isValid = false;
        }
        
        return isValid;
    }
    
    void Update()
    {
        if (gameActive && !answerSubmitted)
        {
            UpdateEquationDisplay();
        }
    }
    
    public void UpdateEquationDisplay()
    {
        int currentNumber = GetCurrentPlayerNumber();
        string equation = $"{firstNumber} {currentOperator} {secondNumber} = {currentNumber}";
        
        // Update equation text directly
        if (equationText != null)
        {
            equationText.text = equation;
        }
        
        // Update through UI Manager if available
        if (uiManager != null)
        {
            uiManager.UpdateEquationDisplay(equation);
        }
    }
    
    int GetCurrentPlayerNumber()
    {
        // Check AR player first, then mobile player
        if (arPlayerController != null)
        {
            return arPlayerController.GetCurrentNumber();
        }
        else if (playerController != null)
        {
            return playerController.currentNumber;
        }
        return 0;
    }
    
    public void SubmitAnswer()
    {
        Debug.Log("=== SUBMIT ANSWER CALLED ===");
        Debug.Log($"answerSubmitted: {answerSubmitted}, gameActive: {gameActive}");
        
        if (answerSubmitted || !gameActive) 
        {
            Debug.Log("SubmitAnswer blocked - answerSubmitted or !gameActive");
            return;
        }
        
        answerSubmitted = true;
        submitButton.interactable = false;
        
        // Check if answer is correct
        int currentNumber = GetCurrentPlayerNumber();
        bool isCorrect = (currentNumber == correctAnswer);
        
        Debug.Log($"SubmitAnswer - Player position: {currentNumber}, Correct answer: {correctAnswer}, IsCorrect: {isCorrect}");
        Debug.Log($"Current equation: {firstNumber} {currentOperator} {secondNumber} = {correctAnswer}");

        // Feedback logic (show immediately)
        if (feedbackDisplay != null)
        {
            if (isCorrect)
            {
                feedbackDisplay.ShowTick();
                correctAnswersInRow++;
                HandleRewardSystem();
                
                // Play audio feedback
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCorrectAnswerSound();
                }
                
                // Update statistics
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.gameData.UpdateStatistics(true);
                }
                
                if (correctAnswersInRow >= 4)
                {
                    feedbackDisplay.ShowGameWon();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayVictorySound();
                    }
                }
            }
            else
            {
                feedbackDisplay.ShowCross();
                correctAnswersInRow = 0;
                SetAllRewardButtonsActive(false); // Hide all reward buttons on wrong
                
                // Play audio feedback
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWrongAnswerSound();
                }
                
                // Update statistics
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.gameData.UpdateStatistics(false);
                }
            }
        }

        // Show result
        Debug.Log("Calling ShowResult...");
        ShowResult(isCorrect);
        
        // Start coroutine to handle next steps
        Debug.Log("Starting HandleResult coroutine...");
        StartCoroutine(HandleResult(isCorrect));
    }
    
    void ShowResult(bool isCorrect)
    {
        Debug.Log($"=== SHOW RESULT CALLED - isCorrect: {isCorrect} ===");
        
        string message;
        
        if (isCorrect)
        {
            message = "Correct! 🎉";
        }
        else
        {
            message = $"Wrong! The answer was {correctAnswer}";
        }
        
        Debug.Log($"Result message: {message}");
        
        // Show result through UI Manager if available
        if (uiManager != null)
        {
            Debug.Log("Using UIManager to show result");
            uiManager.ShowResult(isCorrect, message);
        }
        else
        {
            Debug.Log("UIManager is null, using fallback UI updates");
            // Fallback to direct UI updates
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = message;
                resultText.color = isCorrect ? correctAnswerColor : wrongAnswerColor;
                Debug.Log($"Set resultText: {message}");
            }
            else
            {
                Debug.LogWarning("resultText is null!");
            }
            
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
                Debug.Log("Activated resultPanel");
            }
            else
            {
                Debug.LogWarning("resultPanel is null!");
            }
        }
        // Notify AIAssistant
        OnAnswerChecked?.Invoke(isCorrect);
    }
    
    IEnumerator HandleResult(bool isCorrect)
    {
        Debug.Log($"=== HANDLE RESULT STARTED - isCorrect: {isCorrect} ===");
        
        // Wait for result display time
        Debug.Log($"Waiting {resultDisplayTime} seconds for result display...");
        yield return new WaitForSeconds(resultDisplayTime);
        
        Debug.Log("Result display time finished, hiding result UI...");
        
        // Hide result UI through UI Manager if available
        if (uiManager != null)
        {
            Debug.Log("Using UIManager to hide result");
            uiManager.HideResult();
        }
        else
        {
            Debug.Log("UIManager is null, using fallback to hide result");
            // Fallback to direct UI updates
            if (resultText != null)
                resultText.gameObject.SetActive(false);
                
            if (resultPanel != null)
                resultPanel.SetActive(false);
        }
        
        Debug.Log("Result UI hidden, processing next steps...");
        
        // Reset for next question or end game
        if (isCorrect)
        {
            Debug.Log("Answer was correct, handling reward system...");
            // Handle reward system - this will show the reward button
            HandleRewardSystem();
            
            // DON'T generate new equation here - wait for player to click reward or move
            // The new equation will be generated when:
            // 1. Player clicks reward button (in OnHouseRewardButtonClicked, etc.)
            // 2. Player moves left/right (in SkipCurrentReward)
            
            // Reset game state for next input
            answerSubmitted = false;
            submitButton.interactable = true;
            gameActive = true;
            Debug.Log("Game state reset for next input");
        }
        else
        {
            Debug.Log("Answer was wrong, resetting for retry...");
            // Reset correct answers in row on wrong answer
            correctAnswersInRow = 0;
            UpdateCorrectAnswerCount();
            
            // Destroy all rewards on wrong answer
            DestroyAllRewards();
            
            // Game over or retry logic
            ResetForRetry();
        }
    }
    
    public void GenerateNewEquation()
    {
        // Randomly choose between addition and subtraction
        bool isAddition = Random.Range(0, 2) == 0; // 50% chance for each
        
        if (isAddition)
        {
            currentOperator = "+";
            // Generate addition equation
            firstNumber = Random.Range(1, 21); // 1 to 20
            secondNumber = Random.Range(1, 21); // 1 to 20
            correctAnswer = firstNumber + secondNumber;
            
            // Ensure answer is within -20 to 20 range
            if (correctAnswer > 20)
            {
                // If sum is too large, reduce the numbers
                int maxFirst = Mathf.Min(firstNumber, 20);
                int maxSecond = 20 - maxFirst;
                firstNumber = Random.Range(1, maxFirst + 1);
                secondNumber = Random.Range(1, maxSecond + 1);
                correctAnswer = firstNumber + secondNumber;
            }
        }
        else
        {
            currentOperator = "-";
            // Generate subtraction equation
            firstNumber = Random.Range(1, 21); // 1 to 20
            secondNumber = Random.Range(1, 21); // 1 to 20
            correctAnswer = firstNumber - secondNumber;
            
            // Ensure answer is within -20 to 20 range
            if (correctAnswer < -20)
            {
                // If difference is too negative, adjust the numbers
                firstNumber = Random.Range(1, 21);
                secondNumber = Random.Range(firstNumber, Mathf.Min(firstNumber + 20, 21));
                correctAnswer = firstNumber - secondNumber;
            }
        }
        
        // Reset game state
        answerSubmitted = false;
        submitButton.interactable = true;
        gameActive = true;
        
        // Update display
        UpdateEquationDisplay();
        // Notify AIAssistant
        var ai = FindObjectOfType<AIAssistant>();
        if (ai != null) ai.OnNewEquation();

        if (feedbackDisplay != null) feedbackDisplay.HideAll();
    }
    
    void ResetForRetry()
    {
        // Reset for same equation retry
        answerSubmitted = false;
        submitButton.interactable = true;
        gameActive = true;
        
        // Update display
        UpdateEquationDisplay();
    }
    
    // Public method to check if game is active
    public bool IsGameActive()
    {
        return gameActive && !answerSubmitted;
    }
    
    // Public method to get current equation info
    public (int first, int second, int correct) GetCurrentEquation()
    {
        return (firstNumber, secondNumber, correctAnswer);
    }
    
    void HandleRewardSystem()
    {
        UpdateCorrectAnswerCount();
        Debug.Log($"=== HANDLE REWARD SYSTEM CALLED - Correct answers in row: {correctAnswersInRow} ===");
        // Activate the appropriate reward button
        SetAllRewardButtonsActive(false);
        switch (correctAnswersInRow)
        {
            case 1:
                if (houseRewardButton != null) houseRewardButton.gameObject.SetActive(true);
                break;
            case 2:
                if (petRewardButton != null) petRewardButton.gameObject.SetActive(true);
                break;
            case 3:
                if (carRewardButton != null) carRewardButton.gameObject.SetActive(true);
                break;
            case 4:
                if (treeRewardButton != null) treeRewardButton.gameObject.SetActive(true);
                break;
        }
    }
    
    // Method to skip current reward and generate new equation
    public void SkipCurrentReward()
    {
        Debug.Log("Skipping current reward and generating new equation");
        
        // Hide all reward buttons
        SetAllRewardButtonsActive(false);
        
        // Generate new equation
        GenerateNewEquation();
    }
    
    void SpawnReward(GameObject prefab, string rewardName)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {rewardName}");
            return;
        }
        
        // Find the number sprite for the correct answer
        GameObject targetNumber = GameObject.Find(correctAnswer.ToString());
        if (targetNumber == null)
        {
            Debug.LogWarning($"Could not find target number {correctAnswer}");
            return;
        }
        
        // Check if we're in AR mode
        bool isARMode = (arPlayerController != null);
        
        // Spawn reward as child of the number sprite with offset
        Vector3 spawnPosition = targetNumber.transform.position + rewardOffset;
        GameObject reward = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Parent the reward to the number sprite
        reward.transform.SetParent(targetNumber.transform);
        
        if (isARMode)
        {
            // AR Mode: Make rewards much smaller and position on the cube
            reward.transform.localScale = Vector3.one * 0.1f; // Much smaller - cube-sized
            reward.transform.localPosition = new Vector3(0, 0.15f, 0); // Exactly on top of cube
            Debug.Log($"Spawned {rewardName} in AR mode - small size (0.1f), on cube top");
        }
        else
        {
            // Mobile Mode: Keep original size and positioning
            reward.transform.localPosition = rewardOffset;
            Debug.Log($"Spawned {rewardName} in mobile mode - original size and position");
        }
        
        spawnedRewards.Add(reward);
        
        Debug.Log($"Spawned {rewardName} as child of {targetNumber.name}");
    }
    
    // Method to destroy all rewards (called on wrong answer)
    void DestroyAllRewards()
    {
        Debug.Log($"Destroying all {spawnedRewards.Count} rewards due to wrong answer");
        
        foreach (GameObject reward in spawnedRewards)
        {
            if (reward != null)
            {
                DestroyImmediate(reward);
            }
        }
        
        spawnedRewards.Clear();
        correctAnswersInRow = 0;
        UpdateCorrectAnswerCount();
        
        Debug.Log("All rewards destroyed and progress reset");
    }
    
    void UpdateCorrectAnswerCount()
    {
        if (correctAnswerCountText != null)
        {
            correctAnswerCountText.text = $"Correct: {correctAnswersInRow}";
        }
    }

    // Reward button handlers
    void OnHouseRewardButtonClicked()
    {
        SpawnReward(housePrefab, "House");
        if (houseRewardButton != null) houseRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnPetRewardButtonClicked()
    {
        SpawnReward(petPrefab, "Pet");
        if (petRewardButton != null) petRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnCarRewardButtonClicked()
    {
        SpawnReward(carPrefab, "Car");
        if (carRewardButton != null) carRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnTreeRewardButtonClicked()
    {
        SpawnReward(treePrefab, "Tree");
        if (treeRewardButton != null) treeRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }

    void SetAllRewardButtonsActive(bool active)
    {
        if (houseRewardButton != null) houseRewardButton.gameObject.SetActive(active);
        if (petRewardButton != null) petRewardButton.gameObject.SetActive(active);
        if (carRewardButton != null) carRewardButton.gameObject.SetActive(active);
        if (treeRewardButton != null) treeRewardButton.gameObject.SetActive(active);
    }
    
    void AddRewardButtonListeners()
    {
        if (houseRewardButton != null) 
        {
            houseRewardButton.onClick.RemoveAllListeners();
            houseRewardButton.onClick.AddListener(OnHouseRewardButtonClicked);
            Debug.Log("Added listener to House Button");
        }
        if (petRewardButton != null) 
        {
            petRewardButton.onClick.RemoveAllListeners();
            petRewardButton.onClick.AddListener(OnPetRewardButtonClicked);
            Debug.Log("Added listener to Pet Button");
        }
        if (carRewardButton != null) 
        {
            carRewardButton.onClick.RemoveAllListeners();
            carRewardButton.onClick.AddListener(OnCarRewardButtonClicked);
            Debug.Log("Added listener to Car Button");
        }
        if (treeRewardButton != null) 
        {
            treeRewardButton.onClick.RemoveAllListeners();
            treeRewardButton.onClick.AddListener(OnTreeRewardButtonClicked);
            Debug.Log("Added listener to Tree Button");
        }
    }

    public string GetOperator() { return currentOperator; }
    
    public int GetCorrectAnswersInRow()
    {
        return correctAnswersInRow;
    }
} 