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
        "Need a hint?",
        "It's okay to take your time!",
        "Are you still thinking?",
        "If you need help, just ask!",
        "Stuck? The help button is here!",
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
        string msg = $"The answer is {eq.first} {answerVerifier.GetOperator()} {eq.second} = {eq.correct}. Try to understand how it works!";
        PlayTypewriterExplanation(msg);
        waitingForAnswer = false;
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
}

// In AnswerVerifier.cs, add this event:
// public event System.Action<bool> OnAnswerChecked;
// Call OnAnswerChecked?.Invoke(isCorrect) in ShowResult or HandleResult after checking the answer.
// Also, call aiAssistant.OnNewEquation() when a new equation is generated. 