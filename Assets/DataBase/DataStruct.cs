using System.Collections.Generic;
using UnityEngine;

namespace DataStruct
{
    public interface ISceneData
    {
        public void Initialize();
        public void PrintData();
    }
    public struct ProductionToSettlementData : ISceneData
    {
        public Dictionary<Category.CardType, List<int>> cardDescriptions;

        public void Initialize()
        {
            cardDescriptions = new Dictionary<Category.CardType, List<int>>();
            cardDescriptions[Category.CardType.Creatures] = new List<int>();
            cardDescriptions[Category.CardType.Resources] = new List<int>();
        }

        public readonly void PrintData()
        {
            foreach (var key in cardDescriptions.Keys)
            {
                Debug.Log($"CardType: {key}");
                foreach (var value in cardDescriptions[key])
                {
                    var type = key switch
                    {
                        Category.CardType.Creatures => ((Category.CreatureCardType)value).ToString(),
                        Category.CardType.Resources => ((Category.ResourceCardType)value).ToString(),
                        Category.CardType.Events => ((Category.EventCardType)value).ToString(),
                        Category.CardType.None => throw new System.NotImplementedException(),
                        _ => "UnknownType"
                    };
                    Debug.Log($"Type value: {type}");
                }
            }
        }
    }
}