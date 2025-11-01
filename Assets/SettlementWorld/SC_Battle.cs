class SC_Battle : SettlementCard
{
    public override void InitCard(long cardID)
    {
        this.cardID = cardID;
        nameText.text = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID).creatureCardType.ToString();
        satietyText.transform.parent.gameObject.SetActive(false);
        foodValueText.gameObject.SetActive(false);
        SetCardImage();
    }
}