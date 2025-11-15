using UnityEngine;
using UnityEngine.UI;
using static CardAttributeDB;

class SC_Creature : SettlementCard
{
    public int satiety;
    public new void Awake()
    {
        base.Awake();
    }
    public override void InitCard(long cardID)
    {
        this.cardID = cardID;
        var attr = CardManager.Instance.GetCardAttribute<CreatureCardAttribute>(cardID);
        type = attr.creatureCardType;
        // Debug.Log($"Initializing creature card {cardID} with satiety {attr.basicAttributes.satiety}");
        satietyText.text = $"{attr.basicAttributes.satiety}";
        satiety = attr.basicAttributes.satiety;
        SetCardImage();
    }
    public void InitCard(long cardID, int initialSatiety = -1)
    {
        if (initialSatiety < 0)
        {
            InitCard(cardID);
            return;
        }
        this.cardID = cardID;
        satietyText.text = $"{initialSatiety}";
        satiety = initialSatiety;
    }

    public void EatingFood(int amount)
    {
        satiety -= amount;
        satietyText.text = $"{satiety}";
        // Debug.Log($"Creature card {cardSlot.name} consumed {amount} satiety. Remaining satiety: {satiety}");
    }

}