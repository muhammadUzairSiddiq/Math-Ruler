using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BlackoutFadeAndLoad : MonoBehaviour
{
    public Image blackoutImage; // Fullscreen black Image, alpha 0 at start
    public float fadeDuration = 1f;

    void Awake()
    {
        if (blackoutImage != null)
        {
            var c = blackoutImage.color;
            c.a = 0f;
            blackoutImage.color = c;
            blackoutImage.raycastTarget = false; // Default to false
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        if (blackoutImage == null)
        {
            Debug.LogError("No blackout Image assigned!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            yield break;
        }
        blackoutImage.raycastTarget = true; // Enable during fade
        float t = 0f;
        Color c = blackoutImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            blackoutImage.color = c;
            yield return null;
        }
        c.a = 1f;
        blackoutImage.color = c;
        blackoutImage.raycastTarget = false; // Disable after fade
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
} 