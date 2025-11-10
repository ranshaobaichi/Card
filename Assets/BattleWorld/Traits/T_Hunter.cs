using System.Collections.Generic;
using Category.Battle;

public class T_Hunter : B_Trait, ITraitHolder
{
    public override int MaxLevel => throw new System.NotImplementedException();

    public override List<int> levelThresholds => throw new System.NotImplementedException();

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        throw new System.NotImplementedException();
    }

    public void OnBattleEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnBattleStart()
    {
        throw new System.NotImplementedException();
    }
}