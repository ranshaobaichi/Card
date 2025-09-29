using UnityEngine;
using Category;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardAttributeDB", menuName = "ScriptableObjects/CardAttributeDB")]
public class CardAttributeDB : ScriptableObject
{
    #region 单例
    private static readonly string _resourcePath = "ScriptableObjects/E_CardAttribute";
    private static CardAttributeDB _instance;
    public static CardAttributeDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CardAttributeDB>(_resourcePath);
                if (_instance == null)
                {
                    Debug.LogWarning($"未能从{_resourcePath}找到CardAttribute资源，创建了一个新实例。请确保在Resources文件夹中有配置好的CardAttribute资源。");
                    _instance = CreateInstance<CardAttributeDB>();
                }
                _instance.InitializeWorkEfficiencyValues();
            }
            return _instance;
        }
    }
    #endregion

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

    public Dictionary<CreatureCardType, CreatureCardAttribute> creatureCardAttributes = new Dictionary<CreatureCardType, CreatureCardAttribute>();

    [Header("不同工作类型的效率值设置")]
    [Header("依次为：None, Frenzy, Fast, Normal, Slow, VerySlow")]
    public List<float> workEfficiencyValuesList = new List<float>
    {
        175f, 125f, 100f, 75f, 50f
    };
    private Dictionary<WorkEfficiencyType, float> workEfficiencyValues = new Dictionary<WorkEfficiencyType, float>();
    private void InitializeWorkEfficiencyValues()
    {
        for (int i = 0; i < workEfficiencyValuesList.Count; i++)
        {
            workEfficiencyValues[(WorkEfficiencyType)i] = workEfficiencyValuesList[i];
        }
    }

    public WorkEfficiencyType GetWorkEfficiency(CreatureCardType creatureCardType)
        => creatureCardAttributes.ContainsKey(creatureCardType) ?
            creatureCardAttributes[creatureCardType].craftWorkEfficiency :
            WorkEfficiencyType.None;

    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
        => workEfficiencyValues.ContainsKey(workEfficiencyType) ?
            workEfficiencyValues[workEfficiencyType] :
            0.0f;

    public float GetWorkEfficiencyValue(CreatureCardType creatureCardType)
        => GetWorkEfficiencyValue(GetWorkEfficiency(creatureCardType));

    #endregion

    #region 资源卡
    public struct ResourceCardAttribute
    {
        [Header("资源卡设置")]
        [Tooltip("是否为资源点")] public bool isResourcePoint;
    }

    public Dictionary<ResourceCardType, ResourceCardAttribute> resourceCardAttributes = new Dictionary<ResourceCardType, ResourceCardAttribute>();

    public bool IsResourcePoint(ResourceCardType resourceCardType)
        => resourceCardAttributes.ContainsKey(resourceCardType) && resourceCardAttributes[resourceCardType].isResourcePoint;
    #endregion
}