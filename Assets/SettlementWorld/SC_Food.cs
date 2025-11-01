using static CardAttributeDB;
using UnityEngine;
using UnityEngine.UI;

class SC_Food : SettlementCard
{
    public int satietyValue { get => resourceAttribute.satietyValue; private set { } }
    private ResourceCardAttribute resourceAttribute;
    public override void InitCard(long cardID)
    {
        this.cardID = cardID;
        satietyText.transform.parent.gameObject.SetActive(false);
        resourceAttribute = CardManager.Instance.GetCardAttribute<ResourceCardAttribute>(cardID);
        foodValueText.text = $"{satietyValue}";
        nameText.text = resourceAttribute.resourceCardType.ToString();
        Debug.Log("food type is " + resourceAttribute.resourceCardType.ToString());
        SetCardImage();
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
        foodValueText.text = $"{resourceAttribute.satietyValue}";
        if (resourceAttribute.satietyValue <= 0)
        {
            // Handle food depletion (e.g., remove card from panel)
            Debug.Log($"Food card {cardSlot.name} is depleted and will be removed.");
            SettlementCardManager.Instance.FoodPanel.DeleteCard(this, true);
        }
        return true;
    }
}