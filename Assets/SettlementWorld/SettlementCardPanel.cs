using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SettlementCardPanel : MonoBehaviour
{
    public GameObject CardSlotPrefab => SettlementCardManager.Instance.settlementCardSlotPrefab;
    public Transform contentTransform;
    public List<SettlementCard> cards;
    public event Action onEndDrag;
    void OnEnable()
    {
        // // TEST: directly get the cards
        // foreach (SettlementCard card in gameObject.GetComponentsInChildren<SettlementCard>())
        // {
        //     cards.Add(card);
        //     card.BeginDragEvent += BeginDrag;
        //     card.EndDragEvent += EndDrag;
        //     card.PointerEnterEvent += OnCardHovered;
        //     card.PointerExitEvent += OnCardUnhovered;
        // }        
    }

    #region Card Events
    private void BeginDrag(SettlementCard card)
    {
        SettlementCardManager.Instance.draggingCard = card;
        SettlementCardManager.Instance.hoveredCard = null;
        card.transform.SetParent(SettlementCardManager.Instance.draggingCardSlot.transform);
    }

    private void EndDrag(SettlementCard card)
    {
        card.transform.SetParent(card.cardSlot.transform);
        onEndDrag?.Invoke();
    }

    private void OnCardHovered(SettlementCard card)
    {
        SettlementCardManager.Instance.hoveredCard = card;
    }

    private void OnCardUnhovered(SettlementCard card)
    {
        SettlementCardManager.Instance.hoveredCard = null;
    }

    private void OnCardClicked(SettlementCard card)
    {
        SettlementCardManager.Instance.OnCardClicked(card);
    }
    # endregion

    private void Update()
    {
        SettlementCard selectedCard = SettlementCardManager.Instance.draggingCard;
        if (cards.Contains(selectedCard) == false)
            return;

        int index = cards.IndexOf(selectedCard);
        if (index != 0 && selectedCard.transform.position.x < cards[index - 1].transform.position.x)
        {
            Swap(index - 1);
        }
        else if (index != cards.Count - 1 && selectedCard.transform.position.x > cards[index + 1].transform.position.x)
        {
            Swap(index + 1);
        }
    }

    void Swap(int index)
    {
        SettlementCard selectedCard = SettlementCardManager.Instance.draggingCard;
        GameObject draggingCardSlot = selectedCard.cardSlot;
        GameObject targetCardSlot = cards[index].cardSlot;

        cards[index].cardSlot = draggingCardSlot;
        cards[index].transform.SetParent(draggingCardSlot.transform);
        cards[index].transform.position = draggingCardSlot.transform.position;
        selectedCard.cardSlot = targetCardSlot;

        cards.TrySwap(index, cards.IndexOf(selectedCard), out _);
    }

    public T AddCard<T>(long cardID) where T : SettlementCard
    {
        // Create and initialize the card
        GameObject slot = Instantiate(CardSlotPrefab, contentTransform);
        // BUG: hardcoded to get the first child as the card
        GameObject card = slot.transform.GetChild(0).gameObject;
        T newCard = card.AddComponent<T>();
        newCard.InitCard(cardID);

        // Set the card
        cards.Add(newCard);
        newCard.BeginDragEvent += BeginDrag;
        newCard.EndDragEvent += EndDrag;
        newCard.PointerEnterEvent += OnCardHovered;
        newCard.PointerExitEvent += OnCardUnhovered;
        newCard.PointerClickEvent += OnCardClicked;

        return newCard;
    }

    public void DeleteCard(SettlementCard card, bool removeAttr = true)
    {
        if (!cards.Contains(card)) return;

        cards.Remove(card);
        card.transform.SetParent(card.cardSlot.transform);

        if (removeAttr)
            CardManager.Instance.RemoveCardAttribute(card.cardID);
        Destroy(card.cardSlot);
    }

    // public SettlementCard IsHoveredOverCard()
    // {
        
    // }
}