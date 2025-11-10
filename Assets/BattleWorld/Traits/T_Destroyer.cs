using System.Collections.Generic;
using Category.Battle;

public class T_Destroyer : B_Trait, ITraitHolder
{
    public override int MaxLevel => 5;
    public override List<int> levelThresholds => new List<int> {1, 3, 5, 7, 9};
    private List<float> _percentageOftargetHealth = new List<float> {0f, 0.03f, 0.05f, 0.08f, 0.12f, 0.12f};
    public float percentageOftargetHealth => _percentageOftargetHealth[level];

    void Start() => traitType = Trait.破阵者;

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
    }

    public void OnBattleEnd()
    {
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            if (creature.actAttribute.traits.Contains(traitType) && level > 0)
            {
                creature.attackEffetcts.Add(AttackEffetct.PhysicalDamagePercentageOftargetHealth);
            }
        }
    }
}