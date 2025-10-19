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

    public enum ResourceCardClassification
    {
        None,
        Food,
        Equipment,
        ResourcePoint,
        Others,
    }

    public enum CreatureCardType
    {
        None,
        Any,
        地精苦工,
        劣等潜伏者,
        鹰身女妖伏击者,
        古兽人萨满,
        凶残头目,
        流浪兽人,
        山居女妖,
        返祖兽人,
        游荡拾荒佬,
        古代石像,
        石像魔,
        林中少女,
        松柏巡林者,
        长柳尊者,
        白蜡穿刺者,
        幼体巨蜘蛛,
        巨蜘蛛,
        巨蜘蛛首领,
        长弓农奴,
        自由民,
        征召市民,
        侍从,
        武装侍从,
        平原男爵,
        森林男爵,
        要塞伯爵,
    }

    public enum EventCardType
    {
        None,
        [Tooltip("某种事件")] Event1,
        [Tooltip("某种事件")] Event2,
    }
    #endregion

    [Tooltip("工作类型")]
    public enum WorkType
    {
        None,
        [Tooltip("合成")] Craft,
        [Tooltip("探索")] Explore,
        [Tooltip("互动")] Interact
    }

    public enum WorkEfficiencyType
    {
        None,
        [Tooltip("狂热")] Frenzy,
        [Tooltip("快速")] Fast,
        [Tooltip("普通")] Normal,
        [Tooltip("缓慢")] Slow,
        [Tooltip("极慢")] VerySlow,
    }

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

    public enum GameTimeState
    {
        ProduceState,
        SettlementState,
        BattleState,
    }
}