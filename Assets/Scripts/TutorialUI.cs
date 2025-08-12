using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialTitle;
    public TextMeshProUGUI tutorialDescription;
    public Image tutorialImage;
    
    [Header("Navigation Buttons")]
    public Button tutorialPreviousButton;
    public Button tutorialNextButton;
    public Button tutorialSkipButton;
    public Button closeButton;
    
    [Header("Progress")]
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    
    [Header("Step GameObjects")]
    public GameObject step1Object;
    public GameObject step2Object;
    public GameObject step3Object;
    public GameObject step4Object;
    
    [Header("Animation")]
    public Animator tutorialAnimator;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;
    
    private PracticeGameManager practiceManager;
    private CanvasGroup canvasGroup;
    
         // Tutorial system
     private int currentTutorialStep = 0;
     private bool tutorialCompleted = false;
     private GameObject currentFocusObject = null;
     private GameObject tutorialHouseReward = null;
    
    void Start()
    {
        InitializeTutorialUI();
    }
    
    void InitializeTutorialUI()
    {
        Debug.Log("=== TUTORIAL UI INITIALIZATION ===");
        
        // Find practice manager
        practiceManager = FindObjectOfType<PracticeGameManager>();
        
        // Get canvas group for fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Keep tutorial panel active by default - don't hide it
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Debug.Log("✅ Tutorial Panel kept active by default");
        }
        
        Debug.Log("Tutorial UI initialized");
    }
    
    void SetupButtonListeners()
    {
        if (tutorialPreviousButton != null)
        {
            tutorialPreviousButton.onClick.AddListener(OnPreviousClicked);
        }
        
        if (tutorialNextButton != null)
        {
            tutorialNextButton.onClick.AddListener(OnNextClicked);
        }
        
        if (tutorialSkipButton != null)
        {
            tutorialSkipButton.onClick.AddListener(OnSkipClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }
    
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            
            // Fade in effect
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
            
            // Update progress
            UpdateProgress();
            
            Debug.Log("Tutorial panel shown");
        }
    }
    
    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            // Fade out effect
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                tutorialPanel.SetActive(false);
            }
            
            Debug.Log("Tutorial panel hidden");
        }
    }
    
    public void UpdateTutorialContent(string title, string description, Sprite image = null)
    {
        if (tutorialTitle != null)
        {
            tutorialTitle.text = title;
        }
        
        if (tutorialDescription != null)
        {
            tutorialDescription.text = description;
        }
        
        if (tutorialImage != null && image != null)
        {
            tutorialImage.sprite = image;
            tutorialImage.gameObject.SetActive(true);
        }
        else if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }
        
        Debug.Log($"Tutorial content updated: {title}");
    }
    
         public void UpdateProgress()
     {
         if (progressSlider != null)
         {
             float progress = (float)(currentTutorialStep + 1) / 4f; // 4 tutorial steps
             progressSlider.value = progress;
         }
         
         if (progressText != null)
         {
             progressText.text = $"{currentTutorialStep + 1} / 4";
         }
        
        // Update step GameObjects visibility
        UpdateStepGameObjects();
    }
    
    void UpdateStepGameObjects()
    {
        // Deactivate all step objects first
        if (step1Object != null) step1Object.SetActive(false);
        if (step2Object != null) step2Object.SetActive(false);
        if (step3Object != null) step3Object.SetActive(false);
        if (step4Object != null) step4Object.SetActive(false);
        
        // Activate only the current step object
        switch (currentTutorialStep)
        {
            case 0: // Step 1
                if (step1Object != null) step1Object.SetActive(true);
                Debug.Log("🎯 Step 1 GameObject activated");
                break;
            case 1: // Step 2
                if (step2Object != null) step2Object.SetActive(true);
                Debug.Log("🎯 Step 2 GameObject activated");
                break;
            case 2: // Step 3
                if (step3Object != null) step3Object.SetActive(true);
                Debug.Log("🎯 Step 3 GameObject activated");
                break;
            case 3: // Step 4
                if (step4Object != null) step4Object.SetActive(true);
                Debug.Log("🎯 Step 4 GameObject activated");
                break;
        }
    }
    
    public void SetButtonStates(bool canGoPrevious, bool canGoNext)
    {
        if (tutorialPreviousButton != null)
        {
            tutorialPreviousButton.interactable = canGoPrevious;
        }
        
        if (tutorialNextButton != null)
        {
            tutorialNextButton.interactable = canGoNext;
        }
    }
    
    // Button event handlers - REPLACED for progressive tutorial flow
    void OnPreviousClicked()
    {
        if (currentTutorialStep > 0)
        {
            currentTutorialStep--;
            ShowTutorialStep(currentTutorialStep);
        }
        
        // Play button sound
        PlayButtonSound();
    }
    
    void OnNextClicked()
    {
        if (currentTutorialStep < 3)
        {
            currentTutorialStep++;
            ShowTutorialStep(currentTutorialStep);
        }
        else
        {
            // Step 4 completion - show reward and complete
            ShowRewardAndComplete();
        }
        
        // Play button sound
        PlayButtonSound();
    }
    
    void OnSkipClicked()
    {
        // Skip to completion
        currentTutorialStep = 3;
        ShowTutorialStep(3);
        
        // Play button sound
        PlayButtonSound();
    }
    
    void OnCloseClicked()
    {
        HideTutorial();
        
        // Play button sound
        PlayButtonSound();
    }
    
    void PlayButtonSound()
    {
        // You can add audio manager reference here
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
    }
    
    // Fade effects
    System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        tutorialPanel.SetActive(false);
    }
    
    // Context menu for testing
    [ContextMenu("Test Show Tutorial")]
    void TestShowTutorial()
    {
        Debug.Log("=== TESTING TUTORIAL UI ===");
        ShowTutorial();
        UpdateTutorialContent("Test Title", "This is a test tutorial step to verify the UI is working correctly.");
        Debug.Log("Tutorial should now be visible!");
    }
    
    [ContextMenu("Test Hide Tutorial")]
    void TestHideTutorial()
    {
        Debug.Log("=== HIDING TUTORIAL UI ===");
        HideTutorial();
        Debug.Log("Tutorial should now be hidden!");
    }
    
    [ContextMenu("🔓 Force Activate Tutorial Panel")]
    void ForceActivateTutorialPanel()
    {
        Debug.Log("=== FORCE ACTIVATING TUTORIAL PANEL ===");
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Debug.Log("✅ Tutorial Panel force activated!");
        }
        else
        {
            Debug.LogError("❌ Tutorial Panel not assigned!");
        }
    }
    
    [ContextMenu("🎯 Test Visual Focus System")]
    void TestVisualFocusSystem()
    {
        Debug.Log("=== TESTING VISUAL FOCUS SYSTEM ===");
        
        if (tutorialImage == null)
        {
            Debug.LogError("❌ Tutorial Image not assigned! Please assign it first.");
            return;
        }
        
        // Test focusing on different objects
        Debug.Log("Testing focus on 'Equation'...");
        FocusOnObject("Equation");
        
        // Wait 2 seconds then test next object
        StartCoroutine(TestFocusSequence());
    }
    
    [ContextMenu("🔍 Find All UI Objects in Scene")]
    void FindAllUIObjectsInScene()
    {
        Debug.Log("=== FINDING ALL UI OBJECTS IN SCENE ===");
        
        // Find all UI elements
        Button[] buttons = FindObjectsOfType<Button>();
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();
        Image[] images = FindObjectsOfType<Image>();
        
        Debug.Log($"📊 Found {buttons.Length} buttons, {texts.Length} texts, {images.Length} images");
        
        Debug.Log("🔘 BUTTONS:");
        foreach (Button btn in buttons)
        {
            Debug.Log($"  - {btn.name} (Position: {btn.transform.position})");
        }
        
        Debug.Log("📝 TEXTS:");
        foreach (TextMeshProUGUI text in texts)
        {
            Debug.Log($"  - {text.name} (Text: '{text.text.Substring(0, Mathf.Min(20, text.text.Length))}...')");
        }
        
        Debug.Log("🖼️ IMAGES:");
        foreach (Image img in images)
        {
            Debug.Log($"  - {img.name} (Position: {img.transform.position})");
        }
        
        Debug.Log("=== UI OBJECTS SEARCH COMPLETE ===");
    }
    
    [ContextMenu("🎯 Test Focus on Common Names")]
    void TestFocusOnCommonNames()
    {
        Debug.Log("=== TESTING FOCUS ON COMMON NAMES ===");
        
        if (tutorialImage == null)
        {
            Debug.LogError("❌ Tutorial Image not assigned! Please assign it first.");
            return;
        }
        
        // Test common UI element names
        string[] commonNames = {
            "Question", "Equation", "Text", "Button", "Left", "Right", "Previous", "Next",
            "Help", "AI", "Assistance", "Reward", "House", "Pet", "Car", "Tree"
        };
        
        StartCoroutine(TestCommonNamesSequence(commonNames));
    }
    
    System.Collections.IEnumerator TestCommonNamesSequence(string[] names)
    {
        foreach (string name in names)
        {
            Debug.Log($"🎯 Testing focus on '{name}'...");
            bool success = FocusOnObject(name);
            
            if (success)
            {
                Debug.Log($"✅ Successfully focused on '{name}'!");
                yield return new WaitForSeconds(3f); // Show for 3 seconds
            }
            else
            {
                Debug.Log($"❌ Failed to focus on '{name}'");
                yield return new WaitForSeconds(1f); // Brief pause
            }
        }
        
        Debug.Log("🎯 Hiding focus...");
        HideCurrentFocus();
        Debug.Log("✅ Common names test complete!");
    }
    
    System.Collections.IEnumerator TestFocusSequence()
    {
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Testing focus on 'LeftButton'...");
        FocusOnObject("LeftButton");
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Testing focus on 'AIAssistance'...");
        FocusOnObject("AIAssistance");
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Testing focus on 'RewardButton'...");
        FocusOnObject("RewardButton");
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Hiding focus...");
        HideCurrentFocus();
        
        Debug.Log("✅ Visual focus test complete!");
    }
    
    // Auto-assignment context menus
    [ContextMenu("🔧 Auto-Assign Tutorial UI References")]
    void AutoAssignTutorialUIReferences()
    {
        Debug.Log("=== AUTO-ASSIGNING TUTORIAL UI REFERENCES ===");
        
        // Find Tutorial Panel
        if (tutorialPanel == null)
        {
            // Look for Tutorial Panel in the same canvas
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Transform tutorialPanelTransform = parentCanvas.transform.Find("Tutorial Panel");
                if (tutorialPanelTransform != null)
                {
                    tutorialPanel = tutorialPanelTransform.gameObject;
                    Debug.Log("✅ Tutorial Panel assigned: " + tutorialPanel.name);
                }
                else
                {
                    Debug.LogWarning("❌ Tutorial Panel not found! Make sure it's named 'Tutorial Panel'");
                }
            }
        }
        
        // Find Tutorial Title
        if (tutorialTitle == null && tutorialPanel != null)
        {
            TextMeshProUGUI[] texts = tutorialPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.ToLower().Contains("title") || text.name.ToLower().Contains("header"))
                {
                    tutorialTitle = text;
                    Debug.Log("✅ Tutorial Title assigned: " + text.name);
                    break;
                }
            }
        }
        
        // Find Tutorial Description
        if (tutorialDescription == null && tutorialPanel != null)
        {
            TextMeshProUGUI[] texts = tutorialPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.ToLower().Contains("description") || text.name.ToLower().Contains("body") || text.name.ToLower().Contains("text"))
                {
                    tutorialDescription = text;
                    Debug.Log("✅ Tutorial Description assigned: " + text.name);
                    break;
                }
            }
        }
        
        // Find Tutorial Image
        if (tutorialImage == null && tutorialPanel != null)
        {
            Image[] images = tutorialPanel.GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                if (image.name.ToLower().Contains("image") || image.name.ToLower().Contains("icon"))
                {
                    tutorialImage = image;
                    Debug.Log("✅ Tutorial Image assigned: " + image.name);
                    break;
                }
            }
        }
        
        // Find Navigation Buttons
        if (tutorialPanel != null)
        {
            Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                string buttonName = button.name.ToLower();
                
                if (buttonName.Contains("next"))
                {
                    tutorialNextButton = button;
                    Debug.Log("✅ Next Button assigned: " + button.name);
                }
                else if (buttonName.Contains("previous") || buttonName.Contains("prev"))
                {
                    tutorialPreviousButton = button;
                    Debug.Log("✅ Previous Button assigned: " + button.name);
                }
                else if (buttonName.Contains("skip"))
                {
                    tutorialSkipButton = button;
                    Debug.Log("✅ Skip Button assigned: " + button.name);
                }
                else if (buttonName.Contains("close") || buttonName.Contains("exit"))
                {
                    closeButton = button;
                    Debug.Log("✅ Close Button assigned: " + button.name);
                }
            }
        }
        
        // Find Progress Elements
        if (tutorialPanel != null)
        {
            Slider[] sliders = tutorialPanel.GetComponentsInChildren<Slider>();
            if (sliders.Length > 0)
            {
                progressSlider = sliders[0];
                Debug.Log("✅ Progress Slider assigned: " + progressSlider.name);
            }
            
            TextMeshProUGUI[] texts = tutorialPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.ToLower().Contains("progress") || text.name.ToLower().Contains("step"))
                {
                    progressText = text;
                    Debug.Log("✅ Progress Text assigned: " + text.name);
                    break;
                }
            }
        }
        
        Debug.Log("=== TUTORIAL UI REFERENCES ASSIGNMENT COMPLETE ===");
    }
    
    [ContextMenu("🔍 Check Tutorial UI Reference Status")]
    void CheckTutorialUIReferenceStatus()
    {
        Debug.Log("=== TUTORIAL UI REFERENCE STATUS CHECK ===");
        
        Debug.Log("Tutorial Panel Elements:");
        Debug.Log($"  Tutorial Panel: {(tutorialPanel != null ? "✅ " + tutorialPanel.name : "❌ Missing")}");
        Debug.Log($"  Tutorial Title: {(tutorialTitle != null ? "✅ " + tutorialTitle.name : "❌ Missing")}");
        Debug.Log($"  Tutorial Description: {(tutorialDescription != null ? "✅ " + tutorialDescription.name : "❌ Missing")}");
        Debug.Log($"  Tutorial Image: {(tutorialImage != null ? "✅ " + tutorialImage.name : "❌ Missing")}");
        
        Debug.Log("Navigation Buttons:");
        Debug.Log($"  Previous Button: {(tutorialPreviousButton != null ? "✅ " + tutorialPreviousButton.name : "❌ Missing")}");
        Debug.Log($"  Next Button: {(tutorialNextButton != null ? "✅ " + tutorialNextButton.name : "❌ Missing")}");
        Debug.Log($"  Skip Button: {(tutorialSkipButton != null ? "✅ " + tutorialSkipButton.name : "❌ Missing")}");
        Debug.Log($"  Close Button: {(closeButton != null ? "✅ " + closeButton.name : "❌ Missing")}");
        
        Debug.Log("Progress Elements:");
        Debug.Log($"  Progress Slider: {(progressSlider != null ? "✅ " + progressSlider.name : "❌ Missing")}");
        Debug.Log($"  Progress Text: {(progressText != null ? "✅ " + progressText.name : "❌ Missing")}");
        
        Debug.Log("Step GameObjects:");
        Debug.Log($"  Step 1 Object: {(step1Object != null ? "✅ " + step1Object.name : "❌ Missing")}");
        Debug.Log($"  Step 2 Object: {(step2Object != null ? "✅ " + step2Object.name : "❌ Missing")}");
        Debug.Log($"  Step 3 Object: {(step3Object != null ? "✅ " + step3Object.name : "❌ Missing")}");
        Debug.Log($"  Step 4 Object: {(step4Object != null ? "✅ " + step4Object.name : "❌ Missing")}");
        
        Debug.Log("=== TUTORIAL UI REFERENCE STATUS CHECK COMPLETE ===");
    }
    
    [ContextMenu("🎯 Auto-Assign Step GameObjects")]
    void AutoAssignStepGameObjects()
    {
        Debug.Log("=== AUTO-ASSIGNING STEP GAMEOBJECTS ===");
        
        // Find Step GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            
            if (objName.Contains("step 1") || objName.Contains("step1"))
            {
                step1Object = obj;
                Debug.Log("✅ Step 1 Object assigned: " + obj.name);
            }
            else if (objName.Contains("step 2") || objName.Contains("step2"))
            {
                step2Object = obj;
                Debug.Log("✅ Step 2 Object assigned: " + obj.name);
            }
            else if (objName.Contains("step 3") || objName.Contains("step3"))
            {
                step3Object = obj;
                Debug.Log("✅ Step 3 Object assigned: " + obj.name);
            }
            else if (objName.Contains("step 4") || objName.Contains("step4"))
            {
                step4Object = obj;
                Debug.Log("✅ Step 4 Object assigned: " + obj.name);
            }
        }
        
        Debug.Log("=== STEP GAMEOBJECTS ASSIGNMENT COMPLETE ===");
    }
    
    // Auto-generate complete tutorial UI
    [ContextMenu("🚀 Auto-Generate Complete Tutorial UI")]
    void AutoGenerateCompleteTutorialUI()
    {
        Debug.Log("=== AUTO-GENERATING COMPLETE TUTORIAL UI ===");
        
        if (tutorialPanel == null)
        {
            Debug.LogError("❌ Tutorial Panel not found! Please assign it first.");
            return;
        }
        
        // Generate all UI elements
        GenerateTutorialTitle();
        GenerateTutorialDescription();
        GenerateTutorialImage();
        GenerateNavigationButtons();
        GenerateProgressElements();
        
        // Auto-assign all references
        AutoAssignTutorialUIReferences();
        
        Debug.Log("✅ Complete Tutorial UI generated and assigned!");
        Debug.Log("🎨 You can now customize the appearance in the Inspector");
    }
    
    void GenerateTutorialTitle()
    {
        if (tutorialTitle != null) return; // Already exists
        
        GameObject titleObj = new GameObject("Tutorial Title");
        titleObj.transform.SetParent(tutorialPanel.transform, false);
        
        // Add TextMeshPro component
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Welcome to Practice Mode!";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Position at top
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(20, -80);
        titleRect.offsetMax = new Vector2(-20, -20);
        
        tutorialTitle = titleText;
        Debug.Log("✅ Tutorial Title generated");
    }
    
    void GenerateTutorialDescription()
    {
        if (tutorialDescription != null) return; // Already exists
        
        GameObject descObj = new GameObject("Tutorial Description");
        descObj.transform.SetParent(tutorialPanel.transform, false);
        
        // Add TextMeshPro component
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Step 1: Learn how equations work with + and - operations. You'll need to answer questions like this!";
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = Color.white;
        descText.enableWordWrapping = true;
        
        // Position below title
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 1);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.offsetMin = new Vector2(20, -200);
        descRect.offsetMax = new Vector2(-20, -100);
        
        tutorialDescription = descText;
        Debug.Log("✅ Tutorial Description generated");
    }
    
    void GenerateTutorialImage()
    {
        if (tutorialImage != null) return; // Already exists
        
        GameObject imageObj = new GameObject("Tutorial Image");
        imageObj.transform.SetParent(tutorialPanel.transform, false);
        
        // Add Image component
        Image image = imageObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Semi-transparent dark
        
        // Position in center
        RectTransform imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.sizeDelta = new Vector2(200, 150);
        imageRect.anchoredPosition = Vector2.zero;
        
        tutorialImage = image;
        Debug.Log("✅ Tutorial Image generated (placeholder - you can assign a sprite later)");
    }
    
    void GenerateNavigationButtons()
    {
        // Previous Button
        if (tutorialPreviousButton == null)
        {
            GameObject prevBtn = CreateButton("Previous Button", "← Previous", new Vector2(0.2f, 0.1f));
            tutorialPreviousButton = prevBtn.GetComponent<Button>();
            Debug.Log("✅ Previous Button generated");
        }
        
        // Next Button
        if (tutorialNextButton == null)
        {
            GameObject nextBtn = CreateButton("Next Button", "Next →", new Vector2(0.8f, 0.1f));
            tutorialNextButton = nextBtn.GetComponent<Button>();
            Debug.Log("✅ Next Button generated");
        }
        
        // Skip Button
        if (tutorialSkipButton == null)
        {
            GameObject skipBtn = CreateButton("Skip Button", "Skip Tutorial", new Vector2(0.2f, 0.05f));
            tutorialSkipButton = skipBtn.GetComponent<Button>();
            Debug.Log("✅ Skip Button generated");
        }
        
        // Close Button
        if (closeButton == null)
        {
            GameObject closeBtn = CreateButton("Close Button", "Close", new Vector2(0.8f, 0.05f));
            closeButton = closeBtn.GetComponent<Button>();
            Debug.Log("✅ Close Button generated");
        }
    }
    
    GameObject CreateButton(string buttonName, string buttonText, Vector2 anchorPosition)
    {
        GameObject buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(tutorialPanel.transform, false);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.6f, 1f, 1f); // Blue color
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonTextComponent = textObj.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = buttonText;
        buttonTextComponent.fontSize = 16;
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
        buttonTextComponent.color = Color.white;
        
        // Position text in button center
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Position button
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorPosition;
        buttonRect.anchorMax = anchorPosition;
        buttonRect.sizeDelta = new Vector2(120, 40);
        buttonRect.anchoredPosition = Vector2.zero;
        
        return buttonObj;
    }
    
    void GenerateProgressElements()
    {
        // Progress Slider
        if (progressSlider == null)
        {
            GameObject sliderObj = new GameObject("Progress Slider");
            sliderObj.transform.SetParent(tutorialPanel.transform, false);
            
            // Add Slider component
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.25f; // 1/4 steps
            
            // Position at bottom
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.1f, 0.15f);
            sliderRect.anchorMax = new Vector2(0.9f, 0.15f);
            sliderRect.sizeDelta = new Vector2(0, 20);
            sliderRect.anchoredPosition = Vector2.zero;
            
            progressSlider = slider;
            Debug.Log("✅ Progress Slider generated");
        }
        
        // Progress Text
        if (progressText == null)
        {
            GameObject progressTextObj = new GameObject("Progress Text");
            progressTextObj.transform.SetParent(tutorialPanel.transform, false);
            
            // Add TextMeshPro component
            TextMeshProUGUI progressTextComponent = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressTextComponent.text = "1 / 4";
            progressTextComponent.fontSize = 14;
            progressTextComponent.alignment = TextAlignmentOptions.Center;
            progressTextComponent.color = Color.white;
            
            // Position above slider
            RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0.5f, 0.15f);
            progressTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressTextRect.sizeDelta = new Vector2(100, 30);
            progressTextRect.anchoredPosition = new Vector2(0, 25);
            
                         progressText = progressTextComponent;
             Debug.Log("✅ Progress Text generated");
         }
     }
    
    // Progressive Tutorial System - 4 Steps as requested
    [ContextMenu("🎯 Start Progressive Tutorial")]
    void StartProgressiveTutorial()
    {
        Debug.Log("=== STARTING PROGRESSIVE TUTORIAL ===");
        currentTutorialStep = 0;
        tutorialCompleted = false;
        ShowTutorial();
        ShowTutorialStep(0);
    }
    
    void ShowTutorialStep(int step)
    {
        currentTutorialStep = step;
        
        switch (step)
        {
            case 0: // Step 1: Equation Focus
                ShowStep1_EquationFocus();
                break;
            case 1: // Step 2: Movement Controls
                ShowStep2_MovementControls();
                break;
            case 2: // Step 3: AI Assistance
                ShowStep3_AIAssistance();
                break;
            case 3: // Step 4: Reward System
                ShowStep4_RewardSystem();
                break;
            default:
                CompleteTutorial();
                break;
        }
        
        UpdateProgress();
    }
    
    void ShowStep1_EquationFocus()
    {
        Debug.Log("🎯 STEP 1: Equation Focus");
        
        // Update tutorial content - NO "Welcome to Practice Mode" text as requested
        if (tutorialTitle != null)
            tutorialTitle.text = "Step 1: Understanding Equations";
        
        // Use TypewriterEffect instead of directly setting text
        if (tutorialDescription != null)
        {
            TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
            if (typewriter != null)
            {
                Debug.Log($"🔍 TypewriterEffect found on {typewriter.name}");
                Debug.Log($"🔍 Current targetText: {(typewriter.targetText != null ? typewriter.targetText.name : "NULL")}");
                Debug.Log($"🔍 Tutorial Description: {tutorialDescription.name}");
                
                // CRITICAL: Set the targetText field to point to tutorialDescription
                typewriter.targetText = tutorialDescription;
                Debug.Log($"🔍 Set targetText to: {typewriter.targetText.name}");
                
                // Clear the text first to ensure typewriter effect is visible
                tutorialDescription.text = "";
                
                // Now call PlayTypewriter - it will type to the correct text component
                typewriter.PlayTypewriter("Focus on the equation! Learn how + and - operations work. You'll need to answer questions like this!");
                Debug.Log("✅ Using TypewriterEffect for Step 1 description");
            }
            else
            {
                // Fallback to direct text if no typewriter effect
                tutorialDescription.text = "Focus on the equation! Learn how + and - operations work. You'll need to answer questions like this!";
                Debug.LogWarning("⚠️ No TypewriterEffect found, using direct text");
            }
        }
        
        // Focus on equation area - try multiple possible names
        if (!FocusOnObject("Equation") && !FocusOnObject("EquationText") && !FocusOnObject("QuestionText"))
        {
            Debug.LogWarning("⚠️ Could not find equation object. Please ensure it's named 'Equation', 'EquationText', or 'QuestionText'");
        }
        
        // Set button states for Step 1
        SetButtonStates(false, true); // No Previous, Yes Next
        
        Debug.Log("✅ Step 1: Equation focus set up");
    }
    
    void ShowStep2_MovementControls()
    {
        Debug.Log("🎯 STEP 2: Movement Controls");
        
        if (tutorialTitle != null)
            tutorialTitle.text = "Step 2: Movement Controls";
        
        // Use TypewriterEffect instead of directly setting text
        if (tutorialDescription != null)
        {
            TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
            if (typewriter != null)
            {
                // CRITICAL: Set the targetText field to point to tutorialDescription
                typewriter.targetText = tutorialDescription;
                
                typewriter.PlayTypewriter("Press LEFT and RIGHT buttons to move forward and backward through questions!");
                Debug.Log("✅ Using TypewriterEffect for Step 2 description");
            }
            else
            {
                tutorialDescription.text = "Press LEFT and RIGHT buttons to move forward and backward through questions!";
                Debug.LogWarning("⚠️ No TypewriterEffect found, using direct text");
            }
        }
        
        // Focus on left/right buttons - try multiple possible names
        if (!FocusOnObject("LeftButton") && !FocusOnObject("Left") && !FocusOnObject("PreviousButton"))
        {
            Debug.LogWarning("⚠️ Could not find left button. Please ensure it's named 'LeftButton', 'Left', or 'PreviousButton'");
        }
        
        // Set button states for Step 2
        SetButtonStates(true, true); // Yes Previous, Yes Next
        
        Debug.Log("✅ Step 2: Movement controls set up");
    }
    
    void ShowStep3_AIAssistance()
    {
        Debug.Log("🎯 STEP 3: AI Assistance");
        
        if (tutorialTitle != null)
            tutorialTitle.text = "Step 3: AI Assistance";
        
        // Use TypewriterEffect instead of directly setting text
        if (tutorialDescription != null)
        {
            TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
            if (typewriter != null)
            {
                // CRITICAL: Set the targetText field to point to tutorialDescription
                typewriter.targetText = tutorialDescription;
                
                typewriter.PlayTypewriter("AI Assistance helps you stay motivated! Press the Help button next to AI Assistance to reveal answers when you're stuck!");
                Debug.Log("✅ Using TypewriterEffect for Step 3 description");
            }
            else
            {
                tutorialDescription.text = "AI Assistance helps you stay motivated! Press the Help button next to AI Assistance to reveal answers when you're stuck!";
                Debug.LogWarning("⚠️ No TypewriterEffect found, using direct text");
            }
        }
        
        // Focus on AI Assistance area and Help button - try multiple possible names
        if (!FocusOnObject("AIAssistance") && !FocusOnObject("AIHelp") && !FocusOnObject("HelpButton"))
        {
            Debug.LogWarning("⚠️ Could not find AI Assistance. Please ensure it's named 'AIAssistance', 'AIHelp', or 'HelpButton'");
        }
        
        // CRITICAL: Ensure AI Assistance doesn't run before this step
        // You can add logic here to disable AI Assistance functionality
        
        // Set button states for Step 3
        SetButtonStates(true, true); // Yes Previous, Yes Next
        
        Debug.Log("✅ Step 3: AI Assistance set up");
    }
    
    void ShowStep4_RewardSystem()
    {
        Debug.Log("🎯 STEP 4: Reward System");
        
        if (tutorialTitle != null)
            tutorialTitle.text = "Step 4: Rewards";
        
        // Use TypewriterEffect instead of directly setting text
        if (tutorialDescription != null)
        {
            TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
            if (typewriter != null)
            {
                // CRITICAL: Set the targetText field to point to tutorialDescription
                typewriter.targetText = tutorialDescription;
                
                typewriter.PlayTypewriter("Open any reward button! Press the button to see your reward!");
                Debug.Log("✅ Using TypewriterEffect for Step 4 description");
            }
            else
            {
                tutorialDescription.text = "Open any reward button! Press the button to see your reward!";
                Debug.LogWarning("⚠️ No TypewriterEffect found, using direct text");
            }
        }
        
        // Focus on reward buttons - try multiple possible names
        if (!FocusOnObject("RewardButton") && !FocusOnObject("HouseButton") && !FocusOnObject("PetButton"))
        {
            Debug.LogWarning("⚠️ Could not find reward button. Please ensure it's named 'RewardButton', 'HouseButton', or 'PetButton'");
        }
        
        // Set button states for Step 4
        SetButtonStates(true, true); // Yes Previous, Yes Next (Next will complete tutorial)
        
        Debug.Log("✅ Step 4: Reward system set up");
    }
    
    void ShowRewardAndComplete()
    {
        Debug.Log("🎁 Showing reward and completing tutorial...");
        
        // Instantiate house reward
        CreateTutorialHouseReward();
        
        // Wait 5 seconds then show completion message
        StartCoroutine(ShowCompletionMessage());
    }
    
    void CreateTutorialHouseReward()
    {
        if (tutorialHouseReward != null) return;
        
        // Create a simple house reward (you can replace this with your actual house prefab)
        GameObject house = new GameObject("Tutorial House Reward");
        house.transform.SetParent(tutorialPanel.transform, false);
        
        // Add a simple visual representation
        Image houseImage = house.AddComponent<Image>();
        houseImage.color = new Color(1f, 0.8f, 0.6f, 1f); // Light brown
        
        // Position in center
        RectTransform houseRect = house.GetComponent<RectTransform>();
        houseRect.anchorMin = new Vector2(0.5f, 0.5f);
        houseRect.anchorMax = new Vector2(0.5f, 0.5f);
        houseRect.sizeDelta = new Vector2(100, 80);
        houseRect.anchoredPosition = Vector2.zero;
        
        tutorialHouseReward = house;
        Debug.Log("🏠 Tutorial house reward created!");
    }
    
    System.Collections.IEnumerator ShowCompletionMessage()
    {
        yield return new WaitForSeconds(5f);
        
        Debug.Log("🎉 Congratulations! Your training is complete!");
        
        if (tutorialTitle != null)
            tutorialTitle.text = "🎉 Training Complete!";
        
        // Use TypewriterEffect for completion message
        if (tutorialDescription != null)
        {
            TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
            if (typewriter != null)
            {
                // CRITICAL: Set the targetText field to point to tutorialDescription
                typewriter.targetText = tutorialDescription;
                
                typewriter.PlayTypewriter("Congratulations! Your training is complete! You're free to play now!");
                Debug.Log("✅ Using TypewriterEffect for completion message");
            }
            else
            {
                tutorialDescription.text = "Congratulations! Your training is complete! You're free to play now!";
                Debug.LogWarning("⚠️ No TypewriterEffect found, using direct text for completion");
            }
        }
        
        // Hide all buttons except close
        if (tutorialPreviousButton != null) tutorialPreviousButton.gameObject.SetActive(false);
        if (tutorialNextButton != null) tutorialNextButton.gameObject.SetActive(false);
        if (tutorialSkipButton != null) tutorialSkipButton.gameObject.SetActive(false);
        
        // Show close button
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                CompleteTutorial();
                HideTutorial();
            });
        }
        
        tutorialCompleted = true;
    }
    
    bool FocusOnObject(string objectName)
    {
        Debug.Log($"🎯 Attempting to focus on: {objectName}");
        
        // Remove previous focus
        HideCurrentFocus();
        
        // Find the object to focus on
        GameObject focusObj = GameObject.Find(objectName);
        if (focusObj != null)
        {
            Debug.Log($"✅ Found object: {focusObj.name} at position {focusObj.transform.position}");
            currentFocusObject = focusObj;
            
            // Move and resize Tutorial Image to highlight this object
            if (tutorialImage != null)
            {
                Debug.Log($"✅ Tutorial Image found, attempting to position it");
                
                // Get the target object's position and size
                RectTransform targetRect = focusObj.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    Debug.Log($"✅ Target has RectTransform - Position: {targetRect.position}, Size: {targetRect.sizeDelta}");
                    
                    // Move tutorial image to match target position
                    RectTransform imageRect = tutorialImage.GetComponent<RectTransform>();
                    if (imageRect != null)
                    {
                        // Store original position for debugging
                        Vector3 originalPos = imageRect.position;
                        Vector2 originalSize = imageRect.sizeDelta;
                        
                        Debug.Log($"📏 Tutorial Image original - Position: {originalPos}, Size: {originalSize}");
                        
                        // Move tutorial image to match target position
                        imageRect.position = targetRect.position;
                        
                        // Resize tutorial image to match target size (with some padding)
                        Vector2 targetSize = targetRect.sizeDelta;
                        imageRect.sizeDelta = new Vector2(targetSize.x + 20, targetSize.y + 20);
                        
                        Debug.Log($"🎯 Tutorial Image moved to - Position: {imageRect.position}, Size: {imageRect.sizeDelta}");
                        
                        // Make image visible and add highlight effect
                        tutorialImage.gameObject.SetActive(true);
                        tutorialImage.color = new Color(1f, 1f, 0f, 0.5f); // More visible yellow highlight
                        
                        Debug.Log($"✅ Tutorial Image successfully positioned and highlighted!");
                        return true; // Success
                    }
                    else
                    {
                        Debug.LogError($"❌ Tutorial Image has no RectTransform component!");
                        return false;
                    }
                }
                else
                {
                    Debug.Log($"⚠️ Target has no RectTransform, trying world position");
                    
                    // If no RectTransform, try to use world position
                    Vector3 targetPos = focusObj.transform.position;
                    tutorialImage.transform.position = targetPos;
                    tutorialImage.gameObject.SetActive(true);
                    tutorialImage.color = new Color(1f, 1f, 0f, 0.5f);
                    
                    Debug.Log($"🎯 Tutorial Image moved to world position: {targetPos}");
                    return true; // Success
                }
            }
            else
            {
                Debug.LogError($"❌ Tutorial Image not assigned - cannot show visual focus");
                return false; // Failure
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Could not find object to focus: {objectName}");
            
            // Try to find similar named objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            Debug.Log($"🔍 Searching through {allObjects.Length} objects in scene...");
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains(objectName.ToLower()))
                {
                    Debug.Log($"💡 Found similar object: {obj.name}");
                }
            }
            
            return false; // Failure
        }
    }
    
    void HideCurrentFocus()
    {
        if (currentFocusObject != null)
        {
            Debug.Log($"👁️ Removing focus from: {currentFocusObject.name}");
            currentFocusObject = null;
        }
        
        // Hide the tutorial image when removing focus
        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }
    }
    
    void CompleteTutorial()
    {
        Debug.Log("✅ Tutorial completed successfully!");
        tutorialCompleted = true;
        
        // Reset button states
        if (tutorialPreviousButton != null) tutorialPreviousButton.gameObject.SetActive(true);
        if (tutorialNextButton != null) tutorialNextButton.gameObject.SetActive(true);
        if (tutorialSkipButton != null) tutorialSkipButton.gameObject.SetActive(true);
        
        // Reset tutorial step
        currentTutorialStep = 0;
    }
    
    // Simple tutorial methods for testing (keeping these as backup)
    [ContextMenu("🚀 Show Simple Tutorial")]
    void ShowSimpleTutorial()
    {
        if (tutorialTitle != null)
            tutorialTitle.text = "Welcome to Practice Mode!";
        
        if (tutorialDescription != null)
            tutorialDescription.text = "This is Practice Mode with tutorial system. Use the buttons to navigate!";
        
        ShowTutorial();
        UpdateProgress();
    }
    
    [ContextMenu("🔒 Hide Simple Tutorial")]
    void HideSimpleTutorial()
    {
        HideTutorial();
    }
    
    [ContextMenu("🎯 Test Tutorial Image Visibility")]
    void TestTutorialImageVisibility()
    {
        Debug.Log("=== TESTING TUTORIAL IMAGE VISIBILITY ===");
        
        if (tutorialImage == null)
        {
            Debug.LogError("❌ Tutorial Image not assigned! Please assign it first.");
            return;
        }
        
        // Show the tutorial image in a fixed position
        tutorialImage.gameObject.SetActive(true);
        tutorialImage.color = new Color(1f, 0f, 0f, 0.8f); // Bright red for testing
        
        // Position it in the center of the screen
        RectTransform imageRect = tutorialImage.GetComponent<RectTransform>();
        if (imageRect != null)
        {
            imageRect.anchoredPosition = Vector2.zero; // Center
            imageRect.sizeDelta = new Vector2(200, 150); // Fixed size
            
            Debug.Log("✅ Tutorial Image should now be visible in center with bright red color!");
            Debug.Log($"   Position: {imageRect.anchoredPosition}, Size: {imageRect.sizeDelta}");
        }
        else
        {
            Debug.LogError("❌ Tutorial Image has no RectTransform!");
        }
    }
    
    [ContextMenu("🎯 Hide Tutorial Image")]
    void HideTutorialImage()
    {
        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
            Debug.Log("✅ Tutorial Image hidden!");
        }
    }
    
    // Test Typewriter Effect directly
    [ContextMenu("⌨️ Test Typewriter Effect")]
    void TestTypewriterEffect()
    {
        Debug.Log("=== TESTING TYPEWRITER EFFECT ===");
        
        if (tutorialDescription == null)
        {
            Debug.LogError("❌ Tutorial Description not assigned!");
            return;
        }
        
        TypewriterEffect typewriter = tutorialDescription.GetComponent<TypewriterEffect>();
        if (typewriter == null)
        {
            Debug.LogError("❌ No TypewriterEffect found on Tutorial Description!");
            return;
        }
        
        Debug.Log($"🔍 TypewriterEffect found on {typewriter.name}");
        Debug.Log($"🔍 Current targetText: {(typewriter.targetText != null ? typewriter.targetText.name : "NULL")}");
        Debug.Log($"🔍 Tutorial Description: {tutorialDescription.name}");
        
        // Set targetText to tutorialDescription
        typewriter.targetText = tutorialDescription;
        Debug.Log($"🔍 Set targetText to: {typewriter.targetText.name}");
        
        // Clear text first
        tutorialDescription.text = "";
        
        // Test the typewriter effect
        typewriter.PlayTypewriter("This is a test of the typewriter effect! It should type out character by character.");
        Debug.Log("✅ Typewriter effect test started!");
    }
}
