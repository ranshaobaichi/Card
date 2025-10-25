using UnityEngine;
using Category;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float productionStateDuration = 60f;
    private GameTimeState gameTimeState;
    public ProgressBar timeProgressBar;
    public float speedUpScale = 2f;
    private bool isPaused = false, isSpeedUp = false;
    private Button pauseButton, speedUpButton;

    public GameTimeState GetCurrentState() => gameTimeState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        pauseButton = GameObject.FindWithTag("PauseButton")?.GetComponent<Button>();
        speedUpButton = GameObject.FindWithTag("SpeedUpButton")?.GetComponent<Button>();
    }

    void OnEnable()
    {
        pauseButton.onClick.AddListener(() =>
        {
            isPaused = !isPaused;
            if (isPaused) Time.timeScale = 0f;
            else Time.timeScale = isSpeedUp ? speedUpScale : 1f;
        });

        speedUpButton.onClick.AddListener(() =>
        {
            isSpeedUp = !isSpeedUp;
            if (isSpeedUp) Time.timeScale = isPaused ? 0f : speedUpScale;
            else Time.timeScale = isPaused ? 0f : 1f;
        });
    }

    void Start()
    {
        gameTimeState = GameTimeState.ProduceState;
        timeProgressBar.StartProgressBar(productionStateDuration, ChangeState);
        SceneManager.AfterSceneChanged += OnChangeScene;
        SceneManager.BeforeSceneChanged += BeforeChangeScene;
    }

    private void BeforeChangeScene()
    {
        pauseButton.onClick.RemoveAllListeners();
        speedUpButton.onClick.RemoveAllListeners();
    }

    private void OnChangeScene()
    {
        if (SceneManager.currentScene == SceneManager.ProductionScene)
        {
            timeProgressBar = GameObject.FindWithTag("TimeProgressBar")?.GetComponent<ProgressBar>();
            pauseButton = GameObject.FindWithTag("PauseButton")?.GetComponent<Button>();
            speedUpButton = GameObject.FindWithTag("SpeedUpButton")?.GetComponent<Button>();
        }
    }

    public void ChangeState()
    {
        GameTimeState nextState = gameTimeState switch
        {
            GameTimeState.ProduceState => GameTimeState.SettlementState,
            GameTimeState.SettlementState => GameTimeState.BattleState,
            GameTimeState.BattleState => GameTimeState.ProduceState,
            _ => throw new System.NotImplementedException(),
        };

        Debug.Log($"State changed from {gameTimeState} to {nextState}");
        gameTimeState = nextState;

        switch (nextState)
        {
            case GameTimeState.ProduceState:
                SceneManager.LoadScene(SceneManager.ProductionScene);
                timeProgressBar?.StartProgressBar(productionStateDuration, ChangeState);
                break;
            case GameTimeState.SettlementState:
                // Handle settlement logic here
                // Debug.Log("Entering Settlement State");
                timeProgressBar.StopProgressBar();
                SceneManager.LoadScene(SceneManager.SettlementScene);
                break;
            case GameTimeState.BattleState:
                SceneManager.LoadScene(SceneManager.BattleScene);
                break;
            default:
                break;
        }
    }
}