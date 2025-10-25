using UnityEngine;
using Category.Production;
using Category;
using System.Collections.Generic;
using System;
using UnityEditor.EditorTools;

[CreateAssetMenu(fileName = "CardAttributeDB", menuName = "ScriptableObjects/CardAttributeDB")]
public class CardAttributeDB : ScriptableObject
{
    #region 类声明
    [Serializable]
    public class CreatureCardAttribute : System.ICloneable
    {
        /// <summary>
        /// 基础属性
        /// </summary>
        [Serializable]
        public class BasicAttributes : System.ICloneable
        {
            [Serializable]
            public class WorkEfficiencyAttributes
            {
                public Category.Production.WorkEfficiencyType craftWorkEfficiency;  // 合成效率
                public Category.Production.WorkEfficiencyType exploreWorkEfficiency;    // 探索效率
                public Category.Production.WorkEfficiencyType interactWorkEfficiency;   // 互动效率
                public object Clone()
                {
                    return new WorkEfficiencyAttributes
                    {
                        craftWorkEfficiency = this.craftWorkEfficiency,
                        exploreWorkEfficiency = this.exploreWorkEfficiency,
                        interactWorkEfficiency = this.interactWorkEfficiency
                    };
                }
            }
            public WorkEfficiencyAttributes workEfficiencyAttributes;
            public int EXP;  // 当前经验值
            public int level;
            public int satiety; // 饱食度
            public float health; // 生命值
            public float attackPower; // 攻击力
            public float spellPower; // 法术强度
            public Category.Battle.DamageType normalAttackDamageType; // 普通攻击伤害类型
            public float armor; // 护甲值
            public float spellResistance; // 魔法抗性
            public int moveSpeed; // 移动速度
            public float dodgeRate; // 闪避率
            public int attackSpeed; // 攻击速度
            public int attackRange; // 攻击距离
            // public string attackEffect; // 攻击特效
            // public List<string> skills; // 技能
            public List<DropCard> dropItem; // 掉落物
            public object Clone()
            {
                return new BasicAttributes
                {
                    workEfficiencyAttributes = (WorkEfficiencyAttributes)this.workEfficiencyAttributes.Clone(),
                    EXP = this.EXP,
                    level = this.level,
                    satiety = this.satiety,
                    health = this.health,
                    attackPower = this.attackPower,
                    spellPower = this.spellPower,
                    normalAttackDamageType = this.normalAttackDamageType,
                    armor = this.armor,
                    spellResistance = this.spellResistance,
                    moveSpeed = this.moveSpeed,
                    dodgeRate = this.dodgeRate,
                    attackSpeed = this.attackSpeed,
                    attackRange = this.attackRange,
                    dropItem = new List<DropCard>(this.dropItem)
                };
            }
        }

        /// <summary>
        /// 成长属性
        /// </summary>
        [Serializable]
        public class LevelUpAttributes : System.ICloneable
        {
            public int satietyGrowth; // 饱食度成长
            public float healthGrowth; // 生命值成长
            public float attackPowerGrowth; // 攻击力成长
            public float spellPowerGrowth; // 法术强度成长
            public float armorGrowth; // 护甲值成长
            public float spellResistanceGrowth; // 魔法抗性成长
            public int moveSpeedGrowth; // 移动速度成长 -- 注意：移动速度成长需要存储减少值，不是增加值
            public float dodgeRateGrowth; // 闪避率成长
            public int attackSpeedGrowth; // 攻击速度成长 -- 注意：攻击速度成长需要存储减少值，不是增加值
            public int attackRangeGrowth; // 攻击距离成长
            // public Dictionary<int, Category.Production.WorkEfficiencyType> workEfficiencyGrowth; // 工作效率成长
            public object Clone()
            {
                return new LevelUpAttributes
                {
                    satietyGrowth = this.satietyGrowth,
                    healthGrowth = this.healthGrowth,
                    attackPowerGrowth = this.attackPowerGrowth,
                    spellPowerGrowth = this.spellPowerGrowth,
                    armorGrowth = this.armorGrowth,
                    spellResistanceGrowth = this.spellResistanceGrowth,
                    dodgeRateGrowth = this.dodgeRateGrowth,
                    attackSpeedGrowth = this.attackSpeedGrowth,
                    attackRangeGrowth = this.attackRangeGrowth
                };
            }
        }

        public object Clone()
        {
            return new CreatureCardAttribute
            {
                creatureCardType = this.creatureCardType,
                basicAttributes = (BasicAttributes)this.basicAttributes.Clone(),
                levelUpAttributes = (LevelUpAttributes)this.levelUpAttributes.Clone()
            };
        }

        public static int maxLevel; // 最大等级
        public int levelUpExpIncreasePercent; // 每次升级所需经验提高的百分比
        public CreatureCardType creatureCardType;
        public BasicAttributes basicAttributes = new BasicAttributes();
        public LevelUpAttributes levelUpAttributes = new LevelUpAttributes();
    }

    [Serializable]
    public class ResourceCardAttribute : System.ICloneable
    {
        [Header("资源卡设置")]
        [Tooltip("资源卡类型")] public ResourceCardType resourceCardType;
        [Tooltip("资源卡分类")] public ResourceCardClassification resourceClassification;
        [Tooltip("资源卡耐久值")] public int durability;
        [Tooltip("资源卡饱腹值")] public int satietyValue; // 仅resourceClassification为Food时有效
        [Tooltip("资源卡价值")] public int priceValue;
        public object Clone()
        {
            return new ResourceCardAttribute
            {
                resourceClassification = this.resourceClassification,
                durability = this.durability,
                satietyValue = this.satietyValue
            };
        }
    }
    #endregion

    # region 实际存储数据列表
    
    [Header("不同工作类型的效率值设置")]
    [Header("依次为：None, Frenzy, Fast, Normal, Slow, VerySlow")]
    public List<float> workEfficiencyValues = new List<float>
    {
        175f, 125f, 100f, 75f, 50f
    };
    [Header("生物卡属性列表")]
    public List<CreatureCardAttribute> creatureCardAttributes;
    [Header("资源卡属性列表")]
    public List<ResourceCardAttribute> resourceCardAttributes;
    // [Header("卡牌图标列表")]
    // public List<Sprite> creatureCardIcons;
    // public List<Sprite> resourceCardIcons;
    // public List<Sprite> eventCardIcons;
    #endregion

    # region 辅助字典
    public Dictionary<WorkEfficiencyType, float> workEfficiencyValuesDict = null;
    public Dictionary<CreatureCardType, CreatureCardAttribute> creatureCardAttributesDict = null;
    public Dictionary<ResourceCardType, ResourceCardAttribute> resourceCardAttributesDict = null;
    public Dictionary<CardType, Dictionary<int, Sprite>> cardIcons = new Dictionary<CardType, Dictionary<int, Sprite>>();
    # endregion

    # region 初始化辅助字典方法
    private void InitializeWorkEfficiencyValues()
    {
        workEfficiencyValuesDict = new Dictionary<WorkEfficiencyType, float>();
        for (int i = 0; i < workEfficiencyValues.Count; i++)
        {
            workEfficiencyValuesDict[(WorkEfficiencyType)i] = workEfficiencyValues[i];
        }
    }
    private void InitializeCreatureCardAttributesDict()
    {
        creatureCardAttributesDict = new Dictionary<CreatureCardType, CreatureCardAttribute>();
        foreach (var attr in creatureCardAttributes)
        {
            creatureCardAttributesDict[attr.creatureCardType] = attr;
        }
    }

    private void InitializeResourceCardAttributesDict()
    {
        resourceCardAttributesDict = new Dictionary<ResourceCardType, ResourceCardAttribute>();
        foreach (var attr in resourceCardAttributes)
        {
            resourceCardAttributesDict[attr.resourceCardType] = attr;
        }
    }

    // private void InitializeCardIcons()
    // {
    //     // 生物卡图标
    //     var creatureDict = new Dictionary<int, Sprite>();
    //     foreach (var type in Enum.GetValues(typeof(CreatureCardType)))
    //     {
    //         int index = (int)type;
    //         if (index >= 0 && index < creatureCardIcons.Count)
    //         {
    //             creatureDict[index] = creatureCardIcons[index];
    //         }
    //     }
    //     cardIcons[CardType.Creatures] = creatureDict;

    //     // 资源卡图标
    //     var resourceDict = new Dictionary<int, Sprite>();
    //     foreach (var type in Enum.GetValues(typeof(ResourceCardType)))
    //     {
    //         int index = (int)type;
    //         if (index >= 0 && index < resourceCardIcons.Count)
    //         {
    //             resourceDict[index] = resourceCardIcons[index];
    //         }
    //     }
    //     cardIcons[CardType.Resources] = resourceDict;

    //     // 事件卡图标
    //     var eventDict = new Dictionary<int, Sprite>();
    //     foreach (var type in Enum.GetValues(typeof(EventCardType)))
    //     {
    //         int index = (int)type;
    //         if (index >= 0 && index < eventCardIcons.Count)
    //         {
    //             eventDict[index] = eventCardIcons[index];
    //         }
    //     }
    //     cardIcons[CardType.Events] = eventDict;
    // }
    # endregion

    #region APIs
    public void Initialize()
    {
        Debug.Log("Initializing CardAttributeDB...");
        InitializeWorkEfficiencyValues();
        InitializeCreatureCardAttributesDict();
        InitializeResourceCardAttributesDict();
        // InitializeCardIcons();
    }
    public T GetCardAttribute<T>(Card.CardDescription cardDescription)
    {
        switch (cardDescription.cardType)
        {
            case CardType.Creatures when typeof(T) == typeof(CreatureCardAttribute):
                if (creatureCardAttributesDict.TryGetValue(cardDescription.creatureCardType, out var creatureAttr))
                {
                    return (T)(object)creatureAttr;
                }
                break;
            case CardType.Resources when typeof(T) == typeof(ResourceCardAttribute):
                if (resourceCardAttributesDict.TryGetValue(cardDescription.resourceCardType, out var resourceAttr))
                {
                    return (T)(object)resourceAttr;
                }
                break;
            // 如果有其他类型的卡牌，可以在这里添加处理
            default:
                Debug.LogWarning($"CardAttributeDB: No attributes found for the given card description or type mismatch with {typeof(T).Name}");
                break;
        }
        return default;
    }

    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
    {
        return workEfficiencyValuesDict.ContainsKey(workEfficiencyType) ?
            workEfficiencyValuesDict[workEfficiencyType] :
            0.0f;
    }

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