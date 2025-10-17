using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public GameObject cardPrefab;
    public Canvas canvas;

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
    }

    #region 卡牌逻辑内容管理
    private long cardIdentityID;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public Card draggingCard;

    public void Start()
    {
        cardIdentityID = 1;
        isDragging = false;
        draggingCard = null;
    }

    public long GetCardIdentityID()
    {
        long newID = cardIdentityID++;
        return newID % long.MaxValue;
    }

    public Card CreateCard(Card.CardDescription cardDescription, Vector2 position = default)
    {
        if (!cardDescription.IsValid())
        {
            Debug.LogError("CardManager: Invalid CardDescription");
            return null;
        }
        Card newCard = Instantiate(cardPrefab, position, Quaternion.identity).GetComponent<Card>();
        newCard.SetCardType(cardDescription);
        return newCard;
    }
    #endregion

    #region 合成表管理
    [Header("合成表管理")]
    public CraftTableDB craftTableDB;
    public CraftTableDB.Recipe? GetRecipe(int id) => craftTableDB.GetRecipe(id);
    public CraftTableDB.Recipe? GetRecipe(string name) => craftTableDB.GetRecipe(name);
    public (List<Card>, CraftTableDB.Recipe)? GetRecipe(List<Card> inputCards) => craftTableDB.GetRecipe(inputCards);
    #endregion

    #region 卡牌属性管理
    [Header("卡牌属性管理")]
    public CardAttributeDB cardAttributeDB;

    public WorkEfficiencyType GetWorkEfficiencyType(CreatureCardType creatureCardType)
        => cardAttributeDB.creatureCardAttributes.ContainsKey(creatureCardType) ?
            cardAttributeDB.creatureCardAttributes[creatureCardType].craftWorkEfficiency :
            WorkEfficiencyType.None;

    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
        => cardAttributeDB.workEfficiencyValues.ContainsKey(workEfficiencyType) ?
            cardAttributeDB.workEfficiencyValues[workEfficiencyType] :
            0.0f;

    public float GetWorkEfficiencyValue(CreatureCardType creatureCardType)
        => GetWorkEfficiencyValue(GetWorkEfficiencyType(creatureCardType));

    public bool IsResourcePoint(ResourceCardType resourceCardType)
        => cardAttributeDB.resourceCardAttributes.ContainsKey(resourceCardType) && cardAttributeDB.resourceCardAttributes[resourceCardType].isResourcePoint;

    #endregion

    #region 事件卡UI管理
    [Header("事件卡UI管理")]
    public EventCardUIDB eventCardUIDB;
    public Transform eventUIParent;
    public Vector2 eventUIoffset;
    public Vector2 UIthreshold;
    private Dictionary<Card, EventUI> EventUIs = new Dictionary<Card, EventUI>();

    public bool TryGetEventCardUIPrefab(EventCardType eventCardType, out GameObject prefab)
        => eventCardUIDB.TryGetEventCardUIPrefab(eventCardType, out prefab);

    public void PopUpEventUI(Card card)
    {
        card.cardSlot.EndProduction();
        EventUI eventUI = EventUIs.GetValueOrDefault(card, null);

        if (eventUI == null)
        {
            if (!TryGetEventCardUIPrefab(card.cardType.eventCardType, out GameObject prefab))
            {
                Debug.LogError($"No EventUI prefab found for EventCardType: {card.cardType.eventCardType}");
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
    #endregion
}