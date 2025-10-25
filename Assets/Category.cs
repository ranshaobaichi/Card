using System;
using UnityEditor.EditorTools;
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
        石料,
        切割石砖,
        木料,
        苔藓,
        铁矿,
        银矿,
        金矿,
        黄金,
        秘银,
        金属,
        废料,
        粪便,
        火把,
        头灯,
        石头工具,
        粗铁工具,
        简单武器,
        金属武器,
        坚硬甲壳,
        头目战利品,
        肉块,
        蘑菇,
        可食用植物,
        奴隶,
        灵魂,
        人类尸体,
        巨蜘蛛尸体,
        探索点,
        挖掘点,
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
        部落拾荒者
    }

    public enum EventCardType
    {
        None,
        [Tooltip("某种事件")] Event1,
        [Tooltip("某种事件")] Event2,
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
    }
}