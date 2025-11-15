class SC_Battle : SettlementCard
{
    public override void InitCard(long cardID)
    {
        this.cardID = cardID;
        var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
        type = attr.creatureCardType;
        satietyText.transform.parent.gameObject.SetActive(false);
        SetCardImage();
    }
}