using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button mobileGameButton; // "Mobile Game Button"
    public Button practiseButton;   // "Practise Button"
    public Button arGameButton;     // "AR Game Button"
    public Button settingsButton;   // "Settings Button"
    public Button exitButton;       // "Exit Button"
    
    [Header("Fade Effect")]
    public Image fadeImage; // Your existing fade image
    
    [Header("Scene Names")]
    public string mobileSceneName = "MobileGame";
    public string practiseSceneName = "PractiseGame";
    public string arSceneName = "ARGame";
    
    [Header("Audio")]
    public AudioManager audioManager;
    
    void Start()
    {
        InitializeMainMenu();
    }
    
    void InitializeMainMenu()
    {
        Debug.Log("=== MAIN MENU INITIALIZATION ===");
        
        // Find components if not assigned
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        
        // Initialize fade image
        InitializeFadeImage();
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Check AR support and update UI
        CheckARSupport();
        
        // Play background music
        if (audioManager != null)
        {
            audioManager.PlayMusic("menu_music");
        }
        
        Debug.Log("Main Menu initialized");
    }
    
    void InitializeFadeImage()
    {
        if (fadeImage != null)
        {
            // Start with fade image completely transparent
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            
            // Make sure fade image is active but transparent
            fadeImage.gameObject.SetActive(true);
            
            Debug.Log("Fade image initialized - transparent");
        }
        else
        {
            Debug.LogWarning("Fade image not assigned!");
        }
    }
    
    void SetupButtonListeners()
    {
        if (mobileGameButton != null)
        {
            mobileGameButton.onClick.AddListener(OnMobileGameSelected);
        }
        
        if (practiseButton != null)
        {
            practiseButton.onClick.AddListener(OnPractiseSelected);
        }
        
        if (arGameButton != null)
        {
            arGameButton.onClick.AddListener(OnARGameSelected);
        }
    }
    
    void CheckARSupport()
    {
        bool arSupported = IsARSupported();
        
        // Update AR button state
        if (arGameButton != null)
        {
            arGameButton.interactable = arSupported;
        }
        
        Debug.Log($"AR Support: {arSupported}");
    }
    
    bool IsARSupported()
    {
        // For now, always return true to enable AR button
        // You can add proper AR detection later
        return true;
        
        // Check if AR Foundation is available
        // #if UNITY_XR_ARFOUNDATION
        //     return true;
        // #else
        //     return false;
        // #endif
    }
    
    public void OnMobileGameSelected()
    {
        Debug.Log("Mobile Game selected");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Save mode preference
        PlayerPrefs.SetString("GameMode", "Mobile");
        PlayerPrefs.Save();
        
        // Load mobile scene with fade effect
        LoadSceneWithFade(mobileSceneName);
    }
    
    public void OnPractiseSelected()
    {
        Debug.Log("Practise mode selected");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Save mode preference
        PlayerPrefs.SetString("GameMode", "Practise");
        PlayerPrefs.Save();
        
        // Load practise scene with fade effect (can be same as mobile for now)
        LoadSceneWithFade(practiseSceneName);
    }
    
    public void OnARGameSelected()
    {
        Debug.Log("AR Game selected");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Save mode preference
        PlayerPrefs.SetString("GameMode", "AR");
        PlayerPrefs.Save();
        
        // Load AR scene with fade effect
        LoadSceneWithFade(arSceneName);
    }
    
    void LoadSceneWithFade(string sceneName)
    {
        Debug.Log($"Loading {sceneName} scene with fade effect...");
        StartCoroutine(FadeToScene(sceneName));
    }
    
    IEnumerator FadeToScene(string sceneName)
    {
        // Fade in
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color color = fadeImage.color;
            
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed);
                fadeImage.color = color;
                yield return null;
            }
        }
        
        // Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // TODO: Show settings panel
        // You can implement settings UI here
    }
    
    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
        
        // Quit application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void OnBackToMainMenuClicked()
    {
        Debug.Log("Back to main menu clicked");
        
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
} 