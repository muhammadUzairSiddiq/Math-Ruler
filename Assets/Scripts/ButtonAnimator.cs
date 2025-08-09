using UnityEngine;

public class ButtonAnimator : MonoBehaviour
{
    public float popScale = 1.2f;
    public float popDuration = 0.15f;
    public float returnDuration = 0.15f;
    public AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;
    private Coroutine animCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayAnimation()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PopAnimation());
    }

    System.Collections.IEnumerator PopAnimation()
    {
        // Scale up
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / popDuration);
            float scale = Mathf.Lerp(1f, popScale, popCurve.Evaluate(progress));
            transform.localScale = originalScale * scale;
            yield return null;
        }
        // Scale back
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / returnDuration);
            float scale = Mathf.Lerp(popScale, 1f, popCurve.Evaluate(progress));
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
        animCoroutine = null;
    }
} 