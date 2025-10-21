using System.Collections.Generic;
using Category.Production;
using UnityEngine;

public class CreatureCard : Card
{
    [Header("生物卡设置")]
    [Header("生产设置")]
    [Header("工作类型与效率")]
    [Tooltip("合成类型")] public WorkEfficiencyType craftWorkEfficiency;
    [Tooltip("探索类型")] public WorkEfficiencyType exploreWorkEfficiency;
    [Tooltip("建造类型")] public WorkEfficiencyType interactWorkEfficiency;
    private Dictionary<WorkType, WorkEfficiencyType> workEfficiencies;

    public WorkEfficiencyType GetWorkEfficiency(WorkType workType) =>
        workEfficiencies.TryGetValue(workType, out var efficiency) ? efficiency : WorkEfficiencyType.None;

    public new void Start()
    {
        base.Start();

        // Initialize work efficiencies
        workEfficiencies = new Dictionary<WorkType, WorkEfficiencyType>
        {
            { WorkType.Craft, craftWorkEfficiency },
            { WorkType.Explore, exploreWorkEfficiency },
            { WorkType.Interact, interactWorkEfficiency }
        };
    }

    // public override void SetCardType()
    // {
    //     cardType.cardType = CardType.Creatures;

    //     // TEST: Set creature card type
    //     cardType.creatureCardType = CreatureCardType.Worker;
    // }
}
