using UnityEngine;
using System;

[Serializable]
public class GameData
{
    [Header("Player Progress")]
    public int highScore = 0;
    public int totalGamesPlayed = 0;
    public int totalCorrectAnswers = 0;
    public int totalQuestionsAnswered = 0;
    public float averageAccuracy = 0f;
    
    [Header("Game Settings")]
    public int preferredDifficulty = 1; // 1=Easy, 2=Medium, 3=Hard
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public float masterVolume = 1f;
    
    [Header("Unlockables")]
    public bool[] unlockedRewards = new bool[4]; // House, Pet, Car, Tree
    public int[] rewardUsageCount = new int[4];
    
    [Header("Statistics")]
    public DateTime lastPlayedDate;
    public int longestStreak = 0;
    public int currentStreak = 0;
    
    public GameData()
    {
        lastPlayedDate = DateTime.Now;
        unlockedRewards = new bool[4] { true, false, false, false }; // House unlocked by default
        rewardUsageCount = new int[4];
    }
    
    public void UpdateStatistics(bool wasCorrect)
    {
        totalQuestionsAnswered++;
        if (wasCorrect)
        {
            totalCorrectAnswers++;
            currentStreak++;
            if (currentStreak > longestStreak)
                longestStreak = currentStreak;
        }
        else
        {
            currentStreak = 0;
        }
        
        averageAccuracy = (float)totalCorrectAnswers / totalQuestionsAnswered;
    }
    
    public void UpdateHighScore(int newScore)
    {
        if (newScore > highScore)
        {
            highScore = newScore;
        }
    }
    
    public void UnlockReward(int rewardIndex)
    {
        if (rewardIndex >= 0 && rewardIndex < unlockedRewards.Length)
        {
            unlockedRewards[rewardIndex] = true;
            rewardUsageCount[rewardIndex]++;
        }
    }
    
    public bool IsRewardUnlocked(int rewardIndex)
    {
        return rewardIndex >= 0 && rewardIndex < unlockedRewards.Length && unlockedRewards[rewardIndex];
    }
}

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance => instance;
    
    public GameData gameData;
    private const string SAVE_KEY = "NumberGameData";
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadGameData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log("Game data loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game data: {e.Message}");
                gameData = new GameData();
            }
        }
        else
        {
            gameData = new GameData();
            Debug.Log("Created new game data");
        }
    }
    
    public void SaveGameData()
    {
        try
        {
            gameData.lastPlayedDate = DateTime.Now;
            string json = JsonUtility.ToJson(gameData, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("Game data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game data: {e.Message}");
        }
    }
    
    public void ResetGameData()
    {
        gameData = new GameData();
        SaveGameData();
        Debug.Log("Game data reset");
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameData();
        }
    }
    
    void OnDestroy()
    {
        SaveGameData();
    }
} 