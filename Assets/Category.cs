using System;
using UnityEngine;

namespace Category
{
    #region 卡牌类型
    public enum CardType
    {
        None,
        Resources,
        Creatures,
        Events,
    }

    [Serializable]
    public enum ResourceCardType
    {
        None,
        探索点,
        挖掘点,
        坍塌的房间,
        废弃矿坑,
        遗弃的蘑菇农场,
        铁矿脉,
        稀有矿脉,
        深坑,
        精怪工坊,
        营养土壤间,
        酒馆,
        拷打室,
        部落安全屋,
        黑魔法藏室,
        宝箱陷阱,
        地面上的货币,
        宝箱,
        石料,
        烂木头,
        铁矿石,
        银矿石,
        金矿石,
        切割石砖,
        超大石料,
        木料,
        铁,
        白银,
        粗糙金块,
        劣质金币,
        金币,
        尸体,
        蘑菇,
        肉块,
        真菌啤酒,
        探索口粮,
    }

    [Serializable]
    public enum ResourceCardClassification
    {
        None,
        Food,
        Equipment,
        ResourcePoint,
        Others,
    }

    [Serializable]
    public enum CreatureCardType
    {
        None,
        Any,
        小鬼,
        部落拾荒者,
        寻鼠人,
        大地精斥候,
        部族蛮兵,
        兽人暴徒,
        部落猎头者,
        巨角萨满,
        凶残头目,
        鹰身女妖伏击者,
        山居女妖,
        石像魔,
        古代石像,
        林中少女,
        群山旅人,
        半人马,
        边缘龙血,
        农民,
        征召民兵,
        巨蜘蛛幼崽,
        巨蜘蛛成体,
        巨蜘蛛首领,
    }

    public enum EventCardType
    {
        None,
        蜘蛛巡逻队,
        蜘蛛巢穴,
        失落的魔像,
        新生的精灵,
        鹰身女妖的造访,
        精魂躁动的源头,
    }
    #endregion

    public enum GameTimeState
    {
        ProduceState,
        SettlementState,
        BattleState,
    }

    namespace Production
    {
        [Tooltip("工作类型")]
        public enum WorkType
        {
            None,
            [Tooltip("合成")] Craft,
            [Tooltip("探索")] Explore,
            [Tooltip("互动")] Interact
        }

        [Serializable]
        public enum WorkEfficiencyType
        {
            None,
            [Tooltip("极慢")] VerySlow = 1,
            [Tooltip("缓慢")] Slow = 2,
            [Tooltip("普通")] Normal = 3,
            [Tooltip("快速")] Fast = 4,
            [Tooltip("狂热")] Frenzy = 5,
        }
    }

    namespace Battle
    {
        [Serializable]
        public enum B_CreatureState
        {
            Idle,
            FindingPath,
            Attacking,
            Dead,
        }

        [Serializable]
        public enum LineUp
        {
            Player,
            Enemy
        }

        [Serializable]
        public enum DamageType
        {
            Physical,
            Spell,
            TrueDamage
        }

        [Serializable]
        public enum Trait
        {
            精魂 = 0,
            部落 = 1,
            装甲魔兽 = 2,
            刺客 = 3,
            线列步兵 = 4,
            破阵者 = 5,
            施法者 = 6,
            猎手 = 7,
            冲击骑兵 = 8,
        }

        [Serializable]
        public enum AttackEffetct
        {
            TrueDamagePercentageOfAttackPower,
            PhysicalDamagePercentageOftargetHealth,
            ProbabilityDoubleDamage,
            BounceAttack,
        }
    }
}