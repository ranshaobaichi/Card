using System;
using System.Collections.Generic;
using System.Linq;
using Category;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public bool isMovingCardSlot;
    public int maxCardCount;
    public List<Card> cards = new List<Card>();
    public ProgressBar progressBar;
    private CraftTableDB.Recipe currentRecipe;
    private List<Card> currentCraftingCards = new List<Card>();

    public void UpdateIdentityID(Card card, long defaultID = -1)
    {
        long newID = defaultID == -1 ? CardManager.Instance.GetCardIdentityID() : defaultID;
        while (card != null)
        {
            card.SetCardID(newID);
            card = card.laterCard;
        }
    }

    public void UpdateMovingState(Card card, bool state)
    {
        while (card != null)
        {
            card.ChangeMovingState(state);
            card = card.laterCard;
        }
    }

    public void AddCardsToSlot(List<Card> newCards)
    {
        if (newCards == null || newCards.Count == 0) return;

        foreach (var card in newCards)
        {
            card.cardSlot = this;
            card.transform.SetParent(transform);
            if (cards.Count > 0)
                cards.Last().laterCard = card;
            card.preCard = cards.LastOrDefault();
            cards.Add(card);
        }
        // Debug.Log($"Card {card.name} added to slot {name}. Total cards in slot: {cards.Count}");
        if (cards.Count >= maxCardCount)
            cards.Last().canBePlaced = false;
        else
            cards.Last().canBePlaced = true;

        BeginProduction();
    }

    public void ChangeCardsToSlot(Card card, CardSlot newSlot)
    {
        if (card == null) return;

        if (newSlot == null)
        {
            Debug.LogWarning("New slot is null, directly removing card.");
            RemoveCards(card);
            return;
        }

        List<Card> removeCards = new List<Card> { card };
        ChangeCardsToSlot(removeCards, newSlot);
    }

    public void ChangeCardsToSlot(List<Card> changedCards, CardSlot newSlot)
    {
        if (changedCards == null || changedCards.Count == 0) return;

        if (newSlot == null)
        {
            Debug.LogWarning("New slot is null, directly removing cards.");
            RemoveCards(changedCards);
            return;
        }

        // Modify the linking status
        Card preCard = changedCards.First().preCard, laterCard = changedCards.Last().laterCard;
        changedCards.First().preCard = null;
        changedCards.Last().laterCard = null;

        // Remove all the cards in changedCards list
        cards.RemoveAll(card => changedCards.Contains(card));

        // Reconnect the remaining cards
        if (laterCard != null) laterCard.PlaceAfterCard(preCard);

        // Reset the card placement state
        if (cards.Count >= maxCardCount)
            cards.Last().canBePlaced = false;
        else if (cards.Count > 0)
            cards.Last().canBePlaced = true;

        newSlot.AddCardsToSlot(changedCards);

        // If cardSlot is empty, destroy itself
        if (cards.Count == 0 && !isMovingCardSlot)
            Destroy(gameObject);
    }

    public void RemoveCards(Card card)
    {
        if (card == null) return;
        List<Card> removeCards = new List<Card> { card };
        RemoveCards(removeCards);
    }

    public void RemoveCards(List<Card> removeCards)
    {
        if (removeCards == null || removeCards.Count == 0) return;

        // Modify the linking status
        Card preCard = removeCards.First().preCard, laterCard = removeCards.Last().laterCard;
        removeCards.First().preCard = null;
        removeCards.Last().laterCard = null;

        // Remove all the cards in removeCards list
        cards.RemoveAll(card => removeCards.Contains(card));
        foreach (var card in removeCards)
        {
            Destroy(card.gameObject);
        }

        // If cardSlot is empty, destroy itself
        if (cards.Count == 0 && !isMovingCardSlot)
            Destroy(gameObject);

        // Reconnect the remaining cards
        if (laterCard != null) laterCard.PlaceAfterCard(preCard);

        // Reset the card placement state
        if (cards.Count >= maxCardCount)
            cards.Last().canBePlaced = false;
        else if (cards.Count > 0)
            cards.Last().canBePlaced = true;
    }

    [ContextMenu("Begin Production")]
    public bool BeginProduction()
    {
        // // TEST: Get the workload efficiency
        // workload = 10.0f; // Example value, should be set based on actual logic
        // workloadEfficiency = 1.0f; // Example value, should be set based on actual logic

        // First check whether has a valid recipe
        if (!isMovingCardSlot)
        {
            var result = CraftTableDB.Instance.GetRecipe(cards);
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
                Debug.Log("No matching recipe found.");
            }
        }

        return false;
    }

    [ContextMenu("End Production")]
    public void EndProduction()
    {
        currentCraftingCards.Clear();
        progressBar?.StopProgressBar();
    }

    public void OnBeginProduct()
    {
        Card currentWorkingCreatureCard = currentCraftingCards.Find(card => card.cardType.cardType == Category.CardType.Creatures);
        float workloadEfficiency = CardAttributeDB.Instance.GetWorkEfficiencyValue(currentWorkingCreatureCard.cardType.creatureCardType);
        StartProgressBar(currentRecipe.workload / workloadEfficiency, OnEndProduct);
    }

    public void OnProduct()
    {        
    }

    public void OnEndProduct()
    {
        List<Card> removeCards = new List<Card>();
        foreach (var card in currentCraftingCards)
        {
            if (card.cardType.cardType == CardType.Resource &&
                !CardAttributeDB.Instance.IsResourcePoint(card.cardType.resourceCardType))
            {
                removeCards.Add(card);
            }
        }
        if (removeCards.Count > 0) RemoveCards(removeCards);

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
                        Card newCard = CardManager.Instance.CreateCard(craftingCard.cardDescription, dropPosition);
                        break;
                    }
                }
            }
        }

        Debug.Log($"Production completed, working on {currentRecipe.recipeName} recipe");
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

        progressBar.gameObject.SetActive(true);
        progressBar.StartProgressBar(totalTime, onComplete);
    }
}
