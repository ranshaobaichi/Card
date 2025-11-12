using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
    public Text foodText;
    public Text nameText;
    [Header("卡牌图像列表 - 顺序为：类型、背景、顶部装饰、立绘、底部装饰、侧边装饰")]
    public List<Image> cardImages;
    public void Initialize(Card.CardDescription cardDescription)
    {
        ResourceCardClassification resourceClassification = cardDescription.cardType == CardType.Resources ? DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardDescription).resourceClassification : ResourceCardClassification.None;
        var succ = CardManager.Instance.TryGetCardIconAttribute(cardDescription.cardType, out var cardIconAttrribute, resourceClassification);
        if (succ)
        {
            cardImages[0].sprite = cardIconAttrribute.type;
            cardImages[1].sprite = cardIconAttrribute.background;
            cardImages[2].sprite = cardIconAttrribute.top;
            cardImages[3].sprite = cardIconAttrribute.illustration;
            cardImages[4].sprite = cardIconAttrribute.bottom;
            cardImages[5].sprite = cardIconAttrribute.side;
            nameText.text = cardDescription.ToString();
            if (resourceClassification == ResourceCardClassification.Food)
            {
                foodText.text = DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(cardDescription).satietyValue.ToString();
            }
            else
            {
                foodText.gameObject.SetActive(false);
            }
        }
    }
}
