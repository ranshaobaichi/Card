using System;
using System.Collections.Generic;
using UnityEngine;

using Category.Battle;
using static CardAttributeDB.CreatureCardAttribute;

public interface ITraitHolder
{
    public void ApplyAttribute(BasicAttributes baseAttributes);
    public void ApplyAttackEffect(List<AttackEffetct> attackEffetcts);
    public void OnBattleStart();
}

public abstract class B_Trait : MonoBehaviour
{
    public Trait traitType;
    public int level
    {
        get
        {
            // TODO
            throw new NotImplementedException();
        }
    }
    public int currentTraitCreatureCount;
    public List<int> levelThresholds; // the number of creatures required to reach each level

    /// <summary>
    /// Get the number of creatures needed to reach the next level of this trait
    /// </summary>
    /// <returns>
    /// The number of creatures needed to reach the next level of this trait, or -1 if max level is reached.
    /// </returns>
    public int GetCreatureCountNeededForNextLevel()
    {
        // TODO
        throw new NotImplementedException();
    }
}