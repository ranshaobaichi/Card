using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventCardUIDB", menuName = "ScriptableObjects/EventCardUIDB")]
public class EventCardUIDB : ScriptableObject
{
    [Serializable]
    public struct EventCardUIInfo
    {
        public Category.EventCardType eventCardType;
        public GameObject eventCardUIPrefab;
    }

    public EventCardUIInfo[] eventCardUIInfos;
    private Dictionary<Category.EventCardType, GameObject> eventCardUIDict = new Dictionary<Category.EventCardType, GameObject>();

    private void ConvertToDict()
    {
        eventCardUIDict.Clear();
        foreach (var info in eventCardUIInfos)
        {
            if (!eventCardUIDict.ContainsKey(info.eventCardType))
            {
                eventCardUIDict[info.eventCardType] = info.eventCardUIPrefab;
            }
            else
            {
                Debug.LogWarning($"Duplicate EventCardType {info.eventCardType} in EventCardUIInfos.");
            }
        }
    }

    public bool TryGetEventCardUIPrefab(Category.EventCardType eventCardType, out GameObject prefab)
    {
        if (eventCardUIDict.Count == 0)
        {
            ConvertToDict();
        }
        return eventCardUIDict.TryGetValue(eventCardType, out prefab);
    }
}