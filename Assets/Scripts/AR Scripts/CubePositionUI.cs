using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CubePositionUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI positionText;
    public GameObject positionPanel;
    
    [Header("Display Settings")]
    public bool showPositionUI = true;
    public bool startVisible = true; // NEW: Control initial visibility
    public string notOnCubeText = "Not on any cube";
    public string onCubeText = "Standing on: {0}";
    
    [Header("Animation")]
    public bool enableAnimations = true;
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 0.5f;
    
    private ARPlayerController playerController;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    void Start()
    {
        InitializeUI();
        FindPlayerController();
    }
    
    void InitializeUI()
    {
        // Find UI components if not assigned
        if (positionText == null)
        {
            positionText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (positionPanel == null)
        {
            positionPanel = transform.gameObject;
        }
        
        // Get or add CanvasGroup for animations
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set initial visibility based on startVisible setting
        SetUIVisibility(startVisible);
        
        // Set initial text if starting visible
        if (startVisible && positionText != null)
        {
            positionText.text = notOnCubeText;
        }
    }
    
    void FindPlayerController()
    {
        playerController = FindObjectOfType<ARPlayerController>();
        if (playerController != null)
        {
            // Subscribe to cube detection events
            if (playerController.numberLineGenerator != null)
            {
                playerController.numberLineGenerator.OnPlayerEnteredCube += OnPlayerEnteredCube;
                playerController.numberLineGenerator.OnPlayerExitedCube += OnPlayerExitedCube;
            }
        }
        else
        {
            Debug.LogWarning("ARPlayerController not found - cube position UI will not work");
        }
    }
    
    void OnPlayerEnteredCube(int cubeNumber)
    {
        if (!showPositionUI) return;
        
        // Update the text
        if (positionText != null)
        {
            positionText.text = string.Format(onCubeText, cubeNumber);
        }
        
        // Show the UI with animation
        SetUIVisibility(true);
        
        Debug.Log($"UI: Player entered cube {cubeNumber}");
    }
    
    void OnPlayerExitedCube()
    {
        if (!showPositionUI) return;
        
        // Update the text
        if (positionText != null)
        {
            positionText.text = notOnCubeText;
        }
        
        // Hide the UI with animation
        SetUIVisibility(false);
        
        Debug.Log("UI: Player exited cube");
    }
    
    void SetUIVisibility(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (enableAnimations)
        {
            // Animate the visibility change using Unity's built-in system
            if (visible)
            {
                // Fade in
                canvasGroup.alpha = 0f;
                positionPanel.SetActive(true);
                StartCoroutine(FadeIn(fadeInTime));
            }
            else
            {
                // Fade out
                StartCoroutine(FadeOut(fadeOutTime));
            }
        }
        else
        {
            // No animation
            positionPanel.SetActive(visible);
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
    
    // Public method to manually update the display
    public void UpdateDisplay(int cubeNumber)
    {
        if (cubeNumber == -999)
        {
            OnPlayerExitedCube();
        }
        else
        {
            OnPlayerEnteredCube(cubeNumber);
        }
    }
    
    [ContextMenu("Debug UI Visibility Issues")]
    void DebugUIVisibilityIssues()
    {
        Debug.Log("=== DEBUGGING UI VISIBILITY ISSUES ===");
        Debug.Log($"This script is attached to: {gameObject.name}");
        Debug.Log($"GameObject active in hierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"GameObject active self: {gameObject.activeSelf}");
        
        // Check CanvasGroup
        if (canvasGroup != null)
        {
            Debug.Log($"CanvasGroup alpha: {canvasGroup.alpha}");
            Debug.Log($"CanvasGroup interactable: {canvasGroup.interactable}");
            Debug.Log($"CanvasGroup blocksRaycasts: {canvasGroup.blocksRaycasts}");
        }
        else
        {
            Debug.LogError("❌ CanvasGroup is null!");
        }
        
        // Check positionPanel
        if (positionPanel != null)
        {
            Debug.Log($"PositionPanel active: {positionPanel.activeInHierarchy}");
            Debug.Log($"PositionPanel active self: {positionPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("❌ PositionPanel is null!");
        }
        
        // Check positionText
        if (positionText != null)
        {
            Debug.Log($"PositionText active: {positionText.gameObject.activeInHierarchy}");
            Debug.Log($"PositionText enabled: {positionText.enabled}");
            Debug.Log($"Current text: {positionText.text}");
        }
        else
        {
            Debug.LogError("❌ PositionText is null! Run 'Auto-Assign TextMeshPro Reference' first.");
        }
        
        // Check settings
        Debug.Log($"showPositionUI: {showPositionUI}");
        Debug.Log($"startVisible: {startVisible}");
        Debug.Log($"isVisible: {isVisible}");
        
        // Try to fix common issues
        Debug.Log("\n=== ATTEMPTING TO FIX COMMON ISSUES ===");
        
        if (canvasGroup != null && canvasGroup.alpha == 0f)
        {
            Debug.Log("🔧 Fixing: CanvasGroup alpha was 0, setting to 1");
            canvasGroup.alpha = 1f;
        }
        
        if (positionPanel != null && !positionPanel.activeInHierarchy)
        {
            Debug.Log("🔧 Fixing: PositionPanel was inactive, activating");
            positionPanel.SetActive(true);
        }
        
        if (positionText != null && !positionText.gameObject.activeInHierarchy)
        {
            Debug.Log("🔧 Fixing: PositionText was inactive, activating");
            positionText.gameObject.SetActive(true);
        }
        
        // Force show the UI
        Debug.Log("🔧 Forcing UI to be visible...");
        SetUIVisibility(true);
        
        Debug.Log("✅ Debug and fix attempt completed!");
    }
    
    [ContextMenu("Force Show UI")]
    void ForceShowUI()
    {
        Debug.Log("=== FORCING UI TO SHOW ===");
        
        // Force all components to be active and visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (positionPanel != null)
        {
            positionPanel.SetActive(true);
        }
        
        if (positionText != null)
        {
            positionText.gameObject.SetActive(true);
            positionText.enabled = true;
            positionText.text = "UI FORCED TO SHOW!";
        }
        
        isVisible = true;
        Debug.Log("✅ UI forced to show!");
    }

    [ContextMenu("Toggle Start Visible Setting")]
    void ToggleStartVisible()
    {
        startVisible = !startVisible;
        Debug.Log($"Start Visible setting changed to: {startVisible}");
        
        // Apply the change immediately
        SetUIVisibility(startVisible);
        if (startVisible && positionText != null)
        {
            positionText.text = notOnCubeText;
        }
    }
    
    [ContextMenu("Show Current UI State")]
    void ShowCurrentUIState()
    {
        Debug.Log("=== CURRENT UI STATE ===");
        Debug.Log($"showPositionUI: {showPositionUI}");
        Debug.Log($"startVisible: {startVisible}");
        Debug.Log($"isVisible: {isVisible}");
        Debug.Log($"positionText assigned: {positionText != null}");
        if (positionText != null)
        {
            Debug.Log($"Current text: {positionText.text}");
            Debug.Log($"Text active: {positionText.gameObject.activeInHierarchy}");
        }
        Debug.Log($"positionPanel active: {positionPanel.activeInHierarchy}");
    }

    [ContextMenu("Test TextMeshPro Functionality")]
    void TestTextMeshProFunctionality()
    {
        if (positionText == null)
        {
            Debug.LogError("❌ positionText is null! Run 'Auto-Assign TextMeshPro Reference' first.");
            return;
        }
        
        Debug.Log("=== STARTING VISIBLE TEXTMESHPRO TESTS ===");
        StartCoroutine(TestTextMeshProWithDelays());
    }
    
    IEnumerator TestTextMeshProWithDelays()
    {
        Debug.Log($"✅ positionText assigned to: {positionText.name}");
        
        // Test 1: Basic text update
        Debug.Log("Test 1: Setting basic text...");
        positionText.text = "Test 1: Basic text works!";
        positionText.color = Color.white;
        positionText.fontSize = 24;
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        
        // Test 2: Number display
        Debug.Log("Test 2: Setting number display...");
        positionText.text = "Standing on: 5";
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        
        // Test 3: Color change to green
        Debug.Log("Test 3: Changing to GREEN...");
        positionText.color = Color.green;
        positionText.text = "GREEN TEXT!";
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        
        // Test 4: Color change to red
        Debug.Log("Test 4: Changing to RED...");
        positionText.color = Color.red;
        positionText.text = "RED TEXT!";
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        
        // Test 5: Font size change
        Debug.Log("Test 5: Making text BIGGER...");
        positionText.fontSize = 48;
        positionText.text = "BIG TEXT!";
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        
        // Test 6: Reset to normal
        Debug.Log("Test 6: Resetting to normal...");
        positionText.color = Color.white;
        positionText.fontSize = 24;
        positionText.text = "TextMeshPro test complete!";
        
        Debug.Log("✅ All TextMeshPro tests completed!");
        Debug.Log("You should have seen: White → Green → Red → Big → Normal");
    }

    [ContextMenu("Auto-Assign TextMeshPro Reference")]
    void AutoAssignTextMeshProReference()
    {
        Debug.Log("=== AUTO-ASSIGNING TEXTMESHPRO REFERENCE ===");
        
        // Try to find TextMeshProUGUI with name "Cube number"
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        TextMeshProUGUI targetText = null;
        
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.name.Contains("Cube number") || text.name.Contains("Cube Number"))
            {
                targetText = text;
                Debug.Log($"✅ Found TextMeshProUGUI: {text.name}");
                break;
            }
        }
        
        if (targetText == null)
        {
            // If not found by name, try to find any TextMeshProUGUI in the scene
            if (allTexts.Length > 0)
            {
                targetText = allTexts[0];
                Debug.Log($"⚠️ Using first available TextMeshProUGUI: {targetText.name}");
            }
        }
        
        if (targetText != null)
        {
            positionText = targetText;
            Debug.Log($"✅ Successfully assigned TextMeshProUGUI reference: {targetText.name}");
            
            // Test the assignment
            targetText.text = "Reference assigned successfully!";
            Debug.Log("✅ Test text set - you should see this on screen");
        }
        else
        {
            Debug.LogError("❌ No TextMeshProUGUI found in the scene!");
            Debug.LogError("Make sure you have a TextMeshProUGUI component in your Canvas");
        }
    }

    [ContextMenu("Test UI - Show Cube 5")]
    void TestShowCube5()
    {
        OnPlayerEnteredCube(5);
    }
    
    [ContextMenu("Test UI - Hide")]
    void TestHide()
    {
        OnPlayerExitedCube();
    }
    
    // Animation coroutines
    IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        positionPanel.SetActive(false);
    }
} 