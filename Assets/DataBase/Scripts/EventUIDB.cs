using System;
using System.Collections.Generic;
using UnityEngine;
using Category;
using System.Collections.ObjectModel;

[CreateAssetMenu(fileName = "EventUIDB", menuName = "ScriptableObjects/EventUIDB")]
public class EventUIDB : ScriptableObject
{
    [Serializable]
    public struct EventUIAttribute
    {
        [Serializable]
        public struct OptionAttribute
        {
            [Serializable]
            public struct CostValue
            {
                public List<Card.CardDescription> cardsCost;
                public float timeCost;
            }
            [TextArea]
            public string optionText;
            public CostValue cost;
            public List<DropCard> rewards;
        }
        public EventCardType eventCardType;
        [TextArea]
        public string descriptionText;
        public List<OptionAttribute> options;
    }

    public List<EventUIAttribute> eventCardUIInfos;
    private Dictionary<EventCardType, EventUIAttribute> eventCardUIDict = new Dictionary<EventCardType, EventUIAttribute>();

    public void InitializeEventUIDict()
    {
        eventCardUIDict.Clear();
        foreach (var info in eventCardUIInfos)
        {
            eventCardUIDict[info.eventCardType] = info;
        }
    }

    public ReadOnlyDictionary<Category.EventCardType, EventUIAttribute> GetEventCardUIDict()
    {
        return new ReadOnlyDictionary<Category.EventCardType, EventUIAttribute>(eventCardUIDict);
    }

    public EventUIAttribute? GetEventUIAttribute(Category.EventCardType eventCardType)
    {
        if (eventCardUIDict.TryGetValue(eventCardType, out var attribute))
        {
            return attribute;
        }
        return null;
    }
}