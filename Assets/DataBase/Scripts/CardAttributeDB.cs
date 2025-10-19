using UnityEngine;
using Category;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardAttributeDB", menuName = "ScriptableObjects/CardAttributeDB")]
public class CardAttributeDB : ScriptableObject
{
    #region 生物卡
    public struct CreatureCardAttribute
    {
        // [Header("生物卡设置")]
        // [Header("生产设置")]
        [Header("工作类型与效率")]
        [Tooltip("合成类型")] public WorkEfficiencyType craftWorkEfficiency;
        [Tooltip("探索类型")] public WorkEfficiencyType exploreWorkEfficiency;
        [Tooltip("建造类型")] public WorkEfficiencyType interactWorkEfficiency;
    }

    [Header("不同工作类型的效率值设置")]
    [Header("依次为：None, Frenzy, Fast, Normal, Slow, VerySlow")]
    public List<float> workEfficiencyValuesList = new List<float>
    {
        175f, 125f, 100f, 75f, 50f
    };
    public Dictionary<CreatureCardType, CreatureCardAttribute> creatureCardAttributes = new Dictionary<CreatureCardType, CreatureCardAttribute>();
    public Dictionary<WorkEfficiencyType, float> workEfficiencyValues = null;

    private void InitializeWorkEfficiencyValues()
    {
        for (int i = 0; i < workEfficiencyValuesList.Count; i++)
        {
            workEfficiencyValues[(WorkEfficiencyType)i] = workEfficiencyValuesList[i];
        }
    }
    #endregion

    #region 资源卡
    public struct ResourceCardAttribute
    {
        [Header("资源卡设置")]
        [Tooltip("资源卡分类")] public ResourceCardClassification resourceClassification;
        [Tooltip("资源卡耐久值")] public int durability;
    }
    public Dictionary<ResourceCardType, ResourceCardAttribute> resourceCardAttributes = new Dictionary<ResourceCardType, ResourceCardAttribute>();
    public ResourceCardClassification GetResourceCardClassification(ResourceCardType resourceCardType) 
        => resourceCardAttributes.ContainsKey(resourceCardType) ? 
            resourceCardAttributes[resourceCardType].resourceClassification : 
            ResourceCardClassification.None;
    public WorkEfficiencyType GetWorkEfficiencyType(CreatureCardType creatureCardType)
        => creatureCardAttributes.ContainsKey(creatureCardType) ?
            creatureCardAttributes[creatureCardType].craftWorkEfficiency :
            WorkEfficiencyType.None;

    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
    {
        if (workEfficiencyValues == null)
        {
            workEfficiencyValues = new Dictionary<WorkEfficiencyType, float>();
            InitializeWorkEfficiencyValues();
        }
        return workEfficiencyValues.ContainsKey(workEfficiencyType) ?
            workEfficiencyValues[workEfficiencyType] :
            0.0f;
    }

    public float GetWorkEfficiencyValue(CreatureCardType creatureCardType)
        => GetWorkEfficiencyValue(GetWorkEfficiencyType(creatureCardType));

    public bool IsResourcePoint(ResourceCardType resourceCardType)
        => resourceCardAttributes.ContainsKey(resourceCardType) &&
        resourceCardAttributes[resourceCardType].resourceClassification == ResourceCardClassification.ResourcePoint;

    public int GetDurability(ResourceCardType cardType) => 
        resourceCardAttributes.ContainsKey(cardType) ?
        resourceCardAttributes[cardType].durability : 1;

    
    #endregion

    #region 图标内容
    public Dictionary<CardType, Dictionary<int, Sprite>> cardIcons = new Dictionary<CardType, Dictionary<int, Sprite>>();

    public bool TryGetCardIcon(Card.CardDescription cardDescription, out Sprite icon)
    {
        icon = null;
        return cardIcons.TryGetValue(cardDescription.cardType, out var typeDict) && typeDict.TryGetValue(
            cardDescription.cardType switch
            {
                CardType.Creatures => (int)cardDescription.creatureCardType,
                CardType.Resources => (int)cardDescription.resourceCardType,
                CardType.Events => (int)cardDescription.eventCardType,
                _ => -1,
            }, out icon);
    }
    #endregion
}