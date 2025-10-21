using UnityEngine;
using Category.Production;
using Category;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardAttributeDB", menuName = "ScriptableObjects/CardAttributeDB")]
public class CardAttributeDB : ScriptableObject
{
    #region 生物卡
    public struct CreatureCardAttribute
    {
        /// <summary>
        /// 基础属性
        /// </summary>
        public class BasicAttributes
        {
            public class WorkEfficiencyAttributes
            {
                public Category.Production.WorkEfficiencyType craftWorkEfficiency;  // 合成效率
                public Category.Production.WorkEfficiencyType exploreWorkEfficiency;    // 探索效率
                public Category.Production.WorkEfficiencyType interactWorkEfficiency;   // 互动效率
            }
            public WorkEfficiencyAttributes workEfficiencyAttributes;
            public int satiety; // 饱食度
            public float health; // 生命值
            public int experience; // 首次升级所需经验值
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
        }

        /// <summary>
        /// 成长属性
        /// </summary>
        public struct LevelUpAttributes
        {
            public int satietyGrowth; // 饱食度成长
            public float healthGrowth; // 生命值成长
            public float attackPowerGrowth; // 攻击力成长
            public float spellPowerGrowth; // 法术强度成长
            public float armorGrowth; // 护甲值成长
            public float spellResistanceGrowth; // 魔法抗性成长
            public float dodgeRateGrowth; // 闪避率成长
            public int attackSpeedGrowth; // 攻击速度成长
            public int attackRangeGrowth; // 攻击距离成长
            // public Dictionary<int, Category.Production.WorkEfficiencyType> workEfficiencyGrowth; // 工作效率成长
        }

        public static int maxLevel; // 最大等级
        public static float levelUpExpIncreasePercent; // 每次升级所需经验提高的百分比
        public BasicAttributes basicAttributes;
        public LevelUpAttributes levelUpAttributes;
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
    [SerializeField]
    public struct ResourceCardAttribute
    {
        [Header("资源卡设置")]
        [Tooltip("资源卡分类")] public ResourceCardClassification resourceClassification;
        [Tooltip("资源卡耐久值")] public int durability;
        [Tooltip("资源卡饱腹值")] public int satietyValue; // 仅resourceClassification为Food时有效
    }
    public Dictionary<ResourceCardType, ResourceCardAttribute> resourceCardAttributes = new Dictionary<ResourceCardType, ResourceCardAttribute>();
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

    #region APIs
    public T GetCardAttribute<T>(Card.CardDescription cardDescription) where T : struct
    {
        switch (cardDescription.cardType)
        {
            case CardType.Creatures when typeof(T) == typeof(CreatureCardAttribute):
                if (creatureCardAttributes.TryGetValue(cardDescription.creatureCardType, out var creatureAttr))
                {
                    return (T)(object)creatureAttr;
                }
                break;
            case CardType.Resources when typeof(T) == typeof(ResourceCardAttribute):
                if (resourceCardAttributes.TryGetValue(cardDescription.resourceCardType, out var resourceAttr))
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

    public ResourceCardClassification GetResourceCardClassification(ResourceCardType resourceCardType) 
        => resourceCardAttributes.ContainsKey(resourceCardType) ? 
            resourceCardAttributes[resourceCardType].resourceClassification : 
            ResourceCardClassification.None;
    public WorkEfficiencyType GetWorkEfficiencyType(CreatureCardType creatureCardType)
        => creatureCardAttributes.ContainsKey(creatureCardType) ?
            creatureCardAttributes[creatureCardType].basicAttributes.workEfficiencyAttributes.craftWorkEfficiency :
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
        
    public int GetSatietyValue(ResourceCardType cardType) =>
        resourceCardAttributes.ContainsKey(cardType) ? resourceCardAttributes[cardType].satietyValue : 1;
    #endregion
}