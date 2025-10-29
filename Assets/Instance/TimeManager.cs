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
    private bool isPaused = false;
    private Button pauseButton, speedUpButton;
    private int currentSpeedUpLevel = 0;
    private int[] speedUpLevel = new int[] { 1, 2, 3, 5 };
    public float curGameTime
    {
        get => timeProgressBar.currentTime;
        set => timeProgressBar.StartProgressBar(productionStateDuration, ChangeState, value);
    }

    //TEST
    private Text timescalelevelText;

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

        timescalelevelText = GameObject.FindWithTag("TimeScaleLevelText")?.GetComponent<Text>();
        timescalelevelText.text = $"当前速度倍率：{speedUpLevel[currentSpeedUpLevel]}";
    }

    void OnEnable()
    {
        pauseButton.onClick.AddListener(ChangePauseGameState);
        speedUpButton.onClick.AddListener(ChangeSpeedUpLevel);
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
            timescalelevelText = GameObject.FindWithTag("TimeScaleLevelText")?.GetComponent<Text>();
            timescalelevelText.text = $"当前速度倍率：{speedUpLevel[currentSpeedUpLevel]}";
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

    public void ChangeSpeedUpLevel()
    {
        if (isPaused) return;
        currentSpeedUpLevel = (currentSpeedUpLevel + 1) % speedUpLevel.Length;
        speedUpScale = speedUpLevel[currentSpeedUpLevel];
        Time.timeScale = speedUpScale;
        timescalelevelText.text = $"当前速度倍率：{speedUpLevel[currentSpeedUpLevel]}";
    }

    public void ChangePauseGameState()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            timescalelevelText.text = $"当前速度倍率：0";
        }
        else
        {
            Time.timeScale = speedUpScale;
            timescalelevelText.text = $"当前速度倍率：{speedUpLevel[currentSpeedUpLevel]}";
        }

    }
}