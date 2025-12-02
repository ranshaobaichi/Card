using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Background Musics")]
    public AudioClip mainSceneMusic;
    public AudioClip battleSceneMusic;
    [Header("Sound Effects")]
    public List<AudioClip> clickSoundEffects = new List<AudioClip>();
    public AudioClip putCardDownSoundEffect;
    public AudioClip sceneChangeSoundEffect;
    public AudioClip popCardsSoundEffect;
    public AudioClip eatFoodSoundEffect;

    [Header("Settings")]
    public int fxPoolSize = 8;
    public float musicVolume = 0.7f;
    public float fxVolume = 0.7f;
    public bool startOnAwake = true;

    private AudioSource musicSource;
    private List<AudioSource> fxPool = new List<AudioSource>();
    private int fxPoolIndex = 0;
    private Coroutine musicRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 音乐播放器
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            // 音效池
            for (int i = 0; i < Mathf.Max(1, fxPoolSize); i++)
            {
                GameObject go = new GameObject("FX_Source_" + i);
                go.transform.SetParent(transform);
                AudioSource src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = fxVolume;
                fxPool.Add(src);
            }

            if (startOnAwake)
                StartBackgroundMusicLoop();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayClick();
        }
    }

    public void PlaySceneChange() { PlayFX(sceneChangeSoundEffect); }
    public void PlayPutCardDown() { PlayFX(putCardDownSoundEffect); }
    public void PlayPopCards() { PlayFX(popCardsSoundEffect); }
    public void PlayEatFood() { PlayFX(eatFoodSoundEffect); }


    public void OnSceneLoaded()
    {
        // 场景切换时播放场景切换音效并重选背景音乐（如果需要）
        PlaySceneChange();
        if (SceneManager.currentScene == SceneManager.BattleScene || SceneManager.currentScene == SceneManager.ProductionScene)
            StartBackgroundMusicLoop(SceneManager.currentScene);
    }

    public void StartBackgroundMusicLoop(string sceneName = null)
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(BackgroundMusicLoop(sceneName));
    }

    public void StopBackgroundMusic()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
        if (musicSource != null) musicSource.Stop();
    }

    private System.Collections.IEnumerator BackgroundMusicLoop(string sceneName = null)
    {
        AudioClip clip = sceneName switch
        {
            SceneManager.StartScene => mainSceneMusic,
            SceneManager.ProductionScene => mainSceneMusic,
            SceneManager.SettlementScene => mainSceneMusic,
            SceneManager.BattleScene => battleSceneMusic,
            _ => mainSceneMusic,
        };
        Debug.Log($"SoundManager: Playing background music for scene {sceneName ?? SceneManager.currentScene}, clip: {clip?.name ?? "null"}");
        while (true)
        {
            if (clip == null)
            {
                yield break;
            }
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
            yield return new WaitForSeconds(clip.length);
        }
    }

    public void PlayClick()
    {
        if (clickSoundEffects == null || clickSoundEffects.Count == 0) return;
        AudioClip clip = clickSoundEffects[Random.Range(0, clickSoundEffects.Count)];
        PlayFX(clip);
    }

    // 通用音效播放：从池中拿一个空闲 source 或按轮替覆盖
    private void PlayFX(AudioClip clip)
    {
        if (clip == null || fxPool.Count == 0) return;

        // 查找空闲 source
        AudioSource src = null;
        for (int i = 0; i < fxPool.Count; i++)
        {
            int idx = (fxPoolIndex + i) % fxPool.Count;
            if (!fxPool[idx].isPlaying)
            {
                src = fxPool[idx];
                fxPoolIndex = (idx + 1) % fxPool.Count;
                break;
            }
        }

        // 如果都在播放，则覆盖当前索引
        if (src == null)
        {
            src = fxPool[fxPoolIndex];
            fxPoolIndex = (fxPoolIndex + 1) % fxPool.Count;
        }

        src.clip = clip;
        src.volume = fxVolume;
        src.Play();
    }

    // 可由外部设置音量
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetFXVolume(float v)
    {
        fxVolume = Mathf.Clamp01(v);
        foreach (var s in fxPool) s.volume = fxVolume;
    }
}