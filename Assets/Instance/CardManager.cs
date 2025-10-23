using System.Collections.Generic;
using Category.Production;
using Category;
using UnityEngine;
using static CardAttributeDB;
using Unity.VisualScripting;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public GameObject cardPrefab;
    public Canvas canvas;
    public Dictionary<CardType, List<Card>> allCards = new Dictionary<CardType, List<Card>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        isDragging = false;
        draggingCard = null;
        foreach (CardType cardType in System.Enum.GetValues(typeof(CardType)))
        {
            if (cardType == CardType.None) continue;
            allCards[cardType] = new List<Card>();
        }

        // TEST FUNCTION: 添加场上已有卡牌到管理器
        foreach (var card in FindObjectsByType<Card>(sortMode: FindObjectsSortMode.None))
        {
            allCards[card.cardDescription.cardType].Add(card);
            card.OnCardDeleted += OnCardDeleted;
            card.cardID = GetCardIdentityID();
            AddCardAttribute(card);
        }

        SceneManager.AfterSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged()
    {
        // Clear all cards
        allCards.Clear();

        // Clear battle cards
        if (SceneManager.Instance.currentSceneName != SceneManager.BattleScene)
            battleSceneCreatureCardIDs.Clear();
    }

    public void RemoveCardAttribute(long cardID)
    {
        // delete the card attribute
        if (creatureCardAttributes.ContainsKey(cardID))
        {
            creatureCardAttributes.Remove(cardID);
        }
        if (resourceCardAttributes.ContainsKey(cardID))
        {
            resourceCardAttributes.Remove(cardID);
        }
    }


    #region 卡牌逻辑内容管理    
    private static long cardIdentityID = 1;
    private static long cardSlotIdentityID = 1;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public Card draggingCard;

    public long GetCardIdentityID()
    {
        long newID = cardIdentityID++;
        // Debug.Log($"Generated new card ID: {newID}");
        return newID % long.MaxValue;
    }

    public long GetCardSlotIdentityID()
    {
        long newID = cardSlotIdentityID++;
        // Debug.Log($"Generated new card slot ID: {newID}");
        return newID % long.MaxValue;
    }

    public Card CreateCard(Card.CardDescription cardDescription, Vector2 position = default, long cardID = -1)
    {
        if (!cardDescription.IsValid())
        {
            Debug.LogError("CardManager: Invalid CardDescription");
            return null;
        }
        Card newCard = Instantiate(cardPrefab, position, Quaternion.identity).GetComponent<Card>();
        if (cardID == -1) cardID = GetCardIdentityID();
        newCard.cardID = cardID;
        newCard.SetCardType(cardDescription);
        allCards[newCard.cardDescription.cardType].Add(newCard);
        AddCardAttribute(newCard);
        newCard.OnCardDeleted += OnCardDeleted;
        return newCard;
    }

    /// <summary>
    /// 删除卡牌并移除其属性记录
    /// </summary>
    /// <param name="card"></param>
    private void OnCardDeleted(Card card)
    {
        allCards[card.cardDescription.cardType].Remove(card);
        RemoveCardAttribute(card);
    }
    #endregion

    # region 卡牌合成表管理
    List<CraftTableDB.Recipe> unlockedCraftableRecipes = new List<CraftTableDB.Recipe>();
    public (List<Card>, CraftTableDB.Recipe)? GetRecipe(List<Card> inputCards)
        => DataBaseManager.Instance.craftTableDB.GetRecipe(inputCards, unlockedCraftableRecipes);
    # endregion

    #region 事件卡UI管理
    [Header("事件卡UI管理")]
    public Transform eventUIParent;
    public Vector2 eventUIoffset;
    public Vector2 UIthreshold;
    private Dictionary<Card, EventUI> EventUIs = new Dictionary<Card, EventUI>();

    public void PopUpEventUI(Card card)
    {
        card.cardSlot.EndProduction();
        EventUI eventUI = EventUIs.GetValueOrDefault(card, null);

        if (eventUI == null)
        {
            if (!DataBaseManager.Instance.TryGetEventCardUIPrefab(card.cardDescription.eventCardType, out GameObject prefab))
            {
                Debug.LogError($"No EventUI prefab found for EventCardType: {card.cardDescription.eventCardType}");
                return;
            }
            eventUI = Instantiate(prefab, eventUIParent).GetComponent<EventUI>();
            EventUIs[card] = eventUI;
            card.OnCardDeleted += StopTrackingUIEvent;
        }
        eventUI.eventCard = card;

        /// 设置UI位置
        RectTransform cardRect = card.GetComponent<RectTransform>();
        RectTransform eventUIRect = eventUI.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 1. 将卡牌的世界坐标转换为 Canvas 本地坐标
        Vector2 cardLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, cardRect.position),
            canvas.worldCamera,
            out cardLocalPos
        );

        // 2. 获取尺寸（注意：rect.size 已考虑缩放）
        Vector2 cardSize = cardRect.rect.size;
        Vector2 UISize = eventUIRect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        // 3. 计算卡牌在 Canvas 本地坐标系中的边界
        // 需要考虑 pivot 的偏移：cardLocalPos 对应 pivot 位置，不一定是中心
        Vector2 cardPivotOffset = new Vector2(
            cardSize.x * (cardRect.pivot.x - 0.5f),
            cardSize.y * (cardRect.pivot.y - 0.5f)
        );
        Vector2 cardCenter = cardLocalPos - cardPivotOffset;

        // 4. 判断卡牌相对 Canvas 中心的位置
        int right = 0, up = 0;

        // right: 判断卡牌中心在 Canvas 的左/中/右区域
        if (Mathf.Abs(cardCenter.x) <= UIthreshold.x) right = 0;
        else if (cardCenter.x < 0) right = 1;  // 卡牌在左侧，UI 放右边
        else right = -1;  // 卡牌在右侧，UI 放左边

        // up: 判断卡牌中心在 Canvas 的下/中/上区域
        if (Mathf.Abs(cardCenter.y) <= UIthreshold.y) up = 0;
        else if (cardCenter.y < 0) up = 1;  // 卡牌在下方，UI 放上面
        else up = -1;  // 卡牌在上方，UI 放下面

        // 5. 计算 UI 中心的偏移量
        float xOffset = right * (cardSize.x / 2 + eventUIoffset.x + UISize.x / 2);
        float yOffset = up * (cardSize.y / 2 + eventUIoffset.y + UISize.y / 2);

        // 6. 计算 UI 的目标中心位置（Canvas 本地坐标）
        Vector2 uiCenter = cardCenter + new Vector2(xOffset, yOffset);

        // 7. 考虑 UI 的 pivot，转换为 anchoredPosition
        Vector2 uiPivotOffset = new Vector2(
            UISize.x * (eventUIRect.pivot.x - 0.5f),
            UISize.y * (eventUIRect.pivot.y - 0.5f)
        );
        eventUI.OpenUI(uiCenter + uiPivotOffset);
        // Debug.Log($"CardCenter: {cardCenter}, Right: {right}, Up: {up}, UICenter: {uiCenter}, AnchoredPos: {eventUIRect.anchoredPosition}");
    }

    private void StopTrackingUIEvent(Card card)
    {
        if (EventUIs.ContainsKey(card))
        {
            EventUIs.Remove(card);
        }
    }
    # endregion

    # region 卡牌属性管理
    private void AddCardAttribute(Card card, object attribute = null)
    {
        switch (card.cardDescription.cardType)
        {
            case CardType.Creatures:
                if (attribute != null && attribute is CreatureCardAttribute cca)
                    creatureCardAttributes[card.cardID] = cca.Clone() as CreatureCardAttribute;
                else
                {
                    var dbAttr = DataBaseManager.Instance.GetCardAttribute<CreatureCardAttribute>(card.cardDescription);
                    creatureCardAttributes[card.cardID] = dbAttr?.Clone() as CreatureCardAttribute;
                }
                Debug.Log($"Added satiety attribute for creature card {card.name} : {creatureCardAttributes[card.cardID]?.basicAttributes.satiety}");
                break;
            case CardType.Resources:
                if (attribute != null && attribute is ResourceCardAttribute rca)
                    resourceCardAttributes[card.cardID] = rca.Clone() as ResourceCardAttribute;
                else
                {
                    var dbAttr = DataBaseManager.Instance.GetCardAttribute<ResourceCardAttribute>(card.cardDescription);
                    resourceCardAttributes[card.cardID] = dbAttr?.Clone() as ResourceCardAttribute;
                }
                break;
            case CardType.Events:
                Debug.Log("Event Card has no attributes to add.");
                break;
            default:
                Debug.LogWarning($"CardManager: No attribute added for {card.name} CardType {card.GetCardTypeString()}");
                break;
        }
    }
    private void RemoveCardAttribute(Card card)
    {
        switch (card.cardDescription.cardType)
        {
            case CardType.Creatures:
                creatureCardAttributes.Remove(card.cardID);
                break;
            case CardType.Resources:
                resourceCardAttributes.Remove(card.cardID);
                break;
            default:
                Debug.LogWarning($"CardManager: No attribute removed for {card.name} CardType {card.GetCardTypeString()}");
                break;
        }
    }
    public T GetCardAttribute<T>(long cardID) where T : class
    {
        if (typeof(T) == typeof(CreatureCardAttribute))
        {
            if (!creatureCardAttributes.TryGetValue(cardID, out var v))
            {
                Debug.LogWarning($"CardManager: No CreatureCardAttribute for cardID={cardID}");
                return null;
            }
            return v as T;
        }

        if (typeof(T) == typeof(ResourceCardAttribute))
        {
            if (!resourceCardAttributes.TryGetValue(cardID, out var v))
            {
                Debug.LogWarning($"CardManager: No ResourceCardAttribute for cardID={cardID}");
                return null;
            }
            return v as T;
        }

        Debug.LogWarning($"CardManager.GetCardAttribute<{typeof(T).Name}> unsupported type.");
        return null;
    }
    public T GetCardAttribute<T>(Card card) where T : class
        => card ? GetCardAttribute<T>(card.cardID) : null;

    #region 资源卡
    Dictionary<long, ResourceCardAttribute> resourceCardAttributes = new Dictionary<long, ResourceCardAttribute>();
    public IReadOnlyDictionary<long, ResourceCardAttribute> GetResourceCardAttributes()
        => resourceCardAttributes;
    # endregion

    # region 生物卡
    Dictionary<long, CreatureCardAttribute> creatureCardAttributes = new Dictionary<long, CreatureCardAttribute>();
    public IReadOnlyDictionary<long, CreatureCardAttribute> GetCreatureCardAttributes()
        => creatureCardAttributes;
    public float GetWorkEfficiencyValue(Card creatureCard)
    {
        if (creatureCardAttributes.ContainsKey(creatureCard.cardID))
        {
            var workEfficiencyType = creatureCardAttributes[creatureCard.cardID].basicAttributes.workEfficiencyAttributes.craftWorkEfficiency;
            return DataBaseManager.Instance.GetWorkEfficiencyValue(workEfficiencyType);
        }
        Debug.LogWarning($"CardManager: No CreatureCardAttribute found for {creatureCard.name}");
        return 0.0f;
    }
    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
        => DataBaseManager.Instance.GetWorkEfficiencyValue(workEfficiencyType);


    #endregion

    #region 事件卡
    #endregion

    #endregion

    # region 战斗场景数据
    public List<long> battleSceneCreatureCardIDs = new List<long>();
    # endregion
}