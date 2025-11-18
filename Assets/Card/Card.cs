using System;
using System.Collections;
using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime0_01 = new WaitForSecondsRealtime(0.01f);

    [Serializable]
    public struct CardDescription
    {
        [Tooltip("卡牌所属类型")] public CardType cardType;
        [Tooltip("资源卡类型")] public ResourceCardType resourceCardType;
        [Tooltip("生物卡类型")] public CreatureCardType creatureCardType;
        [Tooltip("事件卡类型")] public EventCardType eventCardType;
        public readonly bool IsValid()
        {
            return cardType switch
            {
                CardType.Creatures => creatureCardType != CreatureCardType.None,
                CardType.Resources => resourceCardType != ResourceCardType.None,
                CardType.Events => eventCardType != EventCardType.None,
                _ => false,
            };
        }
        public override string ToString()
        {
            return cardType switch
            {
                CardType.Creatures => creatureCardType == CreatureCardType.Any ? "任意生物" : creatureCardType.ToString(),
                CardType.Resources => resourceCardType.ToString(),
                CardType.Events => eventCardType.ToString(),
                _ => "Unknown"
            };
        }
        public override bool Equals(object obj)
        {
            if (obj is CardDescription other)
            {
                return cardType == other.cardType &&
                       cardType switch
                       {
                           CardType.Creatures => creatureCardType == other.creatureCardType,
                           CardType.Resources => resourceCardType == other.resourceCardType,
                           CardType.Events => eventCardType == other.eventCardType,
                           _ => true,
                       };
            }
            return false;
        }
        public static bool operator ==(CardDescription a, CardDescription b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(CardDescription a, CardDescription b)
        {
            return !a.Equals(b);
        }
    }
    public Card preCard = null, laterCard = null;
    protected Image cardImage;
    public CardSlot cardSlot;
    private CardSlot movingCardSlot;
    private GameObject canvas;
    public GameObject cardSlotPrefab;

    [Header("拖拽与对齐设置")]
    [Tooltip("卡牌跟随速度")] public float followSpeed;
    [Tooltip("卡牌Y轴对齐距离")] public float yAlignedDistance = 40.0f;
    [Range(1.0f, 1.2f)] [Tooltip("卡牌拖动时放大系数")] public float dragScaleFactor = 1.05f;
    protected Vector3 initPosition;

    [Header("描边设置")]
    public Material outlineMaterial;
    protected Material outlineMaterialInstance;
    [Tooltip("卡牌描边移动速度")] public float outlineMoveSpeed = 1;
    [Tooltip("卡牌可放置提示色")] public Color successOutlineColor = Color.green;
    [Tooltip("卡牌不可放置提示色")] public Color failureOutlineColor = Color.red;
    protected float outlineOffset;
    [Header("合成指示")]
    public GameObject craftTooltipPanel;
    public Text craftTooltipNameText;
    public Text asInputText, asOutputText;

    [Header("卡牌设置")]
    public CardDescription cardDescription;
    public Card preCardBeforeDrag;
    [Tooltip("是否可以被拖动")] public bool canBeDragged;
    [Tooltip("是否可以被放置")] public bool canBePlaced;
    [Tooltip("是否可以放置在其他卡牌上")] public bool canPlaceOnCard;
    public int durability
    {
        get => cardDescription.cardType switch
        {
            CardType.Resources => CardManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardID).durability,
            CardType.Creatures => 999,
            CardType.Events => 999,
            _ => throw new Exception("Invalid card type for durability retrieval."),
        };
        set
        {
            if (cardDescription.cardType == CardType.Resources)
            {
                var resourceAttr = CardManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardID);
                resourceAttr.durability = value;
            }
        }
    }
    public long cardID;
    protected bool isMoving;
    public DisplayCard displayCard;
    
    // 添加协程引用
    private Coroutine displayOutlineCoroutine;

    public void SetCardType(CardDescription description) => cardDescription = description;
    public string GetCardTypeString() => cardDescription.ToString();

    #region 静态接口
    public static bool CanPlacedOn(Card card, Card cardToPlace)
    {
        // Check if the card can be placed on the target card
        bool basicCheck = cardToPlace.canBePlaced == true &&
                            card.cardSlot.cardSlotID != cardToPlace.cardSlot.cardSlotID &&
                            card.canPlaceOnCard == true;

        // Event card can't put on another event card
        // bool eventCardCheck = !(cardSlot.TryGetEventCard(out _) && card.cardSlot.TryGetEventCard(out _));
        bool eventCardCheck = true;

        return basicCheck && eventCardCheck;
    }

    #endregion

    protected void Awake()
    {
        cardImage = GetComponent<Image>();
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        movingCardSlot = GameObject.FindGameObjectWithTag("MovingCardSlot").GetComponent<CardSlot>();
    }

    protected void Start()
    {
        // Initialize card parameters
        isMoving = false;
        this.name = $"Card_{cardID}";

        // // Set the initial cardslot
        // var originalCardSlot = CardManager.Instance.CreateCardSlot(transform.position);
        // CardSlot.ChangeCardToSlot(cardSlot, originalCardSlot, this);

        // Set the material
        cardImage.material = null;
        outlineMaterialInstance = new Material(outlineMaterial);
        outlineOffset = outlineMaterialInstance.GetFloat("_DashOffset");

        // Set the name text
        craftTooltipNameText.text = cardDescription.ToString();
        var craftTables = DataBaseManager.Instance.GetAllRecipes();
        string input = "", output = "";
        foreach (var recipe in craftTables)
        {
            if (recipe.inputCards.Contains(cardDescription))
            {
                input += $"- {recipe.recipeName}\n";
            }
            foreach (var drop in recipe.outputCards)
            {
                if (drop.cardDescription == cardDescription)
                {
                    output += $"- {recipe.recipeName}\n";
                }
                break;
            }
        }
        asInputText.text = input == "" ? "无" : input.TrimEnd('\n');
        asOutputText.text = output == "" ? "无" : output.TrimEnd('\n');
    }

    protected void Update()
    {
        if (isMoving && preCard != null)
        {
            FollowPosition();
        }

        if (craftTooltipPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            var rect = transform as RectTransform;
            var _canvas = rect.GetComponentInParent<Canvas>();
            var cam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;
            bool isPointerOver = RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
            if (!isPointerOver) craftTooltipPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (cardDescription.cardType == CardType.Events)
        {
            SceneManager.BeforeSceneChanged += SaveOption;
        }
    }

    void OnDisable()
    {
        if (cardDescription.cardType == CardType.Events)
        {
            SceneManager.BeforeSceneChanged -= SaveOption;
        }
    }

    protected void FollowPosition()
    {
        Vector3 targetPosition = preCard.transform.position - new Vector3(0, yAlignedDistance, 0);
        var pos = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * followSpeed);
        // Debug.Log($"Following position: current={transform.position}, target={targetPosition}, new={pos}");
        transform.position = pos;

        if (Vector3.Distance(transform.position, targetPosition) <= 2f && !CardManager.Instance.isDragging && !preCard.isMoving)
        {
            // Debug.Log($"Card {cardDescription} reached target position.");
            transform.position = targetPosition;
            isMoving = false;
            cardImage.raycastTarget = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canBeDragged == false || CardManager.Instance.isDragging == true || isMoving == true)
            return;
        // Set the self state and global dragging state
        cardImage.raycastTarget = false;
        CardManager.Instance.draggingCard = this;
        CardManager.Instance.isDragging = true;
        cardSlot.EndProduction();

        // Record the pre-card and initial position
        preCardBeforeDrag = preCard;
        initPosition = transform.position;

        // Update card slot information
        CardSlot.ChangeCardsToSlot(cardSlot, movingCardSlot, GetFollowingCards());

        // Several initializations
        foreach (var card in GetFollowingCards())
        {
            card.transform.localScale = dragScaleFactor * Vector3.one * 0.6f;
        }
        ChangeMovingState(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the mouse position
        if (CardManager.Instance.isDragging == false)
            return;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CardManager.Instance.isDragging == false)
            return;
        bool endOnCard = false;
        // if put on other cards, place it after the card and inherit its cardslot
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

            if (hitObject.CompareTag("Card"))
            {
                endOnCard = true;
                // Debug.Log($"Hit card: {hitObject.name}");
                Card card = hitObject.GetComponent<Card>();
                // Can place on the card
                if (CanPlacedOn(this, card))
                {
                    transform.position = card.transform.position - new Vector3(0, yAlignedDistance, 0);
                    CardSlot.ChangeCardsToSlot(cardSlot, card.cardSlot, GetFollowingCards(), card, false);
                }
                else // Put to original cardSlot or create a new slot
                {
                    transform.position = initPosition - new Vector3(0, yAlignedDistance, 0);
                    CardSlot.ChangeCardsToSlot(cardSlot, preCardBeforeDrag == null ? CardManager.Instance.CreateCardSlot(initPosition) : preCardBeforeDrag.cardSlot, GetFollowingCards(), preCardBeforeDrag, false);
                }
            }
        }

        // if put on empty space, create a new slot
        if (endOnCard == false)
        {
            // Debug.Log($"Put on blank, creating new slot for {name}");
            CardSlot tmp = CardManager.Instance.CreateCardSlot(transform.position);
            CardSlot.ChangeCardsToSlot(cardSlot, tmp, GetFollowingCards(), null, false);
        }

        // Reset the card's state
        CardManager.Instance.draggingCard = null;
        CardManager.Instance.isDragging = false;
        cardImage.raycastTarget = true;
        preCardBeforeDrag = null;
        initPosition = Vector3.zero;
        ChangeMovingState(false);
        foreach (var card in GetFollowingCards())
        {
            card.transform.localScale = Vector3.one * 0.6f;
        }
    }

    public void ChangeMovingState(bool state)
    {
        var setShadow = new Action(() => GetComponent<Shadow>().enabled = state);
        if (state == true)
        {
            isMoving = true;
            cardImage.raycastTarget = false;
            setShadow();
            if (laterCard != null)
            {
                laterCard.ChangeMovingState(true);
            }
        }
        else
        {
            setShadow();
            // Debug.Log($"Card {cardDescription} stopped moving.");
            isMoving = false;
        }
    }

    /// <summary>
    /// Updates the card slot for this card and all the following cards.
    /// </summary>
    /// <param name="newSlot"></param>
    public void UpdateCardSlot(CardSlot newSlot)
    {
        if (cardSlot == newSlot)
            return;
        List<Card> followingCards = GetFollowingCards();
        CardSlot.ChangeCardsToSlot(cardSlot, newSlot, followingCards);
    }

    protected List<Card> GetFollowingCards()
    {
        List<Card> followingCards = new List<Card>();
        Card currentCard = this;
        while (currentCard != null)
        {
            followingCards.Add(currentCard);
            currentCard = currentCard.laterCard;
        }
        return followingCards;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Card draggingCard = CardManager.Instance.draggingCard;
        if (CardManager.Instance.isDragging &&
            draggingCard != null &&
            CanPlacedOn(draggingCard, this)
            )
        {
            SetOutline(true);
            cardSlot.ShowCraftingTooltip(draggingCard);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetOutline(false);
        cardSlot.HideCraftingTooltip();
    }

    public void SetOutline(bool show)
    {
        if (show)
        {
            // Show outline
            cardImage.material = outlineMaterialInstance;

            if (CanPlacedOn(CardManager.Instance.draggingCard, this))
                outlineMaterialInstance.SetColor("_OutlineColor", successOutlineColor);
            else
                outlineMaterialInstance.SetColor("_OutlineColor", failureOutlineColor);

            if (displayOutlineCoroutine != null)
                StopCoroutine(displayOutlineCoroutine);
            displayOutlineCoroutine = StartCoroutine(DisplayOutlineCoroutine());
        }
        else
        {
            // Hide outline
            cardImage.material = null;
            if (displayOutlineCoroutine != null)
            {
                StopCoroutine(displayOutlineCoroutine);
                displayOutlineCoroutine = null;
            }
        }
    }

    private IEnumerator DisplayOutlineCoroutine()
    {
        while (true)
        {
            DisplayOutline();
            yield return new WaitForSecondsRealtime(0.016f * 15);
        }
    }

    public void DisplayOutline()
    {
        if (outlineMaterialInstance != null)
        {
            outlineOffset += outlineMoveSpeed % 10.0f;
            outlineMaterialInstance.SetFloat("_DashOffset", outlineOffset);
        }
    }

    protected bool firstShow = true;
    public void OnPointerClick(PointerEventData eventData)
    {
        // if (cardSlot.TryGetEventCard(out Card eventCard))
        // {
        //     CardManager.Instance.PopUpEventUI(eventCard);
        // }
        switch (cardDescription.cardType)
        {
            case CardType.Resources:
                craftTooltipPanel.SetActive(true);
                if (firstShow)
                {
                    firstShow = false;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(craftTooltipPanel.GetComponent<RectTransform>());
                }
                break;
            case CardType.Creatures:
                CardManager.Instance.DisplayCreatureAttribute(this);
                break;
            case CardType.Events:
                CardManager.Instance.PopUpEventUI(this);
                break;
            default:
                break;
        }
    }

    //// UI Functions for Event Card ////
    /// may be better to put them in child class ///
    public int curWorkOptionIndex = -1;
    public float eventProgress = 0f;
    public void StartEvent(int optionIndex, float progress = 0f, EventUI eventUI = null)
    {
        curWorkOptionIndex = optionIndex;
        var eventUIAttribute = DataBaseManager.Instance.GetEventUIAttribute(cardDescription.eventCardType) ?? default;
        cardSlot.StartProgressBar(eventUIAttribute.options[optionIndex].cost.timeCost, () => {
            OnEndEvent(eventUI);
        });
        cardSlot.progressBar.SetProgressValue(progress);
    }
    public void EndEvent()
    {
        if (curWorkOptionIndex == -1)
        {
            Debug.LogError("No option selected for event card!");
            return;
        }
        cardSlot.EndProduction();
    }

    public void OnEndEvent(EventUI eventUI)
    {
        // Produce rewards
        var eventUIAttribute = DataBaseManager.Instance.GetEventUIAttribute(cardDescription.eventCardType) ?? default;
        foreach (var reward in eventUIAttribute.options[curWorkOptionIndex].rewards)
        {
            for (int i = 0; i < reward.dropCount; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 50f;
                CardManager.Instance.CreateCard(reward.cardDescription, transform.position + (Vector3)randomOffset);
            }
        }

        // Destroy event card
        cardSlot.EndProduction();
        if (eventUI != null) Destroy(eventUI.gameObject);
        CardManager.Instance.DeleteCard(this);
    }

    public void SaveOption()
    {
        if (curWorkOptionIndex != -1)
        {
            CardManager.Instance.eventCardProgress[cardID] = (curWorkOptionIndex, cardSlot.progressBar.progress);
        }
    }
}