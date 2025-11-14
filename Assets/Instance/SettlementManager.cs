using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SettlementCardManager : MonoBehaviour
{
    public static SettlementCardManager Instance;
    public GameObject settlementCardSlotPrefab;
    public GameObject tooltipPrefab;
    public GameObject attributeDisplayPrefab;
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

    [Header("TooltipCanvas")]
    public Canvas tooltipCanvas;
    public Text tooltipHeaderText, tooltipContentText;
    public Button tooltipCloseButton, tooltipConfirmButton;

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

        tooltipCloseButton.onClick.AddListener(() => tooltipCanvas.gameObject.SetActive(false));
        tooltipConfirmButton.onClick.AddListener(() => ExitSettlementScene());
        exitButton = GameObject.FindWithTag("ExitButton")?.GetComponent<Button>();
        exitButton.onClick.AddListener(() => ChangeToNextStage());

        DealWithSettlementData();
        BattlePopulationText.text = $"{0}/{maxBattleCreatures}";
    }

    void OnDisable()
    {
        // remove listeners to end drag events
        FoodPanel.onEndDrag -= EndDraggingFoodCard;
        CreaturePanel.onEndDrag -= EndDraggingCreatureCard;
        BattleCardPanel.onEndDrag -= EndDraggingBattleCard;

        exitButton.onClick.RemoveListener(() => ChangeToNextStage());
        tooltipCloseButton.onClick.RemoveListener(() => tooltipCanvas.gameObject.SetActive(false));
        tooltipConfirmButton.onClick.RemoveListener(() => ExitSettlementScene());
    }

    public void ChangeToNextStage()
    {
        DisplayTooltipPanel();
    }

    private void DisplayTooltipPanel()
    {
        tooltipCanvas.gameObject.SetActive(true);
        List<string> cardsNotFull = new List<string>();
        foreach (SettlementCard card in CreaturePanel.cards)
            if (card is SC_Creature creatureCard && creatureCard.satiety > 0)
                cardsNotFull.Add(creatureCard.nameText.text);

        if (cardsNotFull.Count > 0)
        {
            tooltipHeaderText.text = "以下生物卡尚未吃饱,即将死去：";
            tooltipContentText.text = string.Join("\n", cardsNotFull);
        }
        else
        {
            tooltipHeaderText.text = "所有生物卡都吃饱了！";
        }
    }

    private void ExitSettlementScene()
    {
        List<SC_Creature> cardsNotFull = new List<SC_Creature>();
        foreach (SettlementCard card in CreaturePanel.cards)
            if (card is SC_Creature creatureCard && creatureCard.satiety > 0)
                cardsNotFull.Add(creatureCard);

        foreach (SC_Creature creatureCard in cardsNotFull)
        {
            Debug.Log($"Creature card {creatureCard.nameText.text} is not fully satiated (Satiety: {creatureCard.satiety}). It will be removed.");
            CreaturePanel.DeleteCard(creatureCard, true);
        }

        if (CardManager.Instance.battleSceneCreatureCardIDs.Count > 0)
        {
            SceneManager.LoadScene(SceneManager.BattleScene);
        }
        else
        {
            // TESTING: If no creature cards in battle scene, still allow to proceed to battle scene
            SceneManager.LoadScene(SceneManager.BattleScene);
        }
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
                Instantiate(tooltipPrefab, GetComponentInParent<Canvas>().transform).GetComponent<TooltipText>()
                    .SetTooltipText("战斗面板已满，无法添加更多生物卡！", TooltipText.TooltipMode.Warning);
                return;
            }
            if (creatureCard.satiety > 0)
            {
                Instantiate(tooltipPrefab, GetComponentInParent<Canvas>().transform).GetComponent<TooltipText>()
                    .SetTooltipText("生物卡未吃饱，无法进入战斗面板！", TooltipText.TooltipMode.Warning);
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
            // Debug.Log($"Food card {foodCard.cardSlot.name} hovered over creature card {creatureCard.cardSlot.name}.");
            int actualConsumeSatiety = Mathf.Min(foodCard.satietyValue, creatureCard.satiety);
            if (foodCard.TryConsumeFood(actualConsumeSatiety))
            {
                creatureCard.EatingFood(actualConsumeSatiety);
                creatureCardSatietyDict[creatureCard.cardID] = creatureCard.satiety;
                if (foodCard.satietyValue <= 0)
                {
                    // Handle food depletion (e.g., remove card from panel)
                    Debug.Log($"Food card {foodCard.cardSlot.name} is depleted and will be removed.");
                    FoodPanel.DeleteCard(foodCard, true);
                }
            }
        }
    }

    public void OnCardClicked(SettlementCard card)
    {
        if (card is SC_Creature|| card is SC_Battle)
        {
            CardAttributeDB.CreatureCardAttribute attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(card.cardID);
            CreatureAttributeDisplay panel = Instantiate(attributeDisplayPrefab, GetComponentInParent<Canvas>().transform).GetComponent<CreatureAttributeDisplay>();
            panel.UpdateAttributes(attr);
        }
    }
}