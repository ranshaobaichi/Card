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
    public Text nameText;
    [HideInInspector] public GameObject cardSlot;
    protected Text satietyText;
    protected Text foodValueText;

    // Events for pointer interactions
    [HideInInspector] public event Action<SettlementCard> PointerEnterEvent;
    [HideInInspector] public event Action<SettlementCard> PointerExitEvent;
    [HideInInspector] public event Action<SettlementCard> BeginDragEvent;
    [HideInInspector] public event Action<SettlementCard> EndDragEvent;
    [HideInInspector] public event Action<SettlementCard> PointerClickEvent;
    protected List<Image> cardImages = new List<Image>();

    [Header("Card Data")]
    public long cardID;

    // APIs
    public int ParentIndex() => transform.parent.GetSiblingIndex();
    public abstract void InitCard(long cardID);

    protected void Awake()
    {
        cardImage = GetComponent<Image>();
        canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        nameText = transform.Find("NameText").GetComponent<Text>();
        satietyText = transform.Find("Satiety").GetComponentInChildren<Text>();
        foodValueText = transform.Find("Images").GetComponentInChildren<Text>();

        // Gather card images
        GameObject cardImagesParent = transform.Find("Images").gameObject;
        if (cardImagesParent != null)
        {
            foreach (Transform child in cardImagesParent.transform)
            {
                Image img = child.GetComponent<Image>();
                    cardImages.Add(img);
            }
        }
        // Debug.Log($"Card images count: {cardImages.Count}");
    }

    protected void Start()
    {
        transform.rotation = Camera.main.transform.rotation;
        cardSlot = transform.parent.gameObject;
    }

    protected void SetCardImage()
    {
        // Set the card images
        CardType cardType = CardType.None;
        if (this is SC_Battle || this is SC_Creature)
            cardType = CardType.Creatures;
        else if (this is SC_Food)
            cardType = CardType.Resources;
        else
            Debug.LogError($"Unknown card subclass {this.GetType()} for setting card images.");

        ResourceCardClassification resourceClassification = cardType == CardType.Resources ? CardManager.Instance.GetCardAttribute<ResourceCardAttribute>(cardID).resourceClassification : ResourceCardClassification.None;
        var succ = CardManager.Instance.TryGetCardIconAttribute(cardType, out var cardIconAttrribute, resourceClassification);
        if (succ)
        {
            cardImages[0].sprite = cardIconAttrribute.background;
            cardImages[1].sprite = cardIconAttrribute.side;
            cardImages[2].sprite = cardIconAttrribute.illustration;
            cardImages[3].sprite = cardIconAttrribute.top;
            cardImages[4].sprite = cardIconAttrribute.bottom;
            cardImages[5].sprite = cardIconAttrribute.type;
        }
        else
        {
            Debug.LogError($"Card icon attribute not found for card ID {cardID} of type {cardType}.");
        }
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