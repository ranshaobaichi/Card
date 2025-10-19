using System;
using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
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
                CardType.Creatures => creatureCardType.ToString(),
                CardType.Resources => resourceCardType.ToString(),
                CardType.Events => eventCardType.ToString(),
                _ => "Unknown"
            };
        }
    }
    protected Transform cardSlotSet;
    public Card preCard, laterCard;
    protected Image cardImage;
    public CardSlot cardSlot;
    private CardSlot movingCardSlot;
    private GameObject canvas;
    public GameObject cardSlotPrefab;
    public event Action<Card> OnCardDeleted;

    [Header("拖拽与对齐设置")]
    [Range(5f, 10f)][Tooltip("卡牌跟随速度")] public float followSpeed;
    [Tooltip("卡牌容忍距离")] public float tolerantDistance = 20.0f;
    [Tooltip("卡牌Y轴对齐距离")] public float yAlignedDistance = 40.0f;
    [Range(1.0f, 1.2f)][Tooltip("卡牌拖动时放大系数")] public float dragScaleFactor = 1.05f;
    protected Vector3 initPosition;

    [Header("描边设置")]
    public Material outlineMaterial;
    protected Material outlineMaterialInstance;
    [Tooltip("卡牌描边移动速度")] public float outlineMoveSpeed = 1;
    [Tooltip("卡牌可放置提示色")] public Color successOutlineColor = Color.green;
    [Tooltip("卡牌不可放置提示色")] public Color failureOutlineColor = Color.red;
    protected float outlineOffset;

    [Header("卡牌设置")]
    public CardDescription cardDescription;
    [Tooltip("是否可以被拖动")] public bool canBeDragged;
    [Tooltip("是否可以被放置")] public bool canBePlaced;
    [Tooltip("是否可以放置在其他卡牌上")] public bool canPlaceOnCard;
    public int durability;
    protected long cardID;
    protected bool isMoving;

    public void SetCardID(long id) => cardID = id;
    public long GetCardID() => cardID;
    public void SetCardType(CardDescription description) => cardDescription = description;
    public string GetCardTypeString() => cardDescription.ToString();

    protected void Awake()
    {
        cardImage = GetComponent<Image>();
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        movingCardSlot = GameObject.FindGameObjectWithTag("MovingCardSlot").GetComponent<CardSlot>();
        cardSlotSet = GameObject.FindGameObjectWithTag("CardSlotSet").transform;
    }

    protected void Start()
    {
        // Initialize card parameters
        preCard = laterCard = null;
        cardSlot = null;
        // cardID = CardManager.Instance.GetCardIdentityID();
        isMoving = false;
        durability = cardDescription.cardType switch
        {
            // CardType.Resources => CardManager.Instance.cardAttributeDB.GetDurability(cardDescription.resourceCardType),
            CardType.Resources => 1,
            CardType.Creatures => 99999,
            CardType.Events => 99999,
            _ => throw new NotSupportedException("Unsupported card type for durability initialization"),
        };

        // Set the initial cardslot
        var originalCardSlot = CreateNewSlot();
        UpdateCardSlot(originalCardSlot);
        originalCardSlot.name = $"CardSlot_{cardID}";

        // Set the material
        cardImage.material = null;
        outlineMaterialInstance = new Material(outlineMaterial);
        outlineOffset = outlineMaterialInstance.GetFloat("_DashOffset");

        // TEST: Set the word on the card
        var textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = GetCardTypeString();
        }
    }

    protected void FollowPosition()
    {
        if (preCard == null)
        {
            CancelInvoke(nameof(FollowPosition));
            return;
        }
        Vector3 targetPosition = preCard.transform.position - new Vector3(0, yAlignedDistance, 0);
        var pos = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * followSpeed);
        transform.position = pos;

        if (isMoving == false && transform.position == targetPosition)
        {
            CancelInvoke(nameof(FollowPosition));
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Set the self state and global dragging state
        isMoving = true;
        canBePlaced = true;
        cardImage.raycastTarget = false;
        CardManager.Instance.draggingCard = this;
        CardManager.Instance.isDragging = true;
        cardSlot.EndProduction();

        // Update card slot information
        UpdateCardSlot(movingCardSlot);
        cardSlot.UpdateMovingState(this, true);

        // Several initializations
        foreach (var card in GetFollowingCards())
        {
            card.transform.localScale = dragScaleFactor * Vector3.one;
        }
        initPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the mouse position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool endOnCard = false;
        // if put on other cards, place it after the card and inherit its cardslot
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

            if (hitObject.CompareTag("Card"))
            {
                // Debug.Log($"Hit card: {hitObject.name}");
                Card card = hitObject.GetComponent<Card>();
                // Can't place on the card at the end position
                if ((card.canBePlaced == false && card.cardID != cardID) || canPlaceOnCard == false)
                {
                    transform.position = initPosition;
                }
                // Can place on the card
                else if (card.CanBePlacedOn(this))
                {
                    // Debug.Log($"Placing {name} after {card.name}");
                    CardSlot.ChangeCardToSlot(cardSlot, card.cardSlot, this);
                    endOnCard = true;
                    Vector2 targetPosition = card.transform.position - new Vector3(0, yAlignedDistance, 0);
                    transform.position = targetPosition;
                    // Debug.Log($"PreCard is {preCard}");
                }
            }
        }

        // if put on empty space, create a new slot
        if (endOnCard == false)
        {
            Debug.Log($"Put on blank, creating new slot for {name}");
            UpdateCardSlot(CreateNewSlot());
            cardSlot.name = $"CardSlot_{cardID}";
        }

        // Reset the card's state
        CardManager.Instance.draggingCard = null;
        CardManager.Instance.isDragging = false;
        cardImage.raycastTarget = true;
        cardSlot.UpdateMovingState(this, false);
        foreach (var card in GetFollowingCards())
        {
            card.transform.localScale = Vector3.one;
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
            InvokeRepeating(nameof(FollowPosition), 0f, 0.01f);
        }
        else
        {
            setShadow();
            cardImage.raycastTarget = true;
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

    public CardSlot CreateNewSlot()
    {
        var cardSlotObject = Instantiate(cardSlotPrefab, transform.position, transform.rotation, cardSlotSet);
        // cardSlotObject.name = $"CardSlot_{cardID}";
        return cardSlotObject.GetComponent<CardSlot>();
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
            draggingCard.cardID != cardID
            )
        {
            SetOutline(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetOutline(false);
    }

    public void SetOutline(bool show)
    {
        if (show)
        {
            // Show outline
            cardImage.material = outlineMaterialInstance;

            if (CanBePlacedOn(CardManager.Instance.draggingCard))
                outlineMaterialInstance.SetColor("_OutlineColor", successOutlineColor);
            else
                outlineMaterialInstance.SetColor("_OutlineColor", failureOutlineColor);

            InvokeRepeating(nameof(DisplayOutline), 0f, 0.016f * 15);
        }
        else
        {
            // Hide outline
            cardImage.material = null;
            CancelInvoke(nameof(DisplayOutline));
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

    public bool CanBePlacedOn(Card card)
    {
        // Check if the card can be placed on the target card
        bool basicCheck = card.canBePlaced == true && card.cardID != cardID && card.canPlaceOnCard == true;

        // Event card can't put on another event card
        // bool eventCardCheck = !(cardSlot.TryGetEventCard(out _) && card.cardSlot.TryGetEventCard(out _));
        bool eventCardCheck = true;

        return basicCheck && eventCardCheck;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if (cardSlot.TryGetEventCard(out Card eventCard))
        // {
        //     CardManager.Instance.PopUpEventUI(eventCard);
        // }
        if (cardDescription.cardType == CardType.Events)
            CardManager.Instance.PopUpEventUI(this);
    }

    public void DeleteCard()
    {
        OnCardDeleted?.Invoke(this);
        Destroy(gameObject);
    }
}