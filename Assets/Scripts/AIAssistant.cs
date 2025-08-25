using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AIAssistant : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI explanationText; // Use this for all feedback
    public Button helpButton;
    public AnswerVerifier answerVerifier;

    [Header("Timing Settings")]
    public float thinkingThreshold = 7f; // Seconds before showing 'need help?' message

    [Header("Help System")]
    public bool showHintFirst = true; // Show hint first, then answer on second click
    private bool hasShownHint = false; // Track if hint was shown for current equation

    [Header("Message Pools")]
    [TextArea]
    public List<string> motivationalMessages = new List<string>{
        "Great job! Keep it up!",
        "You're on fire!",
        "Amazing work!",
        "You nailed it!",
        "Fantastic!",
        "Impressive!",
        "You're a math star!",
        "Outstanding!",
        "Brilliant!",
        "Keep crushing it!"
    };
    [TextArea]
    public List<string> gentleMessages = new List<string>{
        "Don't worry, try again!",
        "Keep going, you can do it!",
        "Mistakes help us learn!",
        "Almost there, give it another shot!",
        "Stay positive, try once more!",
        "You're getting closer!",
        "It's okay to be wrong, try again!",
        "Keep practicing, you'll get it!",
        "Don't give up!",
        "Believe in yourself!"
    };
    [TextArea]
    public List<string> thinkingMessages = new List<string>{
        "Need a hint or the answer?",
        "It's okay to take your time!",
        "Are you still thinking?",
        "If you need help, just ask!",
        "Stuck? Click help for a hint, click again for the answer!",
        "Take a deep breath and try!"
    };

    private float timeSinceEquation;
    private bool waitingForAnswer;
    private TypewriterEffect explanationTypewriter;

    void Start()
    {
        AutoFindReferences();
        if (helpButton != null)
            helpButton.onClick.AddListener(OnHelpButtonPressed);
        if (answerVerifier != null)
            answerVerifier.OnAnswerChecked += OnAnswerChecked;
        waitingForAnswer = false;
        if (explanationText != null)
            explanationTypewriter = explanationText.GetComponent<TypewriterEffect>();
    }

    void Update()
    {
        if (waitingForAnswer)
        {
            timeSinceEquation += Time.deltaTime;
            if (timeSinceEquation > thinkingThreshold)
            {
                ShowThinkingMessage();
                waitingForAnswer = false; // Only show once per question
            }
        }
    }

    public void OnNewEquation()
    {
        timeSinceEquation = 0f;
        waitingForAnswer = true;
        hasShownHint = false; // Reset hint state for new equation
        if (explanationTypewriter != null)
            explanationTypewriter.SetInstant("");
        else if (explanationText != null)
            explanationText.text = "";
    }

    public void OnAnswerChecked(bool isCorrect)
    {
        waitingForAnswer = false;
        string msg = isCorrect ? GetRandomMessage(motivationalMessages) : GetRandomMessage(gentleMessages);
        PlayTypewriterExplanation(msg);
    }

    void ShowThinkingMessage()
    {
        PlayTypewriterExplanation(GetRandomMessage(thinkingMessages));
    }

    void OnHelpButtonPressed()
    {
        if (answerVerifier == null) return;
        
        var eq = answerVerifier.GetCurrentEquation();
        
        if (showHintFirst && !hasShownHint)
        {
            // Show hint first
            string hint = GenerateContextualHint(eq.first, eq.second, answerVerifier.GetOperator());
            PlayTypewriterExplanation(hint);
            hasShownHint = true;
            
            // Update button text to indicate next click will show answer
            if (helpButton != null)
            {
                TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Show Answer";
                }
            }
        }
        else
        {
            // Show exact answer
            string msg = $"The answer is {eq.first} {answerVerifier.GetOperator()} {eq.second} = {eq.correct}";
            PlayTypewriterExplanation(msg);
            waitingForAnswer = false;
            
            // Reset button text for next equation
            if (helpButton != null)
            {
                TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Help";
                }
            }
        }
    }

    string GenerateContextualHint(int first, int second, string op)
    {
        switch (op)
        {
            case "+":
                return $"Hint: Start on number {first}, then take {second} steps to the RIGHT. This will help you find the answer!";
            case "-":
                return $"Hint: Start on number {first}, then take {second} steps to the LEFT. This will help you find the answer!";
            case "*":
                return $"Hint: Start on number {first}, then take {second} groups of {first} steps to the RIGHT. This will help you find the answer!";
            case "/":
                return $"Hint: Start on number {first}, then find how many groups of {second} make {first}. This will help you find the answer!";
            default:
                return "Hint: Read the equation carefully and think about what operation to use!";
        }
    }

    string GetRandomMessage(List<string> pool)
    {
        if (pool == null || pool.Count == 0) return "";
        return pool[Random.Range(0, pool.Count)];
    }

    void PlayTypewriterExplanation(string msg)
    {
        if (explanationTypewriter != null)
        {
            var mode = (Random.value > 0.5f) ? TypewriterEffect.TypeMode.Letter : TypewriterEffect.TypeMode.Word;
            explanationTypewriter.PlayTypewriter(msg, mode);
        }
        else if (explanationText != null)
        {
            explanationText.text = msg;
        }
    }

    [ContextMenu("Auto Find References")]
    public void AutoFindReferences()
    {
        if (explanationText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            explanationText = texts.FirstOrDefault(t => t.name.ToLower().Contains("result") || t.name.ToLower().Contains("explanation"));
        }
        if (helpButton == null)
        {
            var buttons = FindObjectsOfType<Button>(true);
            helpButton = buttons.FirstOrDefault(b => b.name.ToLower().Contains("help"));
        }
        if (answerVerifier == null)
        {
            answerVerifier = FindObjectOfType<AnswerVerifier>();
        }
        if (explanationText != null && explanationTypewriter == null)
            explanationTypewriter = explanationText.GetComponent<TypewriterEffect>();
    }

    [ContextMenu("Test Hint System")]
    public void TestHintSystem()
    {
        Debug.Log("=== TESTING HINT SYSTEM ===");
        
        // Simulate a new equation
        hasShownHint = false;
        
        // Test hint generation
        string hint = GenerateContextualHint(5, 3, "+");
        Debug.Log($"Generated hint: {hint}");
        
        // Test button text update
        if (helpButton != null)
        {
            TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Show Answer";
                Debug.Log("Button text updated to: Show Answer");
            }
        }
        
        Debug.Log("Hint system test complete!");
    }

    [ContextMenu("Test Answer System")]
    public void TestAnswerSystem()
    {
        Debug.Log("=== TESTING ANSWER SYSTEM ===");
        
        // Simulate showing answer
        hasShownHint = true;
        
        // Test answer generation
        string answer = "The answer is 5 + 3 = 8";
        Debug.Log($"Generated answer: {answer}");
        
        // Test button text reset
        if (helpButton != null)
        {
            TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Help";
                Debug.Log("Button text reset to: Help");
            }
        }
        
        Debug.Log("Answer system test complete!");
    }

    [ContextMenu("Toggle Hint System")]
    public void ToggleHintSystem()
    {
        showHintFirst = !showHintFirst;
        Debug.Log($"Hint system {(showHintFirst ? "ENABLED" : "DISABLED")}. Help button will now show {(showHintFirst ? "hint first, then answer" : "answer directly")}.");
    }

    [ContextMenu("Reset Help Button")]
    public void ResetHelpButton()
    {
        hasShownHint = false;
        if (helpButton != null)
        {
            TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Help";
                Debug.Log("Help button reset to: Help");
            }
        }
    }

    // Public method to force show hint (useful for external calls)
    public void ForceShowHint()
    {
        if (answerVerifier == null) return;
        
        var eq = answerVerifier.GetCurrentEquation();
        string hint = GenerateContextualHint(eq.first, eq.second, answerVerifier.GetOperator());
        PlayTypewriterExplanation(hint);
        hasShownHint = true;
        
        // Update button text
        if (helpButton != null)
        {
            TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Show Answer";
            }
        }
    }

    // Public method to force show answer (useful for external calls)
    public void ForceShowAnswer()
    {
        if (answerVerifier == null) return;
        
        var eq = answerVerifier.GetCurrentEquation();
        string msg = $"The answer is {eq.first} {answerVerifier.GetOperator()} {eq.second} = {eq.correct}";
        PlayTypewriterExplanation(msg);
        waitingForAnswer = false;
        
        // Reset button text
        if (helpButton != null)
        {
            TextMeshProUGUI buttonText = helpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Help";
            }
        }
    }
}

// In AnswerVerifier.cs, add this event:
// public event System.Action<bool> OnAnswerChecked;
// Call OnAnswerChecked?.Invoke(isCorrect) in ShowResult or HandleResult after checking the answer.
// Also, call aiAssistant.OnNewEquation() when a new equation is generated. 