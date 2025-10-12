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

    public Dictionary<WorkEfficiencyType, float> workEfficiencyValues = new Dictionary<WorkEfficiencyType, float>();
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
        [Tooltip("是否为资源点")] public bool isResourcePoint;
    }
    public Dictionary<ResourceCardType, ResourceCardAttribute> resourceCardAttributes = new Dictionary<ResourceCardType, ResourceCardAttribute>();
    #endregion
}