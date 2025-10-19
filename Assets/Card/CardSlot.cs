using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Category;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public int maxCardCount;
    public List<Card> cards = new List<Card>();
    public ProgressBar progressBar;
    public static CardSlot movingCardSlot;
    private CraftTableDB.Recipe currentRecipe;
    private List<Card> currentCraftingCards = new List<Card>();

# region 静态接口
    public static void UpdateIdentityID(Card card, long defaultID = -1) => UpdateIdentityID(new List<Card> { card }, defaultID);
    public static void UpdateIdentityID(List<Card> cards, long defaultID = -1)
    {
        long newID = defaultID == -1 ? CardManager.Instance.GetCardIdentityID() : defaultID;
        foreach (var card in cards)
        {
            card.SetCardID(newID);
        }
    }

    /// <summary>
    /// Change a single card to a new slot
    /// </summary>
    /// <param name="oldSlot">Old slot</param>
    /// <param name="newSlot">New slot</param>
    /// <param name="changedCard">Changed card</param>
    /// <param name="afterCard">After card</param>
    /// <param name="controlNewCardPos">Whether to control the new card position (set to false when moving cards)</param>
    public static void ChangeCardToSlot(CardSlot oldSlot, CardSlot newSlot, Card changedCard, Card afterCard = null, bool controlNewCardPos = false)
    {
        // Validate input check
        if (changedCard == null) return;
        if (newSlot == null)
        {
            if (oldSlot == null)
            {
                Debug.LogWarning("Both old slot and new slot are null, cannot remove card.");
                return;
            }
            Debug.LogWarning("New slot is null, directly removing and destroying cards.");
            RemoveCard(oldSlot, changedCard, true);
            return;
        }

        long newID = -1;
        List<Card> oldSlotCards = oldSlot == null ? null : oldSlot.cards, newSlotCards = newSlot.cards;
        if (newSlot != movingCardSlot)
            newID = newSlot.cards.Count > 0 ? newSlot.cards[0].GetCardID() : CardManager.Instance.GetCardIdentityID();
        if (afterCard == null) afterCard = newSlotCards.LastOrDefault();
        if (newSlotCards.Count > 0 && newSlotCards.Contains(afterCard) == false)
        {
            Debug.LogWarning($"After card {afterCard?.name} not found in this slot.");
            return;
        }

        // Update the linking status
        // Link old slot cards
        Card oldSlotPreCard = changedCard.preCard, oldSlotLaterCard = changedCard.laterCard;
        if (oldSlotPreCard != null) oldSlotPreCard.laterCard = oldSlotLaterCard;
        if (oldSlotLaterCard != null) oldSlotLaterCard.preCard = oldSlotPreCard;
        // Link new slot cards
        Card newSlotOriLaterCard = afterCard == null ? null : afterCard.laterCard;
        changedCard.preCard = afterCard;
        if (afterCard != null)
        {
            afterCard.laterCard = changedCard;
            afterCard.canBePlaced = false;
        }
        changedCard.laterCard = newSlotOriLaterCard;
        if (newSlotOriLaterCard != null) newSlotOriLaterCard.preCard = changedCard;
        newSlotCards.Insert(afterCard == null ? 0 : newSlotCards.IndexOf(afterCard) + 1, changedCard);
        oldSlotCards?.Remove(changedCard);

        // Update the cardSlot reference and transform parent
        changedCard.cardSlot = newSlot;
        changedCard.transform.SetParent(newSlot.transform);

        // Update the identity ID
        if (newSlot != movingCardSlot)
            UpdateIdentityID(changedCard, newID);

        // Update the transform position
            // Old slot cards
            if (oldSlotCards?.Count > 0)
            {
                Vector3 preCardPos = oldSlotPreCard != null ? oldSlotPreCard.transform.position : oldSlot.transform.position;
                Card cardToPos = oldSlotLaterCard;
                while (cardToPos != null)
                {
                    Vector3 targetPos = preCardPos - new Vector3(0, cardToPos.yAlignedDistance, 0);
                    cardToPos.transform.position = targetPos;
                    preCardPos = cardToPos.transform.position;
                    cardToPos = cardToPos.laterCard;
                }
            }
            // New slot cards
            if (controlNewCardPos)
            {
                Vector3 newSlotPreCardPos = afterCard != null ? afterCard.transform.position : newSlot.transform.position;
                Card newSlotCardToPos = changedCard;
                while (newSlotCardToPos != null)
                {
                    Vector3 targetPos = newSlotPreCardPos - new Vector3(0, newSlotCardToPos.yAlignedDistance, 0);
                    newSlotCardToPos.transform.position = targetPos;
                    newSlotPreCardPos = newSlotCardToPos.transform.position;
                    newSlotCardToPos = newSlotCardToPos.laterCard;
                }
            }

        // Delete old slot if empty
        if (oldSlotCards?.Count == 0 && oldSlot != movingCardSlot)
        {
            Destroy(oldSlot.gameObject);
        }

        // Change the last card's placement state
        oldSlot?.UpdateLastCardPlacementState();
        newSlot.UpdateLastCardPlacementState();

        // Detect whether need to start production
        newSlot.BeginProduction();
    }

    /// <summary>
    /// Change multiple cards to a new slot, which must be continuous
    /// </summary>
    /// <param name="oldSlot">Old slot</param>
    /// <param name="newSlot">New slot</param>
    /// <param name="changedCards">Changed cards</param>
    /// <param name="afterCard">After card</param>
    /// <param name="controlNewCardPos">Whether to control the new card position (set to false when moving cards)</param>
    public static void ChangeCardsToSlot(CardSlot oldSlot, CardSlot newSlot, List<Card> changedCards, Card afterCard = null, bool controlNewCardPos = false)
    {
        // Validate input check
        if (changedCards == null || changedCards.Count == 0) return;
        if (newSlot == null)
        {
            if (oldSlot == null)
            {
                Debug.LogWarning("Both old slot and new slot are null, cannot remove cards.");
                return;
            }
            Debug.LogWarning("New slot is null, directly removing and destroying cards.");
            RemoveCards(oldSlot, changedCards, true);
            return;
        }

        long newID = -1;
        List<Card> oldSlotCards = oldSlot == null ? null : oldSlot.cards, newSlotCards = newSlot.cards;
        if (newSlot != movingCardSlot)
            newID = newSlot.cards.Count > 0 ? newSlot.cards[0].GetCardID() : CardManager.Instance.GetCardIdentityID();
        if (afterCard == null) afterCard = newSlotCards.LastOrDefault();
        if (newSlotCards.Count > 0 && newSlotCards.Contains(afterCard) == false)
        {
            Debug.LogWarning($"After card {afterCard?.name} not found in this slot.");
            return;
        }

        // Update the linking status
        // Link old slot cards
        Card oldSlotPreCard = changedCards.First().preCard, oldSlotLaterCard = changedCards.Last().laterCard;
        if (oldSlotPreCard != null) oldSlotPreCard.laterCard = oldSlotLaterCard;
        if (oldSlotLaterCard != null) oldSlotLaterCard.preCard = oldSlotPreCard;
        // Link new slot cards
        Card newSlotOriLaterCard = afterCard == null ? null : afterCard.laterCard;
        changedCards.First().preCard = afterCard;
        if (afterCard != null)
        {
            afterCard.laterCard = changedCards.First();
            afterCard.canBePlaced = false;
        }
        changedCards.Last().laterCard = newSlotOriLaterCard;
        if (newSlotOriLaterCard != null) newSlotOriLaterCard.preCard = changedCards.Last();
        newSlotCards.InsertRange(afterCard == null ? 0 : newSlotCards.IndexOf(afterCard) + 1, changedCards);
        oldSlotCards?.RemoveAll(card => changedCards.Contains(card));
        
        // Update the cardSlot reference and transform parent
        foreach (var card in changedCards)
        {
            card.cardSlot = newSlot;
            card.transform.SetParent(newSlot.transform);
        }

        // Update the identity ID
        if (newSlot != movingCardSlot)
            UpdateIdentityID(changedCards, newID);

        // Update the transform position
            // Old slot cards
            if (oldSlotCards?.Count > 0)
            {
                Vector3 preCardPos = oldSlotPreCard != null ? oldSlotPreCard.transform.position : oldSlot.transform.position;
                Card cardToPos = oldSlotLaterCard;
                while (cardToPos != null)
                {
                    Vector3 targetPos = preCardPos - new Vector3(0, cardToPos.yAlignedDistance, 0);
                    cardToPos.transform.position = targetPos;
                    preCardPos = cardToPos.transform.position;
                    cardToPos = cardToPos.laterCard;
                }
            }
            // New slot cards
            if (controlNewCardPos)
            {
                Vector3 newSlotPreCardPos = afterCard != null ? afterCard.transform.position : newSlot.transform.position;
                Card newSlotCardToPos = changedCards.First();
                while (newSlotCardToPos != null)
                {
                    Vector3 targetPos = newSlotPreCardPos - new Vector3(0, newSlotCardToPos.yAlignedDistance, 0);
                    newSlotCardToPos.transform.position = targetPos;
                    newSlotPreCardPos = newSlotCardToPos.transform.position;
                    newSlotCardToPos = newSlotCardToPos.laterCard;
                }
            }

        // Delete old slot if empty
        if (oldSlotCards?.Count == 0 && oldSlot != movingCardSlot)
        {
            Destroy(oldSlot.gameObject);
        }

        // Change the last card's placement state
        oldSlot?.UpdateLastCardPlacementState();
        newSlot.UpdateLastCardPlacementState();

        // Detect whether need to start production
        newSlot.BeginProduction();
    }

    /// <summary>
    /// Remove a single card from this slot
    /// </summary>
    /// <param name="card"></param>
    /// <param name="destroyCard">Whether to destroy the card object</param>
    public static void RemoveCard(CardSlot cardSlot, Card card, bool destroyCard = true)
    {
        if (card == null || cardSlot == null) return;
        List<Card> cards = cardSlot.cards;
        if (cards.Contains(card) == false)
        {
            Debug.LogWarning($"Card {card.name} not found in this slot.");
            return;
        }

        // Update the linking status
        Card preCard = card.preCard, laterCard = card.laterCard;
        if (preCard != null) preCard.laterCard = laterCard;
        if (laterCard != null) laterCard.preCard = preCard;
        card.preCard = null;
        card.laterCard = null;

        // Update the cardSlot reference and transform parent
        cards.Remove(card);
        if (destroyCard)
            card.DeleteCard();
        else
        {
            card.cardSlot = null;
            card.transform.SetParent(null);
        }

        // If cardSlot is empty, destroy itself
        if (cards.Count == 0 && cardSlot != movingCardSlot)
        {
            Destroy(cardSlot.gameObject);
            return;
        }

        // Update the transform position
        Vector3 preCardPos = preCard != null ? preCard.transform.position : cardSlot.transform.position;
        Card cardToPos = laterCard;
        while (cardToPos != null)
        {
            Vector3 targetPos = preCardPos - new Vector3(0, cardToPos.yAlignedDistance, 0);
            cardToPos.transform.position = targetPos;
            preCardPos = cardToPos.transform.position;
            cardToPos = cardToPos.laterCard;
        }

        // Update the last card's placement state
        cardSlot.UpdateLastCardPlacementState();

        // Detect whether need to start production
        cardSlot.BeginProduction();
    }

    /// <summary>
    /// Remove multiple cards from this slot, which must be continuous
    /// </summary>
    /// <param name="removeCards"></param>
    public static void RemoveCards(CardSlot cardSlot, List<Card> removeCards, bool destroyCards = true)
    {
        // Validate input check
        if (removeCards == null || removeCards.Count == 0 || cardSlot == null) return;
        List<Card> cards = cardSlot.cards;
        foreach (var card in removeCards)
        {
            if (cards.Contains(card) == false)
            {
                Debug.LogWarning($"Card {card.name} not found in this slot.");
                return;
            }
        }

        // Update the linking status
        Card preCard = removeCards.First().preCard, laterCard = removeCards.Last().laterCard;
        if (preCard != null) preCard.laterCard = laterCard;
        if (laterCard != null) laterCard.preCard = preCard;
        removeCards.First().preCard = null;
        removeCards.Last().laterCard = null;

        // Update the cardSlot reference and transform parent
        foreach (var card in removeCards)
        {
            cards.Remove(card);
            if (destroyCards)
                card.DeleteCard();
            else
            {
                card.cardSlot = null;
                card.transform.SetParent(null);
            }
        }

        // If cardSlot is empty, destroy itself
        if (cards.Count == 0 && cardSlot != movingCardSlot)
        {
            Destroy(cardSlot.gameObject);
            return;
        }

        // Update the transform position
        Vector3 preCardPos = preCard != null ? preCard.transform.position : cardSlot.transform.position;
        Card cardToPos = laterCard;
        while (cardToPos != null)
        {
            Vector3 targetPos = preCardPos - new Vector3(0, cardToPos.yAlignedDistance, 0);
            cardToPos.transform.position = targetPos;
            preCardPos = cardToPos.transform.position;
            cardToPos = cardToPos.laterCard;
        }

        // Change the last card's placement state
        cardSlot.UpdateLastCardPlacementState();

        // Detect whether need to start production
        cardSlot.BeginProduction();
    }
    #endregion

# region 接口方法
    public void UpdateMovingState(Card card, bool state)
    {
        while (card != null)
        {
            card.ChangeMovingState(state);
            card = card.laterCard;
        }
    }

    public void UpdateLastCardPlacementState()
    {
        if (cards.Count == 0) return;
        if (cards.Count >= maxCardCount)
            cards.Last().canBePlaced = false;
        else
            cards.Last().canBePlaced = true;
    }
#endregion

    void Start()
    {
        movingCardSlot = GameObject.FindGameObjectWithTag("MovingCardSlot").GetComponent<CardSlot>();
    }

    public bool BeginProduction()
    {
        // // TEST: Get the workload efficiency
        // workload = 10.0f; // Example value, should be set based on actual logic
        // workloadEfficiency = 1.0f; // Example value, should be set based on actual logic
        if (this == movingCardSlot) return false;

        // First check whether has a valid recipe
        var result = CardManager.Instance.GetRecipe(cards);
        if (result.HasValue)
        {
            Debug.Log($"Found matching recipe: {result.Value.Item2.recipeName}");
            currentCraftingCards = result.Value.Item1;
            currentRecipe = result.Value.Item2;
            OnBeginProduct();
            return true;
        }
        else
        {
            // Debug.Log("No matching recipe found.");
        }
        return false;
    }

    public void EndProduction()
    {
        currentCraftingCards.Clear();
        progressBar?.StopProgressBar();
    }

    public void OnBeginProduct()
    {
        List<Card> currentWorkingCreatureCards = currentCraftingCards.FindAll(card => card.cardDescription.cardType == CardType.Creatures);
        // BUG： If no creature card is found, the production cannot start
        float workloadEfficiency = currentWorkingCreatureCards.Count > 0 ?
                                    CardManager.Instance.GetWorkEfficiencyValue(currentWorkingCreatureCards
                                        .Max(card => CardManager.Instance.GetWorkEfficiencyType(card.cardDescription.creatureCardType))) :
                                    CardManager.Instance.GetWorkEfficiencyValue(WorkEfficiencyType.Normal);

        StartProgressBar(currentRecipe.workload / workloadEfficiency, OnEndProduct);
        Debug.Log($"Starting production for recipe: {currentRecipe.recipeName} with workload efficiency: {workloadEfficiency}, workload: {currentRecipe.workload}");
    }

    public void OnProduct()
    {
        
    }

    public void OnEndProduct()
    {
        List<Card> removeCards = new List<Card>();
        foreach (var card in currentCraftingCards)
        {
            card.durability -= 1;
            if (card.durability <= 0)
            {
                removeCards.Add(card);
            }
        }
        if (removeCards.Count > 0) RemoveCards(this, removeCards, true);

        // 从配方中掉落一张卡牌，根据权重决定
        if (currentRecipe.outputCards != null && currentRecipe.outputCards.Count > 0)
        {
            // 计算权重总和
            int totalWeight = 0;
            foreach (var craftingCard in currentRecipe.outputCards)
            {
                totalWeight += craftingCard.dropWeight;
            }

            // 如果有有效权重，进行权重随机选择
            if (totalWeight > 0)
            {
                // 生成一个随机数，范围在0到权重总和之间
                int randomWeight = UnityEngine.Random.Range(0, totalWeight);
                int currentWeight = 0;
                
                // 根据权重选择卡牌
                foreach (var craftingCard in currentRecipe.outputCards)
                {
                    currentWeight += craftingCard.dropWeight;
                    if (randomWeight < currentWeight)
                    {
                        // Drop pos
                        Vector2 dropPosition = transform.position + new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 0);

                        // 创建选中的卡牌
                        CardManager.Instance.CreateCard(craftingCard.cardDescription, dropPosition);
                        break;
                    }
                }
            }
        }

        Debug.Log($"Production completed, working on {currentRecipe.recipeName} recipe");
        Debug.Log($"Produced {currentRecipe.outputCards.Count} cards, which are:");
        foreach (var craftingCard in currentRecipe.outputCards)
        {
            Debug.Log($"- {craftingCard.GetType()}");
        }
        if (!BeginProduction())
            EndProduction();
    }

    public void StartProgressBar(float totalTime, Action onComplete)
    {
        if (progressBar == null)
        {
            Debug.LogError("ProgressBar is not assigned.");
            return;
        }
        if (totalTime <= 0.01f)
        {
            Debug.Log($"Total time: {totalTime} too short, completing immediately.");
            onComplete?.Invoke();
            return;
        }
        progressBar.gameObject.SetActive(true);
        progressBar.StartProgressBar(totalTime, onComplete);
    }

    public bool TryGetEventCard(out Card eventCard)
    {
        eventCard = cards.Find(card => card.cardDescription.cardType == CardType.Events);
        return eventCard != null;
    }
}
