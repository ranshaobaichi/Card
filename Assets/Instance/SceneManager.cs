using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static string ProductionScene = "ProductionScene";
    public static string BattleScene = "BattleScene";
    public static string SettlementScene = "SettlementScene";
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
}