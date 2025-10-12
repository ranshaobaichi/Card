using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static string ProductionScene = "ProductionScene";
    public static string BattleScene = "BattleScene";
    public string currentSceneName = "";

    public static event System.Action SceneChanged;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public static void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        SceneChanged?.Invoke();
    }
}