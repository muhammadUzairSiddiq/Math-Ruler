using UnityEngine;
using TMPro;

public class TypewriterEffectTest : MonoBehaviour
{
    public TextMeshProUGUI testText;
    public TypewriterEffect typewriter;

    void Start()
    {
        if (typewriter == null && testText != null)
            typewriter = testText.GetComponent<TypewriterEffect>();
        if (typewriter != null)
            StartCoroutine(RunTest());
        else
            Debug.LogError("TypewriterEffect or testText not assigned!");
    }

    System.Collections.IEnumerator RunTest()
    {
        string eq = "7 + 8 = 15";
        testText.text = "";
        Debug.Log("[TypewriterEffectTest] Starting letter-by-letter test");
        typewriter.PlayTypewriter(eq, TypewriterEffect.TypeMode.Letter);
        yield return new WaitForSeconds(eq.Length * typewriter.letterDelay + 1f);
        testText.text = "";
        Debug.Log("[TypewriterEffectTest] Starting word-by-word test");
        typewriter.PlayTypewriter(eq, TypewriterEffect.TypeMode.Word);
    }
} 