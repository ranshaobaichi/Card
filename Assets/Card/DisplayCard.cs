using System;
using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
    [Serializable]
    public class CardImages
    {
        public Image type;
        public Image background;
        public Image top;
        public Image illustration;
        public Image bottom;
        public Image side;
    }
    public Text foodText;
    public Text nameText;
    [Header("卡牌图像列表 - 顺序为：类型、背景、顶部装饰、立绘、底部装饰、侧边装饰")]
    public CardImages cardImagesContainer;
    private Card.CardDescription cardDescription;
    private ResourceCardClassification resourceClassification;
    public bool Initialize(Card.CardDescription cardDescription)
    {
        this.cardDescription = cardDescription;
        resourceClassification = cardDescription.cardType == CardType.Resources ? 
            DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardDescription).resourceClassification : 
            ResourceCardClassification.None;
        var succ = DataBaseManager.Instance.TryGetCardIconAttribute(cardDescription.cardType, out var cardIconAttrribute, resourceClassification);
        if (succ)
        {
            cardImagesContainer.type.sprite = cardIconAttrribute.type;
            cardImagesContainer.background.sprite = cardIconAttrribute.background;
            cardImagesContainer.top.sprite = cardIconAttrribute.top;

            bool succSetIllustration = false;
            switch (cardDescription.cardType)
            {
                case CardType.Creatures:
                    succSetIllustration = DataBaseManager.Instance.TryGetCardIllustration(cardDescription.creatureCardType, out var cardIllustration);
                    if (succSetIllustration)
                    {
                        cardImagesContainer.illustration.sprite = cardIllustration.illustration;
                    }
                    break;
                case CardType.Resources:
                    succSetIllustration = DataBaseManager.Instance.TryGetResourcesCardIcon(cardDescription.resourceCardType, out var resourceIcon);
                    if (succSetIllustration)
                    {
                        cardImagesContainer.illustration.sprite = resourceIcon.icon;
                    }
                    break;
            }
            if (!succSetIllustration) cardImagesContainer.illustration.gameObject.SetActive(false);

            cardImagesContainer.bottom.sprite = cardIconAttrribute.bottom;
            cardImagesContainer.side.sprite = cardIconAttrribute.side;
            nameText.text = cardDescription.ToString();
            if (resourceClassification == ResourceCardClassification.Food)
            {
                foodText.text = DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardDescription).satietyValue.ToString();
            }
            else
            {
                // Debug.LogWarning($"Card {cardDescription} is not a Food resource card, hiding food text.");
                foodText.gameObject.SetActive(false);
            }
            return true;
        }
        else
        {
            Debug.LogError($"Card icon attribute not found for card type {cardDescription.cardType}.");
            return false;
        }
    }

    public void SetOnlyDisplayIllustration(bool value)
    {
        cardImagesContainer.type.gameObject.SetActive(!value);
        // cardImagesContainer.type.raycastTarget = !value;

        cardImagesContainer.background.gameObject.SetActive(!value);
        // cardImagesContainer.background.raycastTarget = !value;

        cardImagesContainer.top.gameObject.SetActive(!value);
        // cardImagesContainer.top.raycastTarget = !value;

        cardImagesContainer.bottom.gameObject.SetActive(!value);
        // cardImagesContainer.bottom.raycastTarget = !value;

        cardImagesContainer.side.gameObject.SetActive(!value);
        // cardImagesContainer.side.raycastTarget = !value;

        nameText.gameObject.SetActive(!value);
        if (resourceClassification == ResourceCardClassification.Food)
            foodText.gameObject.SetActive(!value);
    }
}
