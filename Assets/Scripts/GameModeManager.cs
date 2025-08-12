using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    public string mobileSceneName = "MobileGame";
    public string arSceneName = "ARGame";
    public string practiceSceneName = "PracticeGame";
    
    [Header("Audio")]
    public AudioManager audioManager;
    
    void Start()
    {
        // Find audio manager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
    }
    
    // Simple scene loading methods
    public void LoadMobileGame()
    {
        PlayButtonSound();
        Debug.Log($"Loading Mobile Game: {mobileSceneName}");
        SceneManager.LoadScene(mobileSceneName);
    }
    
    public void LoadARGame()
    {
        PlayButtonSound();
        Debug.Log($"Loading AR Game: {arSceneName}");
        SceneManager.LoadScene(arSceneName);
    }
    
    public void LoadPracticeGame()
    {
        PlayButtonSound();
        Debug.Log($"Loading Practice Game: {practiceSceneName}");
        SceneManager.LoadScene(practiceSceneName);
    }
    
    public void LoadMainMenu()
    {
        PlayButtonSound();
        Debug.Log("Loading Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
    
    void PlayButtonSound()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("button_click");
        }
    }
    
    // Context menu for testing
    [ContextMenu("Test Load Mobile Game")]
    void TestLoadMobileGame()
    {
        Debug.Log("=== TESTING MOBILE GAME LOAD ===");
        LoadMobileGame();
    }
    
    [ContextMenu("Test Load AR Game")]
    void TestLoadARGame()
    {
        Debug.Log("=== TESTING AR GAME LOAD ===");
        LoadARGame();
    }
    
    [ContextMenu("Test Load Practice Game")]
    void TestLoadPracticeGame()
    {
        Debug.Log("=== TESTING PRACTICE GAME LOAD ===");
        LoadPracticeGame();
    }
    
    [ContextMenu("Test Load Main Menu")]
    void TestLoadMainMenu()
    {
        Debug.Log("=== TESTING MAIN MENU LOAD ===");
        LoadMainMenu();
    }
} 