using System;
using System.Collections.Generic;
using System.Linq;
using Category;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftTable", menuName = "ScriptableObjects/CraftTable", order = 1)]
public class CraftTableDB : ScriptableObject
{
    #region 单例
    private static readonly string _resourcePath = "ScriptableObjects/E_CraftTable";
    private static CraftTableDB _instance;
    public static CraftTableDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CraftTableDB>(_resourcePath);
                if (_instance == null)
                {
                    Debug.LogWarning($"未能从{_resourcePath}找到CraftTable资源，创建了一个新实例。请确保在Resources文件夹中有配置好的CraftTable资源。");
                    _instance = CreateInstance<CraftTableDB>();
                }
            }
            return _instance;
        }
    }
    #endregion

    [Serializable]
    public struct CraftingCard
    {
        public Card.CardDescription cardDescription;
        public int dropWeight;
    }

    [Serializable]
    public struct Recipe
    {
        [Header("配方设置")]
        [Tooltip("配方ID")] public int recipeID;
        [Tooltip("配方名称")] public string recipeName;
        [Tooltip("配方描述")] public string recipeDescription;
        [Tooltip("配方工作量")] public float workload;
        [Tooltip("配方工作类型")] public WorkType workType;

        [Header("合成所需卡牌")]
        public List<Card.CardDescription> inputCards;

        [Header("产出卡牌")]
        public List<CraftingCard> outputCards;

        private Recipe(int id = -1, string name = null)
        {
            recipeID = id;
            recipeName = name;
            inputCards = null;
            outputCards = null;
            recipeDescription = null;
            workload = 0f;
            workType = WorkType.None;
        }
    }

    [Header("配方列表")]
    public List<Recipe> recipeList = new List<Recipe>();

    public Recipe? GetRecipe(int id)
    {
        var recipe = recipeList.Find(recipe => recipe.recipeID == id);
        return recipe.recipeID == id ? recipe : null;
    }
    public Recipe? GetRecipe(string name)
    {
        var recipe = recipeList.Find(recipe => recipe.recipeName == name);
        return recipe.recipeName == name ? recipe : null;
    }

    public (List<Card>, Recipe)? GetRecipe(List<Card> inputCards)
    {
        Debug.Log($"Has {recipeList.Count} recipes in total.");
        foreach (var recipe in recipeList)
        {
            var cardDescs = inputCards.Select(card => card.cardType).ToList();
            var usedCardIndices = CanCraftRecipeWithIndices(recipe, cardDescs);

            if (usedCardIndices != null)
            {
                // 根据索引创建使用的卡牌列表
                List<Card> usedCards = new List<Card>();
                foreach (var index in usedCardIndices)
                {
                    usedCards.Add(inputCards[index]);
                }

                return (usedCards, recipe);
            }
        }
        return null;
    }

    // 检查给定的输入卡牌是否满足配方需求，并返回使用的卡牌索引
    private List<int> CanCraftRecipeWithIndices(Recipe recipe, List<Card.CardDescription> availableCards)
    {
        // 统计配方所需的每种卡牌类型的数量
        var requiredTypes = new Dictionary<(CardType, int), int>();
        foreach (var card in recipe.inputCards)
        {
            var key = GetCardTypeKey(card);
            if (requiredTypes.ContainsKey(key))
                requiredTypes[key]++;
            else
                requiredTypes[key] = 1;
        }

        // Debug.Log($"配方 {recipe.recipeName} 需要以下卡牌：");
        // foreach (var type in requiredTypes)
        // {
        //     switch (type.Key.Item1)
        //     {
        //         case CardType.Resource:
        //             Debug.Log($"需要资源卡 {((ResourceCardType)type.Key.Item2).ToString()} x {type.Value}");
        //             break;
        //         case CardType.Creatures:
        //             Debug.Log($"需要生物卡 {((CreatureCardType)type.Key.Item2).ToString()} x {type.Value}");
        //             break;
        //         case CardType.Events:
        //             Debug.Log($"需要事件卡 {((EventCardType)type.Key.Item2).ToString()} x {type.Value}");
        //             break;
        //     }
        // }

        // 创建可用卡牌的跟踪列表
        var usedIndices = new List<int>();
        var remainingRequirements = new Dictionary<(CardType, int), int>(requiredTypes);

        // 处理任意生物卡的情况
        int anyCreatureCount = 0;
        if (remainingRequirements.TryGetValue((CardType.Creatures, (int)CreatureCardType.Any), out int anyCreatureRequired))
        {
            anyCreatureCount = anyCreatureRequired;
            remainingRequirements.Remove((CardType.Creatures, (int)CreatureCardType.Any));
        }

        // 首先满足特定类型的需求
        for (int i = 0; i < availableCards.Count; i++)
        {
            var key = GetCardTypeKey(availableCards[i]);
            if (remainingRequirements.TryGetValue(key, out int count) && count > 0)
            {
                usedIndices.Add(i);
                remainingRequirements[key]--;
                // Debug.Log($@"Use {availableCards[i].cardType}, {availableCards[i].cardType switch
                // {
                //     CardType.Resource => ((ResourceCardType)availableCards[i].resourceCardType).ToString(),
                //     CardType.Creatures => ((CreatureCardType)availableCards[i].creatureCardType).ToString(),
                //     CardType.Events => ((EventCardType)availableCards[i].eventCardType).ToString(),
                //     _ => "未知类型"
                // }}, remaining {remainingRequirements[key]}");
                if (remainingRequirements[key] == 0)
                {
                    remainingRequirements.Remove(key);
                }
            }
        }

        // 如果特定类型的需求未满足，返回null
        if (remainingRequirements.Count > 0)
            return null;

        // 处理任意生物卡需求
        if (anyCreatureCount > 0)
        {
            Debug.Log($"需要任意生物卡 x {anyCreatureCount}");
            int found = 0;
            for (int i = 0; i < availableCards.Count; i++)
            {
                if (!usedIndices.Contains(i) && availableCards[i].cardType == CardType.Creatures)
                {
                    usedIndices.Add(i);
                    found++;
                    if (found == anyCreatureCount)
                        break;
                }
            }

            // 如果没有足够的任意生物卡，返回null
            if (found < anyCreatureCount)
                return null;
        }

        return usedIndices;
    }

    // 获取卡牌类型的唯一键值
    private (CardType, int) GetCardTypeKey(Card.CardDescription card)
    {
        return card.cardType switch
        {
            CardType.Resource => (CardType.Resource, (int)card.resourceCardType),
            CardType.Creatures => (CardType.Creatures, (int)card.creatureCardType),
            CardType.Events => (CardType.Events, (int)card.eventCardType),
            _ => (CardType.None, 0),
        };
    }
}