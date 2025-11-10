using System.Collections.Generic;
using Category.Battle;

public class T_Soldier : B_Trait, ITraitHolder
{
    public override int MaxLevel => 4;
    public override List<int> levelThresholds => new List<int> { 2, 4, 6, 8 };
    private List<float> healthBonus = new List<float> { 0f, 7f, 14f, 30f, 55f };
    private List<float> belowAttackBonus = new List<float> { 0f, 2f, 5f, 8f, 12f };
    void Start() => traitType = Trait.线列步兵;


    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature)
    {
        bool isInBackline = creature.lineUp == LineUp.Player ?
            creature.hexNode.coord.R == 6 || creature.hexNode.coord.R == 7 :
            creature.hexNode.coord.R == 0 || creature.hexNode.coord.R == 1;
        var traits = baseAttributes.traits;
        int level = this.level;
        if (traits.Contains(traitType))
            baseAttributes.health += healthBonus[level];
        if (isInBackline)
            baseAttributes.attackPower += belowAttackBonus[level];
    }

    public void OnBattleEnd()
    {
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
                ModifyAttributes(creature.actAttribute, creature);
        }
    }
}