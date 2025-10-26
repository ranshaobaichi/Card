using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static string ProductionScene = "ProductionScene";
    public static string BattleScene = "BattleScene";
    public static string SettlementScene = "SettlementScene";
    public static string StartScene = "StartScene";
    public static string currentScene = "";
    public static SceneManager Instance;

    public static event System.Action BeforeSceneChanged;
    public static event System.Action AfterSceneChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        currentScene = scene.name;
        AfterSceneChanged?.Invoke();
    }

    public static void LoadScene(string sceneName)
    {
        BeforeSceneChanged?.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    public static void QuitToStartScene()
    {
        BeforeSceneChanged?.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene(StartScene);
        GameObject[] ddolObjs = GameObject.FindGameObjectsWithTag("DontDestroyOnLoad");
        foreach (var obj in ddolObjs)
        {
            // TEST
            // Debug.Log("Destroying object: " + obj.name);
            if (obj.name == "SceneManager" || obj.name == "GlobalTestFunction" || obj.name == "SaveDataManager")
                continue;
            Destroy(obj);
        }

        BeforeSceneChanged = null;
        AfterSceneChanged = null;
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
            // 停止编辑器中的播放模式
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在发布版中退出应用
            UnityEngine.Application.Quit();
#endif
    }
}