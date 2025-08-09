using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("UI References")]
    public CanvasGroup fadeCanvasGroup;
    public UnityEngine.UI.Image fadeImage;
    
    private static GameSceneManager instance;
    public static GameSceneManager Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupFadeUI()
    {
        // Create fade UI if it doesn't exist
        if (fadeCanvasGroup == null)
        {
            GameObject fadeGO = new GameObject("FadeCanvas");
            fadeGO.transform.SetParent(transform);
            
            Canvas canvas = fadeGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure it's on top
            
            CanvasGroup cg = fadeGO.AddComponent<CanvasGroup>();
            fadeCanvasGroup = cg;
            
            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(fadeGO.transform);
            
            UnityEngine.UI.Image img = imageGO.AddComponent<UnityEngine.UI.Image>();
            img.color = fadeColor;
            fadeImage = img;
            
            RectTransform rect = img.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        // Start with fade out
        StartCoroutine(FadeOut());
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneCoroutine(sceneIndex));
    }
    
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }
    
    public void LoadGame()
    {
        LoadScene("MobileGame");
    }
    
    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
    }
    
    IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = true;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        fadeCanvasGroup.alpha = 1f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    
    public void QuitGame()
    {
        // Save data before quitting
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveGameData();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void RestartCurrentScene()
    {
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    // Convenience methods for UI buttons
    public void OnPlayButtonClicked()
    {
        LoadGame();
    }
    
    public void OnMainMenuButtonClicked()
    {
        LoadMainMenu();
    }
    
    public void OnQuitButtonClicked()
    {
        QuitGame();
    }
    
    public void OnRestartButtonClicked()
    {
        RestartCurrentScene();
    }
} 