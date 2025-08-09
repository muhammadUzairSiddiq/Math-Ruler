using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Audio Clips")]
    public List<AudioClipData> musicClips = new List<AudioClipData>();
    public List<AudioClipData> sfxClips = new List<AudioClipData>();
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    private static AudioManager instance;
    public static AudioManager Instance => instance;
    
    private Dictionary<string, AudioClipData> musicDictionary = new Dictionary<string, AudioClipData>();
    private Dictionary<string, AudioClipData> sfxDictionary = new Dictionary<string, AudioClipData>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudio()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }
        
        // Build dictionaries for quick lookup
        BuildAudioDictionaries();
        
        // Load settings from DataManager if available
        if (DataManager.Instance != null)
        {
            masterVolume = DataManager.Instance.gameData.masterVolume;
            musicVolume = DataManager.Instance.gameData.musicEnabled ? 0.7f : 0f;
            sfxVolume = DataManager.Instance.gameData.soundEnabled ? 1f : 0f;
        }
        
        UpdateAudioVolumes();
    }
    
    void BuildAudioDictionaries()
    {
        musicDictionary.Clear();
        sfxDictionary.Clear();
        
        foreach (var clip in musicClips)
        {
            if (!string.IsNullOrEmpty(clip.name))
                musicDictionary[clip.name] = clip;
        }
        
        foreach (var clip in sfxClips)
        {
            if (!string.IsNullOrEmpty(clip.name))
                sfxDictionary[clip.name] = clip;
        }
    }
    
    public void PlayMusic(string musicName)
    {
        if (musicDictionary.TryGetValue(musicName, out AudioClipData clipData))
        {
            musicSource.clip = clipData.clip;
            musicSource.volume = clipData.volume * musicVolume * masterVolume;
            musicSource.pitch = clipData.pitch;
            musicSource.loop = clipData.loop;
            musicSource.Play();
            Debug.Log($"Playing music: {musicName}");
        }
        else
        {
            Debug.LogWarning($"Music clip '{musicName}' not found!");
        }
    }
    
    public void PlaySFX(string sfxName)
    {
        if (sfxDictionary.TryGetValue(sfxName, out AudioClipData clipData))
        {
            sfxSource.PlayOneShot(clipData.clip, clipData.volume * sfxVolume * masterVolume);
            Debug.Log($"Playing SFX: {sfxName}");
        }
        else
        {
            Debug.LogWarning($"SFX clip '{sfxName}' not found!");
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
        
        // Save to DataManager
        if (DataManager.Instance != null)
        {
            DataManager.Instance.gameData.masterVolume = masterVolume;
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
        
        // Save to DataManager
        if (DataManager.Instance != null)
        {
            DataManager.Instance.gameData.musicEnabled = volume > 0f;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioVolumes();
        
        // Save to DataManager
        if (DataManager.Instance != null)
        {
            DataManager.Instance.gameData.soundEnabled = volume > 0f;
        }
    }
    
    void UpdateAudioVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }
    
    // Convenience methods for common game events
    public void PlayCorrectAnswerSound()
    {
        PlaySFX("correct");
    }
    
    public void PlayWrongAnswerSound()
    {
        PlaySFX("wrong");
    }
    
    public void PlayButtonClickSound()
    {
        PlaySFX("button_click");
    }
    
    public void PlayRewardSound()
    {
        PlaySFX("reward");
    }
    
    public void PlayVictorySound()
    {
        PlaySFX("victory");
    }
    
    public void PlayGameOverSound()
    {
        PlaySFX("game_over");
    }
} 