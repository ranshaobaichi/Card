using System.Collections.Generic;
using Category.Battle;

public class T_Cavalry : B_Trait, ITraitHolder
{
    public void ApplyAttackEffect(List<AttackEffetct> attackEffetcts)
    {
        return;
    }

    public void ApplyAttribute(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes)
    {
        return;
    }

    public void OnBattleStart()
    {
        throw new System.NotImplementedException();
    }
}   