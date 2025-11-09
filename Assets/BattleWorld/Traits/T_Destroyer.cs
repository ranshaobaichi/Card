using System.Collections.Generic;
using Category.Battle;

public class T_Destroyer : B_Trait, ITraitHolder
{
    public void ApplyAttribute(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes)
    {
        return;
    }

    public void ApplyAttackEffect(List<AttackEffetct> attackEffetcts)
    {
        return;
    }

    public void OnBattleStart()
    {
        throw new System.NotImplementedException();
    }
}