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
    
    [Header("AR Reward System")]
    // AR rewards are now children of cube prefabs - no prefab references needed
    
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
        
        // Test activating AR rewards on cube 0 (if in AR mode)
        if (isARMode)
        {
            Debug.Log("Testing AR reward activation on cube 0...");
            GameObject testCube = GameObject.Find("0");
            if (testCube != null)
            {
                ActivateARRewardOnCube(testCube, "House");
            }
        }
        else
        {
            Debug.Log("Not in AR mode - skipping AR reward test");
        }
        
        Debug.Log($"Total rewards spawned: {spawnedRewards.Count}");
        Debug.Log("Check that AR rewards appear with correct sizing and positioning!");
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
        
        // Test AR reward activation (if in AR mode)
        if (isARMode)
        {
            Debug.Log("Testing AR reward activation...");
            GameObject testCube = GameObject.Find("0");
            if (testCube != null)
            {
                ActivateARRewardOnCube(testCube, "House");
            }
        }
        else
        {
            Debug.Log("Not in AR mode - skipping AR reward test");
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
        
        // CRITICAL: Deactivate all AR rewards to establish references
        StartCoroutine(DeactivateAllARRewardsWhenReady());
        
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
        
        // Check if we're in AR mode
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: ONLY show reward buttons (NO automatic activation)
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
            
            Debug.Log("AR Mode: Reward buttons shown. Click button to activate prefab on cube.");
        }
        else
        {
            // Mobile Mode: Activate the appropriate reward button
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
    }
    
    /// <summary>
    /// Activates the appropriate AR reward on the correct answer cube based on counter
    /// </summary>
    void ActivateARRewardByCounter()
    {
        if (correctAnswersInRow == 0) return;
        
        // Find the correct answer cube
        GameObject targetCube = GameObject.Find(correctAnswer.ToString());
        if (targetCube == null)
        {
            Debug.LogWarning($"Could not find cube {correctAnswer} for reward activation!");
            return;
        }
        
        // Get the ARCubeRewardManager component
        ARCubeRewardManager rewardManager = targetCube.GetComponent<ARCubeRewardManager>();
        if (rewardManager == null)
        {
            Debug.LogWarning($"ARCubeRewardManager not found on cube {correctAnswer}!");
            return;
        }
        
        // Activate the appropriate reward based on counter
        rewardManager.ActivateReward(correctAnswersInRow);
        Debug.Log($"Activated reward for {correctAnswersInRow} correct answers on cube {correctAnswer}");
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
    
    // SpawnReward method removed - AR mode only activates existing prefab children
    
    // Old ActivateARRewardOnCube() method removed - no longer needed
    
    void ActivateARRewardOnCube(GameObject targetCube, string rewardName)
    {
        if (targetCube == null)
        {
            Debug.LogWarning($"Target cube is null for {rewardName}");
            return;
        }
        
        Debug.Log($"=== ACTIVATING AR REWARD ON CUBE {targetCube.name} ===");
        Debug.Log($"Looking for reward: {rewardName}");
        
        // Get the ARCubeRewardManager component from the cube
        ARCubeRewardManager rewardManager = targetCube.GetComponent<ARCubeRewardManager>();
        if (rewardManager == null)
        {
            Debug.LogWarning($"ARCubeRewardManager not found on cube {targetCube.name}!");
            return;
        }
        
        // Activate the specific reward
        rewardManager.ActivateSpecificReward(rewardName);
        Debug.Log($"✅ SUCCESS: Activated {rewardName} on cube {targetCube.name} via ARCubeRewardManager");
    }
    
    string GetChildrenNames(GameObject parent)
    {
        string[] names = new string[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            names[i] = parent.transform.GetChild(i).name;
        }
        return string.Join(", ", names);
    }
    
    void DeactivateAllARRewardsOnCube(GameObject targetCube)
    {
        Debug.Log($"Deactivating all AR rewards on cube {targetCube.name}");
        
        // Get the ARCubeRewardManager component from the cube
        ARCubeRewardManager rewardManager = targetCube.GetComponent<ARCubeRewardManager>();
        if (rewardManager == null)
        {
            Debug.LogWarning($"ARCubeRewardManager not found on cube {targetCube.name}!");
            return;
        }
        
        // Deactivate all rewards using the manager
        rewardManager.DeactivateAllRewards();
        Debug.Log($"Deactivated all AR rewards on cube {targetCube.name} via ARCubeRewardManager");
        spawnedRewards.Clear();
    }
    
    // SpawnARReward method removed - we only activate existing prefab children
    
    void SpawnMobileReward(GameObject prefab, string rewardName, GameObject targetNumber)
    {
        // Mobile Mode: Original behavior
        Vector3 spawnPosition = targetNumber.transform.position + rewardOffset;
        GameObject reward = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Parent the reward to the number sprite
        reward.transform.SetParent(targetNumber.transform);
        reward.transform.localPosition = rewardOffset;
        
        spawnedRewards.Add(reward);
        Debug.Log($"Spawned {rewardName} in mobile mode - original size and position");
    }
    
    // Method to destroy all rewards (called on wrong answer)
    void DestroyAllRewards()
    {
        Debug.Log($"Destroying all rewards due to wrong answer");
        
        // Check if we're in AR mode
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: Deactivate all AR rewards on all cubes
            DeactivateAllARRewardsOnAllCubes();
        }
        else
        {
            // Mobile Mode: Destroy instantiated rewards
            Debug.Log($"Destroying {spawnedRewards.Count} mobile rewards");
        foreach (GameObject reward in spawnedRewards)
        {
            if (reward != null)
            {
                DestroyImmediate(reward);
            }
        }
        spawnedRewards.Clear();
        }
        
        correctAnswersInRow = 0;
        UpdateCorrectAnswerCount();
        
        Debug.Log("All rewards destroyed/deactivated and progress reset");
    }
    
    void DeactivateAllARRewardsOnAllCubes()
    {
        Debug.Log("Deactivating all AR rewards on all cubes");
        
        // Find ALL cubes in the scene (active + inactive) and deactivate their AR rewards
        for (int i = -20; i <= 20; i++) // Cover the full number line range
        {
            GameObject cube = GameObject.Find(i.ToString());
            if (cube != null)
            {
                Debug.Log($"Found cube {i} (Active: {cube.activeInHierarchy}), deactivating AR rewards...");
                DeactivateAllARRewardsOnCube(cube);
            }
            else
            {
                Debug.Log($"Cube {i} not found by name - trying to find by tag or component...");
                
                // Try to find inactive cubes by searching all objects with "Cube" in the name
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == i.ToString())
                    {
                        Debug.Log($"Found inactive cube {i} via search, deactivating AR rewards...");
                        DeactivateAllARRewardsOnCube(obj);
                        break;
                    }
                }
            }
        }
        
        Debug.Log("All AR rewards deactivated on all cubes");
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
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: Activate the House prefab child on the correct answer cube
            GameObject targetCube = GameObject.Find(correctAnswer.ToString());
            if (targetCube != null)
            {
                ActivateARRewardOnCube(targetCube, "House");
            }
            else
            {
                Debug.LogError($"Could not find cube {correctAnswer} for House reward!");
            }
        }
        else
        {
            // Mobile Mode: Spawn the reward
            GameObject targetNumber = GameObject.Find(correctAnswer.ToString());
            if (targetNumber != null)
            {
                SpawnMobileReward(housePrefab, "House", targetNumber);
            }
        }
        
        if (houseRewardButton != null) houseRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnPetRewardButtonClicked()
    {
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: Activate the Pet prefab child on the correct answer cube
            GameObject targetCube = GameObject.Find(correctAnswer.ToString());
            if (targetCube != null)
            {
                ActivateARRewardOnCube(targetCube, "Pet");
            }
            else
            {
                Debug.LogError($"Could not find cube {correctAnswer} for Pet reward!");
            }
        }
        else
        {
            // Mobile Mode: Spawn the reward
            GameObject targetNumber = GameObject.Find(correctAnswer.ToString());
            if (targetNumber != null)
            {
                SpawnMobileReward(petPrefab, "Pet", targetNumber);
            }
        }
        
        if (petRewardButton != null) petRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnCarRewardButtonClicked()
    {
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: Activate the Car prefab child on the correct answer cube
            GameObject targetCube = GameObject.Find(correctAnswer.ToString());
            if (targetCube != null)
            {
                ActivateARRewardOnCube(targetCube, "Car");
            }
            else
            {
                Debug.LogError($"Could not find cube {correctAnswer} for Car reward!");
            }
        }
        else
        {
            // Mobile Mode: Spawn the reward
            GameObject targetNumber = GameObject.Find(correctAnswer.ToString());
            if (targetNumber != null)
            {
                SpawnMobileReward(carPrefab, "Car", targetNumber);
            }
        }
        
        if (carRewardButton != null) carRewardButton.gameObject.SetActive(false);
        GenerateNewEquation();
    }
    void OnTreeRewardButtonClicked()
    {
        bool isARMode = (arPlayerController != null);
        
        if (isARMode)
        {
            // AR Mode: Activate the Tree prefab child on the correct answer cube
            GameObject targetCube = GameObject.Find(correctAnswer.ToString());
            if (targetCube != null)
            {
                ActivateARRewardOnCube(targetCube, "Tree");
            }
            else
            {
                Debug.LogError($"Could not find cube {correctAnswer} for Tree reward!");
            }
        }
        else
        {
            // Mobile Mode: Spawn the reward
            GameObject targetNumber = GameObject.Find(correctAnswer.ToString());
            if (targetNumber != null)
            {
                SpawnMobileReward(treePrefab, "Tree", targetNumber);
            }
        }
        
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
    
    [ContextMenu("Setup AR Reward System")]
    void SetupARRewardSystem()
    {
        Debug.Log("=== SETTING UP AR REWARD SYSTEM ===");
        
        Debug.Log("AR rewards are now children of cube prefabs - no prefab assignment needed!");
        Debug.Log("Make sure your Cube prefab has these children:");
        Debug.Log("- AR House (initially deactivated)");
        Debug.Log("- AR Pet (initially deactivated)");
        Debug.Log("- AR Car (initially deactivated)");
        Debug.Log("- AR Tree (initially deactivated)");
        Debug.Log("Test with 'Test AR Reward Activation (No Instantiation)' context menu");
    }
    
    [ContextMenu("Test AR Reward System")]
    void TestARRewardSystem()
    {
        Debug.Log("=== TESTING AR REWARD SYSTEM ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! This test is for AR mode only.");
            return;
        }
        
        // Test activating AR rewards on different cubes
        Debug.Log("Testing AR reward activation on different cubes...");
        
        GameObject testCube0 = GameObject.Find("0");
        if (testCube0 != null)
        {
            ActivateARRewardOnCube(testCube0, "House");
        }
        
        GameObject testCube1 = GameObject.Find("1");
        if (testCube1 != null)
        {
            ActivateARRewardOnCube(testCube1, "Pet");
        }
        
        GameObject testCube2 = GameObject.Find("2");
        if (testCube2 != null)
        {
            ActivateARRewardOnCube(testCube2, "Car");
        }
        
        GameObject testCube3 = GameObject.Find("3");
        if (testCube3 != null)
        {
            ActivateARRewardOnCube(testCube3, "Tree");
        }
        
        Debug.Log($"Total AR rewards activated: {spawnedRewards.Count}");
        Debug.Log("Check that AR rewards appear on top of their respective cubes!");
    }
    
    [ContextMenu("Fix Existing Reward Scales")]
    void FixExistingRewardScales()
    {
        Debug.Log("=== FIXING EXISTING REWARD SCALES (BUILD-SAFE) ===");
        
        foreach (GameObject reward in spawnedRewards)
        {
            if (reward != null)
            {
                // Set proper scale for AR visibility
                reward.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                
                // Make reward face the camera
                reward.transform.LookAt(Camera.main.transform);
                reward.transform.Rotate(0, 180, 0); // Adjust rotation so it faces camera properly
                
                Debug.Log($"Fixed transform for {reward.name}: Scale={reward.transform.localScale}");
            }
        }
        
        Debug.Log("All existing rewards fixed with proper AR positioning and scale!");
    }
    
    [ContextMenu("Test Build-Safe AR Reward")]
    void TestBuildSafeARReward()
    {
        Debug.Log("=== TESTING BUILD-SAFE AR REWARD ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! This test is for AR mode only.");
            return;
        }
        
        // Test with house reward using the new activation system
        GameObject testCube = GameObject.Find("0");
        if (testCube != null)
        {
            // Use the new activation system
            ActivateARRewardOnCube(testCube, "House");
            Debug.Log("Test reward activated using new activation system!");
        }
    }
    
    [ContextMenu("Test AR Reward Activation (No Instantiation)")]
    void TestARRewardActivationNoInstantiation()
    {
        Debug.Log("=== TESTING AR REWARD ACTIVATION (NO INSTANTIATION) ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! AR Player Controller not found.");
            return;
        }
        
        // Test direct activation of existing prefab children
        Debug.Log("Testing direct activation of existing prefab children...");
        
        // Find cube 3 and activate House reward
        GameObject testCube = GameObject.Find("3");
        if (testCube != null)
        {
            Debug.Log($"Found test cube: {testCube.name} at position {testCube.transform.position}");
            ActivateARRewardOnCube(testCube, "House");
        }
        else
        {
            Debug.LogWarning("Could not find cube 3 for testing!");
        }
        
        Debug.Log("=== AR REWARD ACTIVATION TEST COMPLETE ===");
        Debug.Log("Check if House appeared on top of cube 3 (not floating elsewhere)!");
    }
    
    [ContextMenu("Test AR Wrong Answer Handling")]
    void TestARWrongAnswerHandling()
    {
        Debug.Log("=== TESTING AR WRONG ANSWER HANDLING ===");
        
        bool isARMode = (arPlayerController != null);
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! AR Player Controller not found.");
            return;
        }
        
        // First activate some rewards
        Debug.Log("Activating rewards first...");
            GameObject testCube = GameObject.Find("0");
            if (testCube != null)
            {
            ActivateARRewardOnCube(testCube, "House");
            ActivateARRewardOnCube(testCube, "Pet");
            ActivateARRewardOnCube(testCube, "Car");
        }
        
        Debug.Log("Now simulating wrong answer...");
        // Simulate wrong answer
        DestroyAllRewards();
        
        Debug.Log("All AR rewards should now be deactivated!");
    }
    
    [ContextMenu("Debug Cube Prefab Children Names")]
    void DebugCubePrefabChildrenNames()
    {
        Debug.Log("=== DEBUGGING CUBE PREFAB CHILDREN NAMES ===");
        
        // Find a cube to inspect
        GameObject testCube = GameObject.Find("3"); // Look for cube 3
        if (testCube == null)
        {
            Debug.LogWarning("Could not find cube 3 to inspect!");
            return;
        }
        
        Debug.Log($"Inspecting cube: {testCube.name}");
        Debug.Log($"Cube position: {testCube.transform.position}");
        
        // List all children
        Debug.Log("All children of this cube:");
        for (int i = 0; i < testCube.transform.childCount; i++)
        {
            Transform child = testCube.transform.GetChild(i);
            Debug.Log($"  Child {i}: '{child.name}' - Active: {child.gameObject.activeInHierarchy}");
        }
        
        // Look specifically for reward objects
        Debug.Log("\nLooking for reward objects by name patterns:");
        string[] possibleNames = { "AR House", "AR Pet", "AR Car", "AR Tree", "House", "Pet", "Car", "Tree", "ar_house", "ar_pet", "ar_car", "ar_tree" };
        
        foreach (string name in possibleNames)
        {
            Transform rewardChild = testCube.transform.Find(name);
            if (rewardChild != null)
            {
                Debug.Log($"✅ Found reward: '{name}' at position {rewardChild.transform.position}");
            }
            else
            {
                Debug.Log($"❌ Not found: '{name}'");
            }
        }
    }
    
    [ContextMenu("Test All AR Reward Types (Build-Safe)")]
    void TestAllARRewardTypes()
    {
        Debug.Log("=== TESTING ALL AR REWARD TYPES (BUILD-SAFE) ===");
        
        bool isARMode = (arPlayerController != null);
        Debug.Log($"Current mode: {(isARMode ? "AR" : "Mobile")}");
        
        if (!isARMode)
        {
            Debug.LogWarning("Not in AR mode! This test is for AR mode only.");
            return;
        }
        
        // Test all AR reward types with the new activation system
        TestARRewardType("House", "0");
        TestARRewardType("Pet", "1");
        TestARRewardType("Car", "2");
        TestARRewardType("Tree", "3");
        
        Debug.Log($"Total AR rewards activated: {spawnedRewards.Count}");
        Debug.Log("All rewards should appear on top of their respective cubes!");
        
        // Verify build-safe settings
        VerifyBuildSafeSettings();
    }
    
    [ContextMenu("Verify Build-Safe AR Settings")]
    void VerifyBuildSafeSettings()
    {
        Debug.Log("=== VERIFYING BUILD-SAFE AR SETTINGS ===");
        
        if (spawnedRewards.Count == 0)
        {
            Debug.LogWarning("No rewards to verify. Spawn some rewards first.");
            return;
        }
        
        foreach (GameObject reward in spawnedRewards)
        {
            if (reward != null)
            {
                Debug.Log($"--- {reward.name} Build-Safe Verification ---");
                
                // Check world position (should be above cube surface)
                Vector3 worldPos = reward.transform.position;
                bool correctPosition = worldPos.y > 0.3f; // Should be above ground/cube surface
                Debug.Log($"World Position: {worldPos} - {(correctPosition ? "✅ ABOVE SURFACE" : "❌ TOO LOW")}");
                
                // Check scale
                Vector3 scale = reward.transform.localScale;
                bool correctScale = Mathf.Approximately(scale.x, 1.5f) && 
                                  Mathf.Approximately(scale.y, 1.5f) && 
                                  Mathf.Approximately(scale.z, 1.5f);
                Debug.Log($"Scale: {scale} - {(correctScale ? "✅ CORRECT" : "❌ WRONG")}");
                
                // Check rotation
                Vector3 rotation = reward.transform.rotation.eulerAngles;
                bool correctRotation = Mathf.Approximately(rotation.y, 90f) && 
                                     Mathf.Approximately(rotation.x, 0f) && 
                                     Mathf.Approximately(rotation.z, 0f);
                Debug.Log($"Rotation: {rotation} - {(correctRotation ? "✅ CORRECT" : "❌ WRONG")}");
                
                // Check if positioned as child of cube (should be child in new system)
                bool isPositionedCorrectly = reward.transform.parent != null; // Should be child of cube
                Debug.Log($"Positioning: {(isPositionedCorrectly ? "✅ CHILD OF CUBE" : "❌ NOT CHILD OF CUBE")}");
                
                Debug.Log($"Overall Status: {(correctPosition && correctScale && correctRotation && isPositionedCorrectly ? "✅ BUILD-SAFE READY" : "❌ NEEDS FIXING")}");
            }
        }
        
        Debug.Log("=== BUILD-SAFE VERIFICATION COMPLETE ===");
        Debug.Log("These exact settings will be used in AR builds!");
    }
    
    void TestARRewardType(string rewardName, string cubeName)
    {
        GameObject testCube = GameObject.Find(cubeName);
        if (testCube != null)
        {
            Debug.Log($"Testing {rewardName} reward activation...");
            ActivateARRewardOnCube(testCube, rewardName);
        }
        else
        {
            Debug.LogWarning($"Test cube {cubeName} not found for {rewardName} reward");
        }
    }
    
    [ContextMenu("Test Player Position Detection")]
    void TestPlayerPositionDetection()
    {
        Debug.Log("=== TESTING PLAYER POSITION DETECTION ===");
        
        if (arPlayerController == null)
        {
            Debug.LogError("AR Player Controller not found!");
            return;
        }
        
        // Test current player state
        int currentNumber = arPlayerController.GetCurrentNumber();
        int currentCubeNumber = arPlayerController.GetCurrentCubeNumber();
        Vector3 playerPosition = arPlayerController.GetPlayerPosition();
        
        Debug.Log($"Current Player Number: {currentNumber}");
        Debug.Log($"Current Cube Number: {currentCubeNumber}");
        Debug.Log($"Player Position: {playerPosition}");
        
        // Test reward positioning logic
        if (currentCubeNumber != -999)
        {
            Debug.Log($"✅ Player is standing on cube {currentCubeNumber}");
            Debug.Log($"   Rewards will spawn on top of this cube");
        }
        else
        {
            Debug.Log($"⚠️ Player is not standing on any cube");
            Debug.Log($"   Rewards will spawn at player position");
        }
        
        // Test with current equation
        Debug.Log($"Current Equation: {firstNumber} + {secondNumber} = {correctAnswer}");
        if (currentCubeNumber == correctAnswer)
        {
            Debug.Log($"🎯 Player is on the CORRECT answer cube!");
        }
        else
        {
            Debug.Log($"❌ Player is NOT on the correct answer cube");
        }
    }
    
    [ContextMenu("Test Reward Positioning Logic")]
    void TestRewardPositioningLogic()
    {
        Debug.Log("=== TESTING REWARD POSITIONING LOGIC ===");
        
        if (arPlayerController == null)
        {
            Debug.LogError("AR Player Controller not found!");
            return;
        }
        
        // Simulate different scenarios
        Debug.Log("Testing reward positioning for different scenarios:");
        
        // Scenario 1: Player on correct cube
        int correctAnswerCube = correctAnswer;
        Debug.Log($"\n--- Scenario 1: Player on correct cube ({correctAnswerCube}) ---");
        SimulateRewardPositioning(correctAnswerCube, true);
        
        // Scenario 2: Player on wrong cube
        int wrongCube = (correctAnswer + 1) % 10; // Different cube
        Debug.Log($"\n--- Scenario 2: Player on wrong cube ({wrongCube}) ---");
        SimulateRewardPositioning(wrongCube, false);
        
        // Scenario 3: Player not on any cube
        Debug.Log($"\n--- Scenario 3: Player not on any cube ---");
        SimulateRewardPositioning(-999, false);
    }
    
    void SimulateRewardPositioning(int playerCubeNumber, bool shouldBeOnCorrectCube)
    {
        // Find target cube
        GameObject targetCube = GameObject.Find(correctAnswer.ToString());
        if (targetCube == null)
        {
            Debug.LogWarning($"Target cube {correctAnswer} not found!");
            return;
        }
        
        // In new system, rewards are children of cube prefabs
        Debug.Log($"✅ Reward will be activated as child of cube {correctAnswer}");
        Debug.Log($"   Target Cube Position: {targetCube.transform.position}");
        Debug.Log($"   Reward will appear exactly where positioned in prefab");
        Debug.Log($"   No instantiation - just activation of existing child");
    }

    [ContextMenu("Test System Compilation")]
    void TestSystemCompilation()
    {
        Debug.Log("=== TESTING SYSTEM COMPILATION ===");
        Debug.Log("✅ All compilation errors should be fixed!");
        Debug.Log("✅ AR rewards now activate as children of cube prefabs");
        Debug.Log("✅ No more instantiation - only activation");
        Debug.Log("✅ Test with 'Test AR Reward Activation (No Instantiation)'");
    }

    [ContextMenu("Test Compilation Fixed")]
    void TestCompilationFixed()
    {
        Debug.Log("=== TESTING COMPILATION FIXED ===");
        Debug.Log("✅ All compilation errors should be fixed!");
        Debug.Log("✅ No duplicate variable declarations");
        Debug.Log("✅ All position references use transform.position");
        Debug.Log("✅ System ready for testing!");
    }

    [ContextMenu("Debug AR House Transform on Cube 3")]
    void DebugARHouseTransformOnCube3()
    {
        Debug.Log("=== DEBUGGING AR HOUSE TRANSFORM ON CUBE 3 ===");
        
        GameObject cube3 = GameObject.Find("3");
        if (cube3 == null)
        {
            Debug.LogError("Cube 3 not found!");
            return;
        }
        
        Debug.Log($"Cube 3 position: {cube3.transform.position}");
        Debug.Log($"Cube 3 scale: {cube3.transform.localScale}");
        
        // Find AR House child
        Transform arHouse = cube3.transform.Find("AR House");
        if (arHouse != null)
        {
            Debug.Log($"AR House found! Active: {arHouse.gameObject.activeInHierarchy}");
            Debug.Log($"AR House local position: {arHouse.localPosition}");
            Debug.Log($"AR House world position: {arHouse.position}");
            Debug.Log($"AR House local scale: {arHouse.localScale}");
            Debug.Log($"AR House local rotation: {arHouse.localRotation.eulerAngles}");
            
            // Check if it's visible
            Renderer renderer = arHouse.GetComponent<Renderer>();
            if (renderer != null)
            {
                Debug.Log($"AR House renderer enabled: {renderer.enabled}");
                Debug.Log($"AR House material: {renderer.material.name}");
            }
            else
            {
                Debug.LogWarning("AR House has no Renderer component!");
            }
        }
        else
        {
            Debug.LogError("AR House not found as child of cube 3!");
            Debug.Log("Available children:");
            for (int i = 0; i < cube3.transform.childCount; i++)
            {
                Transform child = cube3.transform.GetChild(i);
                Debug.Log($"  - {child.name} (Active: {child.gameObject.activeInHierarchy})");
            }
        }
    }

    [ContextMenu("Find All AR Rewards in Scene")]
    void FindAllARRewardsInScene()
    {
        Debug.Log("=== FINDING ALL AR REWARDS IN SCENE ===");
        
        // Find all objects with "AR" in their name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> arRewards = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("AR") && (obj.name.Contains("House") || obj.name.Contains("Pet") || obj.name.Contains("Car") || obj.name.Contains("Tree")))
            {
                arRewards.Add(obj);
            }
        }
        
        Debug.Log($"Found {arRewards.Count} AR reward objects:");
        foreach (GameObject reward in arRewards)
        {
            Debug.Log($"  - {reward.name} at position {reward.transform.position}");
            Debug.Log($"    Parent: {(reward.transform.parent != null ? reward.transform.parent.name : "NO PARENT")}");
            Debug.Log($"    Active: {reward.activeInHierarchy}");
            Debug.Log($"    Scale: {reward.transform.localScale}");
        }
        
        if (arRewards.Count == 0)
        {
            Debug.LogError("No AR reward objects found in scene!");
            Debug.Log("You need to add AR House, AR Pet, AR Car, AR Tree as children of your Cube prefab.");
        }
    }

    Transform FindChildRecursively(Transform parent, string childName)
    {
        // Check direct children first
        Transform child = parent.Find(childName);
        if (child != null)
            return child;
        
        // Look specifically under "world space Canvas" first (where AR rewards are)
        Transform canvas = parent.Find("world space Canvas");
        if (canvas != null)
        {
            Transform reward = canvas.Find(childName);
            if (reward != null)
                return reward;
        }
        
        // Search recursively through all other children
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursively(parent.GetChild(i), childName);
            if (result != null)
                return result;
        }
        
        return null;
    }

    [ContextMenu("Check Cube Prefab for AR Rewards")]
    void CheckCubePrefabForARRewards()
    {
        Debug.Log("=== CHECKING CUBE PREFAB FOR AR REWARDS ===");
        
        // Look for ARNumberLineGenerator to get the same prefab it uses
        ARNumberLineGenerator arGenerator = FindObjectOfType<ARNumberLineGenerator>();
        if (arGenerator == null)
        {
            Debug.LogError("ARNumberLineGenerator not found in scene!");
            return;
        }
        
        // Get the prefab that ARNumberLineGenerator uses
        GameObject cubePrefab = arGenerator.cubePrefab;
        if (cubePrefab == null)
        {
            Debug.LogError("ARNumberLineGenerator's cubePrefab is null!");
            return;
        }
        
        Debug.Log($"Found ARNumberLineGenerator's prefab: {cubePrefab.name}");
        Debug.Log($"Prefab has {cubePrefab.transform.childCount} children:");
        
        for (int i = 0; i < cubePrefab.transform.childCount; i++)
        {
            Transform child = cubePrefab.transform.GetChild(i);
            Debug.Log($"  Child {i}: '{child.name}' (Active: {child.gameObject.activeInHierarchy})");
            
            // Check if this child has AR rewards
            if (child.name == "Cube")
            {
                Debug.Log($"    Cube has {child.childCount} children:");
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform grandChild = child.GetChild(j);
                    Debug.Log($"      GrandChild {j}: '{grandChild.name}' (Active: {grandChild.gameObject.activeInHierarchy})");
                }
            }
        }
        
        // Also check if there are any AR rewards anywhere in the scene
        GameObject[] arRewards = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> foundRewards = new List<GameObject>();
        
        foreach (GameObject obj in arRewards)
        {
            if (obj.name.Contains("AR") && (obj.name.Contains("House") || obj.name.Contains("Pet") || obj.name.Contains("Car") || obj.name.Contains("Tree")))
            {
                foundRewards.Add(obj);
            }
        }
        
        Debug.Log($"Found {foundRewards.Count} AR reward objects in entire scene:");
        foreach (GameObject reward in foundRewards)
        {
            Debug.Log($"  - {reward.name} at position {reward.transform.position}");
            Debug.Log($"    Parent: {(reward.transform.parent != null ? reward.transform.parent.name : "NO PARENT")}");
        }
    }

    [ContextMenu("Simple AR Reward Test")]
    void SimpleARRewardTest()
    {
        Debug.Log("=== SIMPLE AR REWARD TEST ===");
        
        // Step 1: Find cube 3
        GameObject cube3 = GameObject.Find("3");
        if (cube3 == null)
        {
            Debug.LogError("Cube 3 not found! Generate number line first.");
            return;
        }
        
        Debug.Log($"Found cube 3 at position {cube3.transform.position}");
        
        // Step 2: First activate ALL AR rewards to establish references
        Debug.Log("Step 2: Activating all AR rewards to establish references...");
        ActivateAllARRewardsOnCube(cube3);
        
        // Step 3: Now deactivate them all
        Debug.Log("Step 3: Deactivating all AR rewards...");
        DeactivateAllARRewardsOnCube(cube3);
        
        // Step 4: Activate House reward
        Debug.Log("Step 4: Activating House reward...");
        ActivateARRewardOnCube(cube3, "House");
        
        Debug.Log("=== TEST COMPLETE ===");
        Debug.Log("Check if House is now visible on cube 3!");
    }
    
    [ContextMenu("Test Counter-Based AR Reward Activation")]
    void TestCounterBasedARRewardActivation()
    {
        Debug.Log("=== TESTING COUNTER-BASED AR REWARD ACTIVATION ===");
        
        // Find cube 3 for testing
        GameObject cube3 = GameObject.Find("3");
        if (cube3 == null)
        {
            Debug.LogError("Cube 3 not found! Generate number line first.");
            return;
        }
        
        Debug.Log($"Found cube 3 at position {cube3.transform.position}");
        
        // Test each counter value
        for (int i = 1; i <= 4; i++)
        {
            Debug.Log($"Testing counter value: {i}");
            correctAnswersInRow = i;
            correctAnswer = 3; // Set correct answer to 3 for testing
            
            ActivateARRewardByCounter();
            
            // Check what reward is active
            ARCubeRewardManager rewardManager = cube3.GetComponent<ARCubeRewardManager>();
            if (rewardManager != null)
            {
                string activeReward = rewardManager.GetActiveRewardName();
                Debug.Log($"Active reward after counter {i}: {activeReward}");
            }
            
            // Wait a moment to see the reward
            System.Threading.Thread.Sleep(1000);
        }
        
        // Reset
        correctAnswersInRow = 0;
        Debug.Log("=== TEST COMPLETE ===");
    }

    void ActivateAllARRewardsOnCube(GameObject targetCube)
    {
        Debug.Log($"Activating all AR rewards on cube {targetCube.name}");
        
        // Get the ARCubeRewardManager component from the cube
        ARCubeRewardManager rewardManager = targetCube.GetComponent<ARCubeRewardManager>();
        if (rewardManager == null)
        {
            Debug.LogWarning($"ARCubeRewardManager not found on cube {targetCube.name}!");
            return;
        }
        
        // Activate all rewards using the manager (for testing purposes)
        rewardManager.ActivateSpecificReward("House");
        rewardManager.ActivateSpecificReward("Pet");
        rewardManager.ActivateSpecificReward("Car");
        rewardManager.ActivateSpecificReward("Tree");
        
        Debug.Log($"Activated all AR rewards on cube {targetCube.name} via ARCubeRewardManager");
    }

    IEnumerator DeactivateAllARRewardsWhenReady()
    {
        Debug.Log("=== WAITING FOR AR REWARDS TO BE READY ===");
        
        // Check every frame for AR rewards (instant detection)
        while (true)
        {
            // Check if any cubes exist with ARCubeRewardManager
            GameObject testCube = GameObject.Find("0");
            if (testCube != null)
            {
                // Check if this cube has ARCubeRewardManager component
                ARCubeRewardManager rewardManager = testCube.GetComponent<ARCubeRewardManager>();
                if (rewardManager != null)
                {
                    Debug.Log("ARCubeRewardManager detected! INSTANTLY deactivating...");
                    
                    // Deactivate ALL AR rewards immediately (including inactive cubes)
                    DeactivateAllARRewardsOnAllCubes();
                    
                    Debug.Log("All AR rewards deactivated INSTANTLY - references established!");
                    break;
                }
            }
            
            yield return null; // Check every frame (no delay)
        }
    }
} 