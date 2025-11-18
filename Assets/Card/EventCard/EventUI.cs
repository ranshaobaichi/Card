using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventUI : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static int openEventUICount = 0;
    public GameObject displayCardPrefab;
    public GameObject optionButtonPrefab;

    public GameObject cardDisplayPanel;
    public GameObject optionPanel;
    private Canvas _canvas;
    public Button closeBtn;
    public Text descriptionText;
    public Text nameText;
    public Card eventCard;

    private Vector2 _pointerOffset;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        openEventUICount++;
        closeBtn.onClick.AddListener(() => Destroy(gameObject));
    }

    void OnDestroy()
    {
        openEventUICount--;
    }

    public void Initialize(Card card, int optIndex = -1, float progress = -1f)
    {
        eventCard = card;
        var eventUIAttribute = DataBaseManager.Instance.GetEventUIAttribute(card.cardDescription.eventCardType) ?? default;

        nameText.text = card.cardDescription.ToString();

        descriptionText.text = eventUIAttribute.descriptionText;
        foreach (var optionAttr in eventUIAttribute.options)
        {
            var optionBtnGO = Instantiate(optionButtonPrefab, optionPanel.transform);
            var optionBtn = optionBtnGO.GetComponent<E_OptionButton>();
            optionBtn.Initialize(this, optionAttr, eventUIAttribute.options.IndexOf(optionAttr));
        }

        // Set the position to mouse position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), Input.mousePosition, _canvas.worldCamera, out Vector3 mousePos);
        transform.position = mousePos;

        RefreshDisplayCards();
    }

    public void RefreshDisplayCards()
    {
        var cards = eventCard.cardSlot.cards;
        foreach (var card in cards)
        {
            if (card.cardDescription.cardType != Category.CardType.Events)
            {
                var cardGO = Instantiate(displayCardPrefab, cardDisplayPanel.transform);
                cardGO.transform.localScale = 0.35f * Vector3.one;
                var displayCard = cardGO.GetComponent<DisplayCard>();
                displayCard.Initialize(card.cardDescription);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos - (Vector3)_pointerOffset;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        _pointerOffset = mousePos - new Vector3(transform.position.x, transform.position.y);
    }

}