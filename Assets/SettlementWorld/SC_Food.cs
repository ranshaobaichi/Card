using System.Reflection;
using Unity.VisualScripting;
using static CardAttributeDB;
using UnityEngine;

class SC_Food : SettlementCard
{
    public int satietyValue { get => resourceAttribute.satietyValue; private set { } }
    private ResourceCardAttribute resourceAttribute;
    public override void InitCard(long cardID)
    {
        this.cardID = cardID;
        resourceAttribute = CardManager.Instance.GetCardAttribute<ResourceCardAttribute>(cardID);
        text.text = $"{satietyValue}";
    }
    public bool TryConsumeFood(int amount)
    {
        if (amount > satietyValue)
        {
            Debug.LogWarning($"Trying to consume more food ({amount}) than available ({satietyValue}).");
            return false;
        }
        // Reduce the satiety value
        resourceAttribute.satietyValue -= amount;
        text.text = $"{resourceAttribute.satietyValue}";
        if (resourceAttribute.satietyValue <= 0)
        {
            // Handle food depletion (e.g., remove card from panel)
            Debug.Log($"Food card {cardSlot.name} is depleted and will be removed.");
            SettlementCardManager.Instance.FoodPanel.DeleteCard(this);
        }
        return true;
    }
}