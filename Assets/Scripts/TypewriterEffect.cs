using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    [Range(0.01f, 0.2f)]
    public float letterDelay = 0.05f;
    [Range(0.05f, 0.5f)]
    public float wordDelay = 0.2f;

    public enum TypeMode { Letter, Word }
    public TypeMode defaultMode = TypeMode.Letter;

    private Coroutine typingCoroutine;

    public void PlayTypewriter(string text)
    {
        PlayTypewriter(text, defaultMode);
    }

    public void PlayTypewriter(string text, TypeMode mode)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        Debug.Log($"[TypewriterEffect] Starting typewriter: '{text}' Mode: {mode}");
        if (mode == TypeMode.Word)
            typingCoroutine = StartCoroutine(TypeTextWordByWord(text));
        else
            typingCoroutine = StartCoroutine(TypeTextLetterByLetter(text));
    }

    IEnumerator TypeTextLetterByLetter(string text)
    {
        if (targetText == null) yield break;
        targetText.text = "";
        foreach (char c in text)
        {
            targetText.text += c;
            Debug.Log($"[TypewriterEffect] Letter: '{c}' | Current: '{targetText.text}'");
            yield return new WaitForSeconds(letterDelay);
        }
    }

    IEnumerator TypeTextWordByWord(string text)
    {
        if (targetText == null) yield break;
        targetText.text = "";
        string[] tokens = TokenizeForWordMode(text);
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            targetText.text += token;
            Debug.Log($"[TypewriterEffect] Word: '{token}' | Current: '{targetText.text}'");
            // Only add a space if the next token is not an operator or symbol
            if (i < tokens.Length - 1)
            {
                string next = tokens[i + 1];
                if (!IsOperatorOrSymbol(next) && !IsOperatorOrSymbol(token))
                    targetText.text += " ";
            }
            yield return new WaitForSeconds(wordDelay);
        }
    }

    // Tokenize equation: numbers, operators, and symbols as separate tokens
    string[] TokenizeForWordMode(string text)
    {
        // If it looks like an equation, split into numbers/operators/symbols
        if (Regex.IsMatch(text, @"[\d]+\s*[\+\-\=]\s*[\d]+"))
        {
            // Split into numbers, operators, and =, keeping them as tokens
            var matches = Regex.Matches(text, @"\d+|[\+\-\=]");
            string[] tokens = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
                tokens[i] = matches[i].Value;
            Debug.Log($"[TypewriterEffect] Tokenized equation: {string.Join(",", tokens)}");
            return tokens;
        }
        // Otherwise, split by space as before
        var split = text.Split(' ');
        Debug.Log($"[TypewriterEffect] Tokenized by space: {string.Join(",", split)}");
        return split;
    }

    bool IsOperatorOrSymbol(string token)
    {
        return token == "+" || token == "-" || token == "=";
    }

    public void SetInstant(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (targetText != null)
            targetText.text = text;
    }
} 