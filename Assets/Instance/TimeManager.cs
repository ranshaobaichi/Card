using UnityEngine;
using Category;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public Text stateTimerText;
    public Text stateText;
    public float timeScale;
    public float[] gameTimeStateDuration = new float[3]; // Example durations for each state
    private GameTimeState gameTimeState;
    private float gameTimer;

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
    }

    void Start()
    {
        gameTimeState = GameTimeState.ProduceState;
        timeScale = 1.0f;
        gameTimer = 0f;
        RefreshUIReferences();

        SceneManager.SceneChanged += RefreshUIReferences;
    }

    void Update()
    {
        gameTimer += Time.deltaTime * timeScale;

        if (gameTimer >= gameTimeStateDuration[(int)gameTimeState])
        {
            ChangeState();
        }

        if (stateTimerText != null)
            stateTimerText.text = $"Time: {gameTimer:F1}s / {gameTimeStateDuration[(int)gameTimeState]}s";
        if (stateText != null)
            stateText.text = $"State: {gameTimeState}";
    }

    void RefreshUIReferences()
    {
        stateTimerText = stateTimerText != null ? stateTimerText : GameObject.Find("StateTimerText").GetComponent<Text>();
        stateText = stateText != null ? stateText : GameObject.Find("StateText").GetComponent<Text>();
        Debug.Log("UI references refreshed.");
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
        gameTimer = 0f;

        switch (nextState)
        {
            case GameTimeState.ProduceState:
                SceneManager.LoadScene(SceneManager.ProductionScene);
                break;
            case GameTimeState.SettlementState:
                // Handle settlement logic here
                Debug.Log("Entering Settlement State");
                break;
            case GameTimeState.BattleState:
                SceneManager.LoadScene(SceneManager.BattleScene);
                break;
            default:
                break;
        }
    }
}