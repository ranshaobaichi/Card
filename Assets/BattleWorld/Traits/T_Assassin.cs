using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_Assassin : B_Trait, ITraitHolder
{
    private List<float> damageBonusPerLevel = new List<float> {0f, 0.1f, 0.16f, 0.25f, 0.4f, 1.0f};
    private List<float> probabilityDoubleDamagePerLevel = new List<float> {0f, 0.10f, 0.18f, 0.35f, 0.5f, 0.8f};
    public float doubleDamageProbability => probabilityDoubleDamagePerLevel[level];
    void Start() => traitType = Trait.刺客;


    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        if (creature.actAttribute.traits.Contains(traitType))
        {
            float bonus = damageBonusPerLevel[level];
            float attackPower = baseAttributes.attackPower;
            attackPower = attackPower * (1 + bonus);
            baseAttributes.attackPower = Mathf.Round(attackPower * 100f) / 100f;
        }
    }

    public void OnBattleEnd()
    {
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            ModifyAttributes(creature.actAttribute);
            if (creature.actAttribute.traits.Contains(traitType) && level > 0)
            {
                creature.attackEffetcts.Add(AttackEffetct.ProbabilityDoubleDamage);
            }
        }
    }
}