using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeAndLoadScene : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = FindObjectOfType<CanvasGroup>();
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("No CanvasGroup assigned for fade!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            yield break;
        }
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
} 