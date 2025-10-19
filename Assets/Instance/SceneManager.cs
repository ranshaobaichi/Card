using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static string ProductionScene = "ProductionScene";
    public static string BattleScene = "BattleScene";
    public static string SettlementScene = "SettlementScene";
    public string currentSceneName = "";
    public static SceneManager Instance;

    /// <summary>
    /// Datas
    /// </summary>
    public DataStruct.ProductionToSettlementData productionToSettlementData;

    public static event System.Action SceneChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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