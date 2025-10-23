using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SettlementCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected Canvas canvas;
    protected Image cardImage;
    public Text text;
    [HideInInspector] public GameObject cardSlot;

    // Events for pointer interactions
    [HideInInspector] public event Action<SettlementCard> PointerEnterEvent;
    [HideInInspector] public event Action<SettlementCard> PointerExitEvent;
    // [HideInInspector] public event Action<SettlementCard, bool> PointerUpEvent;
    // [HideInInspector] public event Action<SettlementCard> PointerDownEvent;
    [HideInInspector] public event Action<SettlementCard> BeginDragEvent;
    [HideInInspector] public event Action<SettlementCard> EndDragEvent;
    // [HideInInspector] public event Action<SettlementCard, bool> SelectEvent;

    [Header("Card Data")]
    public long cardID;

    // APIs
    public int ParentIndex() => transform.parent.GetSiblingIndex();
    public abstract void InitCard(long cardID);

    protected void Awake()
    {
        cardImage = GetComponent<Image>();
        canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        text = GetComponentInChildren<Text>();
    }

    protected void Start()
    {
        cardSlot = transform.parent.gameObject;
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
    # endregion
}