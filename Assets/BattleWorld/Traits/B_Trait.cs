using System;
using System.Collections.Generic;
using UnityEngine;

using Category.Battle;
using static CardAttributeDB.CreatureCardAttribute;

public interface ITraitHolder
{
    /// <summary>
    /// Called at the start of the battle
    /// Should use ModifyAttributes to modify creature attributes
    /// Should add attack effects if needed
    /// </summary>
    public void OnBattleStart();
    /// <summary>
    /// Deal with the pernament effects bounsed by this trait after battle ends
    /// </summary>
    public void OnBattleEnd();
    /// <summary>
    /// Modify the base attributes of the creature and modify in place
    /// Do Not modify the attack effects here, add them in OnBattleStart instead
    /// </summary>
    /// <param name="baseAttributes">
    /// which need to be modified
    /// </param>
    public void ModifyAttributes(BasicAttributes baseAttributes, B_Creature creature = null);
}

public abstract class B_Trait : MonoBehaviour
{
    public Trait traitType;
    public int level
    {
        get
        {
            for (int i = levelThresholds.Count - 1; i >= 0; i--)
            {
                if (currentTraitCreatureCount >= levelThresholds[i])
                    return Mathf.Max(i, MaxLevel);
            }
            return 0;
        }
    }
    abstract public int MaxLevel { get; }
    public int currentTraitCreatureCount;
    public LineUp lineUp;
    abstract public List<int> levelThresholds { get; } // the number of creatures required to reach each level
    protected List<B_Creature> inBattleCreatures => BattleWorldManager.Instance.GetInBattleCreatures(lineUp);

    /// <summary>
    /// Get the number of creatures needed to reach the next level of this trait
    /// </summary>
    /// <returns>
    /// The number of creatures needed to reach the next level of this trait, or -1 if max level is reached.
    /// </returns>
    public int GetCreatureCountNeededForNextLevel()
    {
        int curLevel = level;
        int nextThreshold = levelThresholds[Mathf.Max(curLevel + 1, MaxLevel)];
        int need = nextThreshold - currentTraitCreatureCount;
        return Mathf.Abs(need);
    }
}