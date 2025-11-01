using System.Collections.Generic;
using UnityEngine;
using Category;
using System;

[CreateAssetMenu(fileName = "CardIconsDB", menuName = "ScriptableObjects/CardIconsDB", order = 1)]
public class CardIconsDB : ScriptableObject
{
    [Serializable]
    public class CardIconAttribute
    {
        public CardType cardType;
        public ResourceCardClassification resourceCardClassification;
        public Sprite type;
        public Sprite background;
        public Sprite top;
        [HideInInspector] public Sprite illustration = null;
        public Sprite bottom;
        public Sprite side;
    }

    public List<CardIconAttribute> cardIconAttributes;
    private Dictionary<CardType, CardIconAttribute> cardIconsDict;
    private Dictionary<ResourceCardClassification, CardIconAttribute> resourceCardClassificationIconsDict;
    public void Initialize()
    {
        InitCardIconAttributes();
    }
    public void InitCardIconAttributes()
    {
        cardIconsDict = new Dictionary<CardType, CardIconAttribute>();
        resourceCardClassificationIconsDict = new Dictionary<ResourceCardClassification, CardIconAttribute>();
        foreach (var attribute in cardIconAttributes)
        {
            if (attribute.cardType != CardType.Resources)
            {
                cardIconsDict[attribute.cardType] = attribute;
            }
            else
            {
                resourceCardClassificationIconsDict[attribute.resourceCardClassification] = attribute;
            }
        }
    }

    public bool TryGetCardIconAttribute(CardType cardType, out CardIconAttribute attribute, ResourceCardClassification resourceCardClassification)
    {
        // Debug.Log($"TryGetCardIconAttribute: cardType={cardType}, resourceCardClassification={resourceCardClassification}");
        if (cardType == CardType.Resources)
        {
            if (resourceCardClassificationIconsDict.TryGetValue(resourceCardClassification, out attribute))
            {
                return true;
            }
        }
        else
        {
            if (cardIconsDict.TryGetValue(cardType, out attribute))
            {
                return true;
            }
        }
        attribute = null;
        return false;
    }
}