using System;
using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardAttributeDB;

public abstract class SettlementCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    protected Canvas canvas;
    protected Image cardImage;
    [HideInInspector] public GameObject cardSlot;
    protected Text satietyText;
    protected object type;
    public DisplayCard displayCard;

    // Events for pointer interactions
    [HideInInspector] public event Action<SettlementCard> PointerEnterEvent;
    [HideInInspector] public event Action<SettlementCard> PointerExitEvent;
    [HideInInspector] public event Action<SettlementCard> BeginDragEvent;
    [HideInInspector] public event Action<SettlementCard> EndDragEvent;
    [HideInInspector] public event Action<SettlementCard> PointerClickEvent;
    // protected List<Image> cardImages = new List<Image>();

    [Header("Card Data")]
    public long cardID;

    // APIs
    public int ParentIndex() => transform.parent.GetSiblingIndex();
    public abstract void InitCard(long cardID);

    protected void Awake()
    {
        cardImage = GetComponent<Image>();
        canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        satietyText = transform.Find("Satiety").GetComponentInChildren<Text>();
        displayCard = GetComponentInChildren<DisplayCard>();
    }

    protected void Start()
    {
        transform.rotation = Camera.main.transform.rotation;
        cardSlot = transform.parent.gameObject;
    }

    protected void SetCardImage()
    {
        // Set the card images
        Card.CardDescription cardDescription = new Card.CardDescription();
        if (this is SC_Battle || this is SC_Creature)
        {
            cardDescription.cardType = CardType.Creatures;
            cardDescription.creatureCardType = (CreatureCardType)type;
        }
        else if (this is SC_Food)
        {
            cardDescription.cardType = CardType.Resources;
            cardDescription.resourceCardType = (ResourceCardType)type;
        }
        else
            Debug.LogError($"Unknown card subclass {this.GetType()} for setting card images.");

        displayCard.Initialize(cardDescription);
    }

    # region Unity Event Handlers
    public void OnBeginDrag(PointerEventData eventData)
    {
        cardImage.raycastTarget = false;
        BeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cardImage.raycastTarget = true;
        transform.position = cardSlot.transform.position;
        EndDragEvent?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClickEvent?.Invoke(this);
    }
    #endregion
}