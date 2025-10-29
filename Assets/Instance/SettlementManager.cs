using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettlementCardManager : MonoBehaviour
{
    public static SettlementCardManager Instance;
    public GameObject settlementCardSlotPrefab;
    public Button exitButton;

    [Header("Settlement Card Panels")]
    [Header("Food")]
    public SettlementCardPanel FoodPanel;
    [Header("Creature")]
    public SettlementCardPanel CreaturePanel;
    public RectTransform CreaturePanelPutArea;
    private Dictionary<long, int> creatureCardSatietyDict = new Dictionary<long, int>();
    [Header("Battle")]
    public int maxBattleCreatures = 3;
    public SettlementCardPanel BattleCardPanel;
    public RectTransform BattleCardPanelPutArea;
    public Text BattlePopulationText;

    [Header("Dragging State")]
    public GameObject draggingCardSlot;
    public SettlementCard draggingCard;
    public SettlementCard hoveredCard;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // add listeners to end drag events
        FoodPanel.onEndDrag += EndDraggingFoodCard;
        CreaturePanel.onEndDrag += EndDraggingCreatureCard;
        BattleCardPanel.onEndDrag += EndDraggingBattleCard;

        exitButton = GameObject.FindWithTag("ExitButton")?.GetComponent<Button>();
        exitButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.BattleScene));
        DealWithSettlementData();
        BattlePopulationText.text = $"{0}/{maxBattleCreatures}";
    }

    void OnDisable()
    {
        // remove listeners to end drag events
        FoodPanel.onEndDrag -= EndDraggingFoodCard;
        CreaturePanel.onEndDrag -= EndDraggingCreatureCard;
        BattleCardPanel.onEndDrag -= EndDraggingBattleCard;
    }

    public void DealWithSettlementData()
    {
        // Show all the creature cards in the Creature Panel
        // Debug.Log($"Has creature cards count: {CardManager.Instance.GetCreatureCardAttributes().Count}");
        foreach (var (cardID, creatureCardAttr) in CardManager.Instance.GetCreatureCardAttributes())
            CreaturePanel.AddCard<SC_Creature>(cardID);

        // Show all the food cards in the Food Panel
        foreach (var (cardID, resourceCardAttr) in CardManager.Instance.GetResourceCardAttributes())
            if (resourceCardAttr.resourceClassification == Category.ResourceCardClassification.Food)
                FoodPanel.AddCard<SC_Food>(cardID);
    }

    private bool IsPointerOverRect(RectTransform rect)
    {
        if (rect == null) return false;
        var canvas = rect.GetComponentInParent<Canvas>();
        var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
    }


    public void EndDraggingCreatureCard()
    {
        SC_Creature creatureCard = draggingCard as SC_Creature;
        draggingCard = null;
        if (IsPointerOverRect(BattleCardPanelPutArea))
        {
            if (BattleCardPanel.cards.Count >= maxBattleCreatures)
            {
                Debug.Log("Battle panel is full. Cannot add more creature cards.");
                return;
            }
            // Handle creature card being dragged over battle panel
            // Debug.Log($"Creature card {creatureCard.cardSlot.name} hovered over battle panel.");
            BattleCardPanel.AddCard<SC_Battle>(creatureCard.cardID);
            CreaturePanel.DeleteCard(creatureCard, false);
            BattlePopulationText.text = $"{BattleCardPanel.cards.Count}/{maxBattleCreatures}";

            // Add the card to CardManager's battle scene creature card list
            CardManager.Instance.battleSceneCreatureCardIDs.Add(creatureCard.cardID);
        }
    }

    public void EndDraggingBattleCard()
    {
        SC_Battle battleCard = draggingCard as SC_Battle;
        draggingCard = null;
        if (IsPointerOverRect(CreaturePanelPutArea))
        {
            // Handle battle card being dragged over creature panel
            Debug.Log($"Battle card {battleCard.cardSlot.name} hovered over creature panel.");
            var creatureCard = CreaturePanel.AddCard<SC_Creature>(battleCard.cardID);
            if (!creatureCardSatietyDict.ContainsKey(battleCard.cardID))
                creatureCardSatietyDict[battleCard.cardID] = creatureCard.satiety;
            else
                creatureCard.InitCard(battleCard.cardID, creatureCardSatietyDict[battleCard.cardID]);
            BattleCardPanel.DeleteCard(battleCard, false);

            // Update battle population text
            BattlePopulationText.text = $"{BattleCardPanel.cards.Count}/{maxBattleCreatures}";

            // Remove the card from CardManager's battle scene creature card list
            CardManager.Instance.battleSceneCreatureCardIDs.Remove(battleCard.cardID);
        }
    }

    public void EndDraggingFoodCard()
    {            
        SC_Food foodCard = draggingCard as SC_Food;
        draggingCard = null;
        if (hoveredCard is SC_Creature creatureCard && creatureCard.satiety >= 0)
        {
            // Handle food card being dragged over creature card
            Debug.Log($"Food card {foodCard.cardSlot.name} hovered over creature card {creatureCard.cardSlot.name}.");
            int actualConsumeSatiety = Mathf.Min(foodCard.satietyValue, creatureCard.satiety);
            if (foodCard.TryConsumeFood(actualConsumeSatiety))
            {
                creatureCard.EatingFood(actualConsumeSatiety);
                creatureCardSatietyDict[creatureCard.cardID] = creatureCard.satiety;
            }
        }
    }
    
}