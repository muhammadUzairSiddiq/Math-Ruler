using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomSetup : MonoBehaviour
{
    [Header("Existing UI References")]
    public Canvas uiCanvas;
    public GameObject resultPanel;
    public GameObject correctResultImage;
    public GameObject wrongResultImage;
    
    [Header("UI Elements")]
    public TextMeshProUGUI equationText;
    public Button submitButton;
    public TextMeshProUGUI submitButtonText;
    public TextMeshProUGUI correctAnswerCountText;
    public TextMeshProUGUI resultText;
    
    [Header("Manager References")]
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public InputHandler inputHandler;
    
    [Header("Reward Prefabs")]
    public GameObject housePrefab;
    public GameObject petPrefab;
    public GameObject carPrefab;
    public GameObject treePrefab;
    
    [Header("AR Reward Prefabs")]
    // AR rewards are now children of cube prefabs - no prefab fields needed
    
    [Header("Reward Buttons")]
    public Button houseRewardButton;
    public Button petRewardButton;
    public Button carRewardButton;
    public Button treeRewardButton;
    
    [Header("Setup Settings")]
    public bool autoSetup = true;
    public bool useExistingResultPanel = true;
    
    [Header("Feedback Objects")]
    public GameObject cross;
    public GameObject tick;
    public GameObject gameWon;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupWithExistingUI();
        }
    }
    
    [ContextMenu("Setup with Existing UI")]
    public void SetupWithExistingUI()
    {
        Debug.Log("=== CUSTOM SETUP STARTED ===");
        
        // Find existing UI elements
        FindExistingUIElements();
        
        // Create missing UI elements
        CreateMissingUIElements();
        
        // Create managers
        CreateManagers();
        
        // Assign references
        AssignReferences();
        
        // Configure UI
        ConfigureUI();
        
        // Auto-find feedback objects
        if (cross == null) cross = GameObject.Find("cross");
        if (tick == null) tick = GameObject.Find("tick");
        if (gameWon == null) gameWon = GameObject.Find("gamewon");
        
        Debug.Log("=== CUSTOM SETUP COMPLETED ===");
    }
    
    void FindExistingUIElements()
    {
        // Find existing Canvas
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            Debug.Log($"Found Canvas: {uiCanvas?.name}");
        }
        
        // Find existing Result Panel
        if (resultPanel == null)
        {
            resultPanel = GameObject.Find("Result Panel");
            Debug.Log($"Found Result Panel: {resultPanel?.name}");
            
            // If result panel is found, also find its child images
            if (resultPanel != null)
            {
                // Find child images within the result panel
                Transform[] children = resultPanel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.name == "Correct Result" && correctResultImage == null)
                    {
                        correctResultImage = child.gameObject;
                        Debug.Log($"Found Correct Result image: {child.name}");
                    }
                    else if (child.name == "wrong Result" && wrongResultImage == null)
                    {
                        wrongResultImage = child.gameObject;
                        Debug.Log($"Found Wrong Result image: {child.name}");
                    }
                }
            }
        }
        
        // If still not found, try direct search
        if (correctResultImage == null)
        {
            correctResultImage = GameObject.Find("Correct Result");
            Debug.Log($"Found Correct Result (direct search): {correctResultImage?.name}");
        }
        
        if (wrongResultImage == null)
        {
            wrongResultImage = GameObject.Find("wrong Result");
            Debug.Log($"Found Wrong Result (direct search): {wrongResultImage?.name}");
        }
        
        // Find existing equation text
        if (equationText == null)
        {
            GameObject equationGO = GameObject.Find("equation text");
            if (equationGO != null)
            {
                equationText = equationGO.GetComponent<TextMeshProUGUI>();
                Debug.Log($"Found existing equation text: {equationText?.name}");
            }
            else
            {
                Debug.Log("No existing equation text found - will create one");
            }
        }
        
        // Find existing correct answer count text
        if (correctAnswerCountText == null)
        {
            // Look for any TextMeshPro in top-right corner
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in allTexts)
            {
                RectTransform rect = text.GetComponent<RectTransform>();
                if (rect != null && rect.anchorMin.x > 0.8f && rect.anchorMin.y > 0.8f)
                {
                    correctAnswerCountText = text;
                    Debug.Log($"Found correct answer count text: {text.name}");
                    break;
                }
            }
            
            if (correctAnswerCountText == null)
            {
                Debug.Log("No correct answer count text found in top-right corner");
            }
        }
        
        // Find result text (for showing correct/wrong messages)
        TextMeshProUGUI resultText = null;
        if (resultPanel != null)
        {
            // Look for result text within the result panel
            resultText = resultPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (resultText != null)
            {
                Debug.Log($"Found result text in result panel: {resultText.name}");
            }
        }
        
        // If not found in result panel, try to find by name
        if (resultText == null)
        {
            GameObject resultTextGO = GameObject.Find("Result Text");
            if (resultTextGO != null)
            {
                resultText = resultTextGO.GetComponent<TextMeshProUGUI>();
                Debug.Log($"Found result text by name: {resultText?.name}");
            }
        }
        
        // Store result text for assignment later
        this.resultText = resultText;
        
        // Find reward prefabs
        FindRewardPrefabs();
        
        // Find existing reward buttons
        FindRewardButtons();
    }
    
    void FindRewardButtons()
    {
        Debug.Log("Looking for existing reward buttons in scene...");
        
        // Find existing reward buttons by name
        if (houseRewardButton == null)
        {
            GameObject houseButtonGO = GameObject.Find("House Button");
            if (houseButtonGO != null)
            {
                houseRewardButton = houseButtonGO.GetComponent<Button>();
                Debug.Log($"Found existing House Button: {houseButtonGO.name}");
            }
            else
            {
                Debug.LogWarning("House Button not found - please check the name");
            }
        }
        
        if (petRewardButton == null)
        {
            GameObject petButtonGO = GameObject.Find("Pet Button");
            if (petButtonGO != null)
            {
                petRewardButton = petButtonGO.GetComponent<Button>();
                Debug.Log($"Found existing Pet Button: {petButtonGO.name}");
            }
            else
            {
                Debug.LogWarning("Pet Button not found - please check the name");
            }
        }
        
        if (carRewardButton == null)
        {
            GameObject carButtonGO = GameObject.Find("Car Button");
            if (carButtonGO != null)
            {
                carRewardButton = carButtonGO.GetComponent<Button>();
                Debug.Log($"Found existing Car Button: {carButtonGO.name}");
            }
            else
            {
                Debug.LogWarning("Car Button not found - please check the name");
            }
        }
        
        if (treeRewardButton == null)
        {
            GameObject treeButtonGO = GameObject.Find("Tree Button");
            if (treeButtonGO != null)
            {
                treeRewardButton = treeButtonGO.GetComponent<Button>();
                Debug.Log($"Found existing Tree Button: {treeButtonGO.name}");
            }
            else
            {
                Debug.LogWarning("Tree Button not found - please check the name");
            }
        }
        
        Debug.Log($"Reward buttons found - House: {(houseRewardButton != null ? "Yes" : "No")}, Pet: {(petRewardButton != null ? "Yes" : "No")}, Car: {(carRewardButton != null ? "Yes" : "No")}, Tree: {(treeRewardButton != null ? "Yes" : "No")}");
    }
    
    void CreateMissingUIElements()
    {
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found! Cannot create UI elements.");
            return;
        }
        
        // Create Equation Text (only if not found)
        if (equationText == null)
        {
            GameObject equationGO = new GameObject("Equation Text");
            equationGO.transform.SetParent(uiCanvas.transform, false);
            
            RectTransform rectTransform = equationGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.9f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.9f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 80);
            
            equationText = equationGO.AddComponent<TextMeshProUGUI>();
            equationText.text = "7 + 8 = 0";
            equationText.fontSize = 36;
            equationText.color = Color.white;
            equationText.alignment = TextAlignmentOptions.Center;
            
            Debug.Log("Created new Equation Text");
        }
        else
        {
            // Update existing equation text
            equationText.text = "7 + 8 = 0";
            Debug.Log("Using existing equation text");
        }
        
        // Find existing Submit Button
        if (submitButton == null)
        {
            GameObject submitButtonGO = GameObject.Find("Submit Button");
            if (submitButtonGO != null)
            {
                submitButton = submitButtonGO.GetComponent<Button>();
                submitButtonText = submitButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                Debug.Log($"Found existing Submit Button: {submitButton.name}");
            }
            else
            {
                Debug.Log("No Submit Button found - please add one manually");
            }
        }
        
        // Create reward buttons if they don't exist
        // CreateRewardButtons(); // This line is removed as per the edit hint
    }
    
    void CreateManagers()
    {
        // Create GameManager with AnswerVerifier
        if (answerVerifier == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            answerVerifier = gameManager.AddComponent<AnswerVerifier>();
            Debug.Log("Created GameManager with AnswerVerifier");
        }
        
        // Create UIManager
        if (uiManager == null)
        {
            GameObject uiManagerGO = new GameObject("UIManager");
            uiManager = uiManagerGO.AddComponent<UIManager>();
            Debug.Log("Created UIManager");
        }
        
        // Create InputHandler
        if (inputHandler == null)
        {
            GameObject inputHandlerGO = new GameObject("InputHandler");
            inputHandler = inputHandlerGO.AddComponent<InputHandler>();
            Debug.Log("Created InputHandler");
        }
    }
    
    void AssignReferences()
    {
        // Assign references to AnswerVerifier
        if (answerVerifier != null)
        {
            answerVerifier.equationText = equationText;
            answerVerifier.submitButton = submitButton;
            answerVerifier.submitButtonText = submitButtonText;
            answerVerifier.correctAnswerCountText = correctAnswerCountText;
            answerVerifier.resultText = resultText;
            
            // CRITICAL: Assign UIManager to AnswerVerifier
            answerVerifier.uiManager = uiManager;
            Debug.Log($"Assigned UIManager to AnswerVerifier: {(uiManager != null ? "Yes" : "No")}");
            
            // Assign reward prefabs
            answerVerifier.housePrefab = housePrefab;
            answerVerifier.petPrefab = petPrefab;
            answerVerifier.carPrefab = carPrefab;
            answerVerifier.treePrefab = treePrefab;
            
            // AR reward prefabs are now children of cube prefabs - no assignment needed
            
            // CRITICAL: Assign reward buttons to AnswerVerifier
            answerVerifier.houseRewardButton = houseRewardButton;
            answerVerifier.petRewardButton = petRewardButton;
            answerVerifier.carRewardButton = carRewardButton;
            answerVerifier.treeRewardButton = treeRewardButton;
            Debug.Log($"Assigned reward buttons to AnswerVerifier - House: {(houseRewardButton != null ? "Yes" : "No")}, Pet: {(petRewardButton != null ? "Yes" : "No")}, Car: {(carRewardButton != null ? "Yes" : "No")}, Tree: {(treeRewardButton != null ? "Yes" : "No")}");
            
            // Use existing result panel if available
            if (useExistingResultPanel && resultPanel != null)
            {
                answerVerifier.resultPanel = resultPanel;
                Debug.Log("Assigned result panel to AnswerVerifier");
            }
            
            // Find PlayerController
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                answerVerifier.playerController = playerController;
                Debug.Log($"Assigned PlayerController: {playerController.name}");
            }
            else
            {
                Debug.LogWarning("PlayerController not found! Please assign it manually.");
            }
        }
        
        // Assign references to UIManager
        if (uiManager != null)
        {
            uiManager.equationText = equationText;
            uiManager.submitButton = submitButton;
            uiManager.submitButtonText = submitButtonText;
            
            // Assign reward buttons to UIManager
            uiManager.houseRewardButton = houseRewardButton;
            uiManager.petRewardButton = petRewardButton;
            uiManager.carRewardButton = carRewardButton;
            uiManager.treeRewardButton = treeRewardButton;
            
            // Use existing result panel and images
            if (useExistingResultPanel && resultPanel != null)
            {
                uiManager.resultPanel = resultPanel;
                uiManager.correctResultImage = correctResultImage;
                uiManager.wrongResultImage = wrongResultImage;
                Debug.Log("Assigned result panel and images to UIManager");
            }
            
            Debug.Log("Assigned reward buttons to UIManager");
        }
        
        // Assign references to InputHandler
        if (inputHandler != null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                inputHandler.playerController = playerController;
            }
            
            if (answerVerifier != null)
            {
                inputHandler.answerVerifier = answerVerifier;
            }
        }
    }
    
    void ConfigureUI()
    {
        // Configure submit button onClick
        if (submitButton != null && answerVerifier != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => answerVerifier.SubmitAnswer());
            Debug.Log("Configured Submit Button onClick");
        }
        
        // Hide result panel initially
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
            Debug.Log("Hidden Result Panel initially");
        }
        
        // Hide result images initially
        if (correctResultImage != null)
        {
            correctResultImage.SetActive(false);
        }
        
        if (wrongResultImage != null)
        {
            wrongResultImage.SetActive(false);
        }
    }
    
    [ContextMenu("Check Setup")]
    public void CheckSetup()
    {
        Debug.Log("=== CHECKING CUSTOM SETUP ===");
        
        // Check for required components
        AnswerVerifier answerVerifier = FindObjectOfType<AnswerVerifier>();
        UIManager uiManager = FindObjectOfType<UIManager>();
        InputHandler inputHandler = FindObjectOfType<InputHandler>();
        PlayerController playerController = FindObjectOfType<PlayerController>();
        
        if (answerVerifier != null)
            Debug.Log("✓ AnswerVerifier found");
        else
            Debug.LogError("✗ AnswerVerifier not found");
            
        if (uiManager != null)
            Debug.Log("✓ UIManager found");
        else
            Debug.LogError("✗ UIManager not found");
            
        if (inputHandler != null)
            Debug.Log("✓ InputHandler found");
        else
            Debug.LogError("✗ InputHandler not found");
            
        if (playerController != null)
            Debug.Log("✓ PlayerController found");
        else
            Debug.LogWarning("⚠ PlayerController not found - please add one to your scene");
        
        // Check for UI elements
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
            Debug.Log("✓ Canvas found");
        else
            Debug.LogError("✗ Canvas not found");
            
        TextMeshProUGUI equationText = FindObjectOfType<TextMeshProUGUI>();
        if (equationText != null)
            Debug.Log("✓ UI Text elements found");
        else
            Debug.LogError("✗ UI Text elements not found");
        
        // Check for existing result panel
        GameObject resultPanel = GameObject.Find("Result Panel");
        if (resultPanel != null)
            Debug.Log("✓ Existing Result Panel found");
        else
            Debug.LogWarning("⚠ No existing Result Panel found");
        
        Debug.Log("=== CUSTOM SETUP CHECK COMPLETED ===");
    }
    
    void FindRewardPrefabs()
    {
        // Try to find mobile prefabs by name in Resources folder or Assets
        if (housePrefab == null)
        {
            housePrefab = FindPrefabByName("house");
            if (housePrefab != null)
                Debug.Log($"Found house prefab: {housePrefab.name}");
        }
        
        if (petPrefab == null)
        {
            petPrefab = FindPrefabByName("pet");
            if (petPrefab != null)
                Debug.Log($"Found pet prefab: {petPrefab.name}");
        }
        
        if (carPrefab == null)
        {
            carPrefab = FindPrefabByName("car");
            if (carPrefab != null)
                Debug.Log($"Found car prefab: {carPrefab.name}");
        }
        
        if (treePrefab == null)
        {
            treePrefab = FindPrefabByName("tree");
            if (treePrefab != null)
                Debug.Log($"Found tree prefab: {treePrefab.name}");
        }
        
        // AR rewards are now children of cube prefabs - no need to find AR prefabs
    }
    
    GameObject FindPrefabByName(string name)
    {
        // Try to find prefab in Resources folder
        GameObject prefab = Resources.Load<GameObject>(name);
        if (prefab != null)
            return prefab;
        
        // Try to find prefab in Assets/Prefabs folder
        prefab = Resources.Load<GameObject>($"Prefabs/{name}");
        if (prefab != null)
            return prefab;
        
        // Try to find prefab in Assets/Prefabs/Reward Objects folder
        prefab = Resources.Load<GameObject>($"Prefabs/Reward Objects/{name}");
        if (prefab != null)
            return prefab;
        
        // Try to find prefab in Assets/Prefabs/Reward Objects/AR Rewards folder
        prefab = Resources.Load<GameObject>($"Prefabs/Reward Objects/AR Rewards/{name}");
        if (prefab != null)
            return prefab;
        
        Debug.LogWarning($"Could not find prefab: {name}");
        return null;
    }
} 