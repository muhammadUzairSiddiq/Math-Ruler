using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;
    public int currentScore = 0;
    public int correctAnswersInRow = 0;
    public int totalQuestions = 0;
    
    [Header("Game Settings")]
    public int questionsPerRound = 10;
    public float timeLimit = 60f;
    public bool enableTimer = true;
    
    [Header("References")]
    public AnswerVerifier answerVerifier;
    public UIManager uiManager;
    public PlayerController playerController;
    public AIAssistant aiAssistant;
    
    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnCorrectAnswersInRowChanged;
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }
    
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Find references if not assigned
        if (answerVerifier == null) answerVerifier = FindObjectOfType<AnswerVerifier>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (aiAssistant == null) aiAssistant = FindObjectOfType<AIAssistant>();
        
        // Subscribe to events
        if (answerVerifier != null)
        {
            answerVerifier.OnAnswerChecked += HandleAnswerChecked;
        }
        
        SetGameState(GameState.Playing);
    }
    
    public void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                case GameState.Victory:
                    HandleVictory();
                    break;
            }
        }
    }
    
    private void HandleAnswerChecked(bool isCorrect)
    {
        totalQuestions++;
        
        if (isCorrect)
        {
            currentScore += 10;
            correctAnswersInRow++;
            OnScoreChanged?.Invoke(currentScore);
            OnCorrectAnswersInRowChanged?.Invoke(correctAnswersInRow);
            
            // Check for victory condition
            if (correctAnswersInRow >= 5)
            {
                SetGameState(GameState.Victory);
            }
        }
        else
        {
            correctAnswersInRow = 0;
            OnCorrectAnswersInRowChanged?.Invoke(correctAnswersInRow);
        }
    }
    
    void HandleGameOver()
    {
        Debug.Log($"Game Over! Final Score: {currentScore}");
        // Implement game over logic
    }
    
    void HandleVictory()
    {
        Debug.Log($"Victory! Final Score: {currentScore}");
        // Implement victory logic
    }
    
    public void ResetGame()
    {
        currentScore = 0;
        correctAnswersInRow = 0;
        totalQuestions = 0;
        SetGameState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }
    
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }
    
    void OnDestroy()
    {
        if (answerVerifier != null)
        {
            answerVerifier.OnAnswerChecked -= HandleAnswerChecked;
        }
    }
} 