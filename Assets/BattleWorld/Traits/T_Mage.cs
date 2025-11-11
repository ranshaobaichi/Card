using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_Mage : B_Trait, ITraitHolder
{
    private List<float> spellDamageBonusPerLevel = new List<float> {0f, 0.15f, 0.40f, 0.70f};
    private List<float> spellBonusPerSoulPerLevel = new List<float> {0f, 0.02f, 0.05f, 0.08f};
    void Start() => traitType = Trait.施法者;

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        if (creature.actAttribute.traits.Contains(traitType))
        {
            float bonus1 = spellDamageBonusPerLevel[level];
            float bonus2 = spellBonusPerSoulPerLevel[level] * activeTraitDict[Trait.精魂].currentTraitCreatureCount;
            float spellPower = baseAttributes.spellPower;
            spellPower *= (1 + bonus1 + bonus2);
            baseAttributes.spellPower = Mathf.Round(spellPower * 100f) / 100f;
        }
    }

    public void OnBattleEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            ModifyAttributes(creature.actAttribute);
        }
    }
}