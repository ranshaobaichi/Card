using UnityEngine;
using Category;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float productionStateDuration = 60f;
    private GameTimeState gameTimeState;
    public ProgressBar timeProgressBar;

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
    }

    void Start()
    {
        gameTimeState = GameTimeState.ProduceState;
        timeProgressBar.StartProgressBar(productionStateDuration, ChangeState);
    }

    public void ChangeState()
    {
        timeProgressBar?.StopProgressBar();
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
                PrepareProductionToSettlementData();
                SceneManager.LoadScene(SceneManager.SettlementScene);
                break;
            case GameTimeState.BattleState:
                SceneManager.LoadScene(SceneManager.BattleScene);
                break;
            default:
                break;
        }
    }

    public void PauseGame()
    {
        switch (gameTimeState)
        {
            case GameTimeState.ProduceState:
                timeProgressBar?.PauseProgressBar();
                break;
            case GameTimeState.SettlementState:                
            case GameTimeState.BattleState:
            default:
                throw new System.NotImplementedException();
        }
    }

    public void ResumeGame()
    {
        switch (gameTimeState)
        {
            case GameTimeState.ProduceState:
                timeProgressBar?.ResumeProgressBar();
                break;
            case GameTimeState.SettlementState:
            case GameTimeState.BattleState:
            default:
                throw new System.NotImplementedException();
        }
    }
    
    private void PrepareProductionToSettlementData()
    {
        var productionToSettlementData = new DataStruct.ProductionToSettlementData();
        productionToSettlementData.Initialize();
        var allCards = CardManager.Instance.allCards;
        foreach (var cardPair in allCards)
        {
            var cardType = cardPair.Key;
            var card = cardPair.Value;
            if (cardType == CardType.None || cardType == CardType.Events)
            {
                continue;
            }
            productionToSettlementData.cardDescriptions[cardType].Add(card.cardDescription.cardType switch
            {
                CardType.Creatures => (int)card.cardDescription.creatureCardType,
                CardType.Resources => (int)card.cardDescription.resourceCardType,
                _ => throw new System.NotImplementedException(),
            });
        }
        SceneManager.Instance.productionToSettlementData = productionToSettlementData;
    }
}