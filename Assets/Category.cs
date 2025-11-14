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
        王座厅探索点,
        废弃的蘑菇农场,
        蘑菇,
        金币,
        石料,
        木料,
        浅层隧道群,
        铁矿脉,
        稀有矿脉,
        铁矿石,
        金矿石,
        银矿石,
        蘑菇汁,
        真菌啤酒,
        巨型蘑菇,
        尸体,
        石砖,
        建筑材料,
        酒馆,
        锻造加工台,
        废铁,
        土壤,
        大型石料,
        初级防具,
        粗糙工具,
        废铁武器,
        深层隧道网,
        采石坑,
        灵魂,
        小圆盾,
        燃料,
        铁块,
        白银,
        金块,
        熔炉,
        精怪安全屋,
        肉食,
        地下城入口,
        附魔石匣,
        制式武器,
        传导魔杖,
        铁甲,
        白银护石,
        秘银,
        骑士巨剑,
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
        地下城领主,
        巨鼠,
        扈从,
        侍从,
        流浪骑士,
        森林男爵,

    }

    public enum EventCardType
    {
        None,
        地下城废墟1,
        地下城废墟2,
        失落的魔像,
        群山的客人,
        名副其实的猎人,
        稀薄的古老血脉
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