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

    [Serializable]
    public class CardIllustration
    {
        public CreatureCardType cardDescription;
        public Sprite illustration;
    }

    [Serializable]
    public class ResourcesCardIcons
    {
        public ResourceCardType resourceCardType;
        public Sprite icon;
    }

    public List<CardIconAttribute> cardIconAttributes;
    public List<CardIllustration> cardIllustrations;
    public List<ResourcesCardIcons> resourcesCardIcons;

    private Dictionary<CardType, CardIconAttribute> cardIconsDict;
    private Dictionary<ResourceCardClassification, CardIconAttribute> resourceCardClassificationIconsDict;
    private Dictionary<CreatureCardType, CardIllustration> cardIllustrationsDict;
    private Dictionary<ResourceCardType, ResourcesCardIcons> resourcesCardIconsDict;

    public void InitializeCardIconDict()
    {
        InitCardIconAttributes();
        InitCardIllustrations();
        InitResourcesCardIcons();
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
    public void InitCardIllustrations()
    {
        cardIllustrationsDict = new Dictionary<CreatureCardType, CardIllustration>();
        foreach (var illustration in cardIllustrations)
        {
            cardIllustrationsDict[illustration.cardDescription] = illustration;
        }
    }

    public void InitResourcesCardIcons()
    {
        resourcesCardIconsDict = new Dictionary<ResourceCardType, ResourcesCardIcons>();
        foreach (var resourceIcon in resourcesCardIcons)
        {
            resourcesCardIconsDict[resourceIcon.resourceCardType] = resourceIcon;
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

    public bool TryGetCardIllustration(CreatureCardType cardDescription, out CardIllustration illustration)
    {
        if (cardIllustrationsDict.TryGetValue(cardDescription, out illustration))
        {
            return true;
        }
        illustration = null;
        return false;
    }

    public bool TryGetResourcesCardIcon(ResourceCardType resourceCardType, out ResourcesCardIcons resourceIcon)
    {
        if (resourcesCardIconsDict.TryGetValue(resourceCardType, out resourceIcon))
        {
            return true;
        }
        resourceIcon = null;
        return false;
    }
}