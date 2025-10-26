using System.Collections.Generic;
using Category.Production;
using Category;
using UnityEngine;
using static CardAttributeDB;
using Unity.VisualScripting;
using System;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public GameObject cardPrefab;
    public GameObject cardSlotPrefab;
    public Transform cardSlotSet;
    public Canvas canvas;
    public Dictionary<CardType, List<Card>> allCards = new Dictionary<CardType, List<Card>>();
    public Dictionary<long, CardSlot> allCardSlots = new Dictionary<long, CardSlot>();
    public event Action<Card> onCardCreated;
    public event Action<Card> onCardDeleted;

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
            card.cardID = GetCardIdentityID();
            AddCardAttribute(card);
        }

        SceneManager.AfterSceneChanged += OnSceneChanged;
        SceneManager.BeforeSceneChanged += () =>
        {
            if (SceneManager.currentScene == SceneManager.ProductionScene)
            {
                SaveDataManager.Instance.SaveGame();
            }
        };

        InitProductionScene();
    }

    private void OnSceneChanged()
    {
        // Clear all cards
        foreach (var cardList in allCards.Values)
            cardList.Clear();
        allCardSlots.Clear();

        // Clear battle cards
        if (SceneManager.currentScene != SceneManager.BattleScene)
            battleSceneCreatureCardIDs.Clear();

        // Reinitialize the allCards dictionary
        if (SceneManager.currentScene == SceneManager.ProductionScene)
        {
            cardSlotSet = GameObject.FindGameObjectWithTag("CardSlotSet").transform;
            Debug.Log("Loading Production Scene Cards...");
            LoadProductionScene();
        }
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

    /// <summary>
    /// init the production scene from save file
    /// </summary>
    public void InitProductionScene()
    {
        string fileName;
        if (SaveDataManager.isNewGame)
            fileName = SaveDataManager.InitialSaveDataFileName;
        else
            fileName = SaveDataManager.SaveDataFileName;
        if (!SaveDataManager.Instance.TryGetSaveData(fileName, out SaveDataManager.SaveData saveData))
            return;
        Dictionary<long, Card> tmpCardsDict = new Dictionary<long, Card>();

        // Set all identity IDs
        CurCardID = saveData.curCardID;
        CurCardSlotID = saveData.curCardSlotID;

        // first create all cards
        foreach (var cardData in saveData.allCardData)
        {
            object attribute = cardData.cardDescription.cardType switch
            {
                CardType.Creatures => JsonUtility.FromJson<CreatureCardAttribute>(cardData.attribute),
                CardType.Resources => JsonUtility.FromJson<ResourceCardAttribute>(cardData.attribute),
                _ => null,
            };
            Card card = CreateCard(cardData.cardDescription, Vector2.zero, cardData.cardID, attribute);
            tmpCardsDict[card.cardID] = card;
        }

        // then create all card slots and place cards into slots
        foreach (var cardSlotData in saveData.allCardSlotData)
        {
            CardSlot cardSlot = CreateCardSlot(cardSlotData.position);
            foreach (var cardID in cardSlotData.cardIDs)
            {
                if (tmpCardsDict.TryGetValue(cardID, out var card))
                {
                    CardSlot.ChangeCardToSlot(card.cardSlot, cardSlot, card, null, true);
                }
                else
                {
                    Debug.LogError($"CardSlotData contains unknown cardID: {cardID}");
                }
            }
        }

        // TEST: unlock all the recipes
        unlockedCraftableRecipes = DataBaseManager.Instance.GetAllRecipes();

        // Init MainUI
        MainUIManager mainUIManager = FindObjectOfType<MainUIManager>();
        mainUIManager.InitMainUI();
    }

    /// <summary>
    /// 运行时方法，加载生产场景
    /// </summary>
    public void LoadProductionScene()
    {
        SaveDataManager.SaveData saveData = SaveDataManager.currentSaveData;
        Dictionary<long, Card> tmpCardsDict = new Dictionary<long, Card>();

        // Create all cards
        foreach (var cardData in saveData.allCardData)
        {
            object attribute = cardData.cardDescription.cardType switch
            {
                CardType.Creatures => GetCardAttribute<CreatureCardAttribute>(cardData.cardID),
                CardType.Resources => GetCardAttribute<ResourceCardAttribute>(cardData.cardID),
                _ => null,
            };
            if (attribute == null) continue;
            Card card = CreateCard(cardData.cardDescription, Vector2.zero, cardData.cardID, attribute);
            tmpCardsDict[card.cardID] = card;
        }

        // Create all card slots and place cards into slots
        foreach (var cardSlotData in saveData.allCardSlotData)
        {
            CardSlot cardSlot = CreateCardSlot(cardSlotData.position);
            foreach (var cardID in cardSlotData.cardIDs)
                if (tmpCardsDict.TryGetValue(cardID, out var card))
                {
                    CardSlot.ChangeCardToSlot(card.cardSlot, cardSlot, card, null, true);
                }
        }

        // Init MainUI
        MainUIManager mainUIManager = FindObjectOfType<MainUIManager>();
        mainUIManager.InitMainUI();
    }

    #region 卡牌逻辑内容管理    
    private static long cardIdentityID = 1;
    private static long cardSlotIdentityID = 1;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public Card draggingCard;
    public long CurCardID { get => cardIdentityID; private set => cardIdentityID = value; }
    public long CurCardSlotID { get => cardSlotIdentityID; private set => cardSlotIdentityID = value; }

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

    public Card CreateCard(Card.CardDescription cardDescription, Vector2 position = default, long cardID = -1, object attribute = null)
    {
        if (!cardDescription.IsValid())
        {
            Debug.LogError("CardManager: Invalid CardDescription");
            return null;
        }
        Card newCard = Instantiate(cardPrefab, position, Quaternion.identity).GetComponent<Card>();

        // Set the basic attr
        if (cardID == -1) cardID = GetCardIdentityID();
        newCard.cardID = cardID;
        newCard.SetCardType(cardDescription);

        // Add cardslot
        var newSlot = CreateCardSlot(position);
        CardSlot.ChangeCardToSlot(null, newSlot, newCard, null, true);

        // Add to manager
        allCards[newCard.cardDescription.cardType].Add(newCard);
        AddCardAttribute(newCard, attribute);

        onCardCreated?.Invoke(newCard);
        return newCard;
    }

    public void DeleteCard(Card card)
    {
        if (card.cardSlot != null)
            CardSlot.RemoveCard(card.cardSlot, card, false);

        allCards[card.cardDescription.cardType].Remove(card);
        RemoveCardAttribute(card);
        if (card.cardDescription.cardType == CardType.Events)
            StopTrackingUIEvent(card);

        onCardDeleted?.Invoke(card);
        Destroy(card.gameObject);
    }

    public CardSlot CreateCardSlot(Vector2 position)
    {
        var cardSlotObject = Instantiate(cardSlotPrefab, position, transform.rotation, cardSlotSet);
        // cardSlotObject.name = $"CardSlot_{cardID}";
        CardSlot cardSlot = cardSlotObject.GetComponent<CardSlot>();
        cardSlot.cardSlotID = GetCardSlotIdentityID();
        // Debug.Log($"Created CardSlot ID: {cardSlot.cardSlotID} at position {position}");
        allCardSlots[cardSlot.cardSlotID] = cardSlot;
        return cardSlot;
    }

    public void DeleteCardSlot(CardSlot cardSlot)
    {
        // Debug.Log($"Deleting CardSlot ID: {cardSlot.cardSlotID}");
        allCardSlots.Remove(cardSlot.cardSlotID);

        if (cardSlot.cards.Count > 0)
        {
            CardSlot.RemoveCards(cardSlot, cardSlot.cards, true);
        }

        Destroy(cardSlot.gameObject);
    }

    #endregion

    # region 卡牌合成表管理
    List<CraftTableDB.Recipe> unlockedCraftableRecipes = new List<CraftTableDB.Recipe>();
    public IReadOnlyList<CraftTableDB.Recipe> GetUnlockedCraftableRecipes()
        => unlockedCraftableRecipes.AsReadOnly();
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
                // Debug.Log($"Added satiety attribute for creature card {card.name} : {creatureCardAttributes[card.cardID]?.basicAttributes.satiety}");
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
    public bool TryGetCardAttribute<T>(long cardID, out T attribute) where T : class
    {
        attribute = GetCardAttribute<T>(cardID);
        return attribute != null;
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