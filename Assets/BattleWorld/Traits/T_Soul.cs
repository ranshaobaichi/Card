using System;
using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_Soul : B_Trait, ITraitHolder
{
    public int deathCount = 0;
    [Header("Bouns Attributes")]
    public List<float> healthBonusPerLevel = new List<float> { 0, 0.01f, 0.02f, 0.04f, 0.06f, 0.06f }; // the health bonus provided at each level
    public List<float> attackBonusPerLevel = new List<float> { 0, 0, 0.01f, 0.02f, 0.03f, 0.03f }; // the attack bonus provided at each level
    public int expBonus = 25;
    private void AddDeathCount(B_Creature _) => deathCount++;
    void Start() => traitType = Trait.精魂;

    public void OnBattleStart()
    {
        deathCount = 0;
        BattleWorldManager.Instance.OnCreatureDead += AddDeathCount;
    }

    public void OnBattleEnd()
    {
        BattleWorldManager.Instance.OnCreatureDead -= AddDeathCount;
        if (lineUp == LineUp.Enemy) return;

        foreach (var creatureID in BattleWorldManager.Instance.playerDeployedCreatureIDs)
        {
            CardAttributeDB.CreatureCardAttribute attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(creatureID);
            if (attr != null && attr.basicAttributes.traits.Contains(traitType))
            {
                // health bonus
                float healthBonus = healthBonusPerLevel[level] * deathCount * attr.basicAttributes.health;
                healthBonus = Mathf.Round(healthBonus * 100f) / 100f;
                // attack bonus
                float attackBonus = attackBonusPerLevel[level] * deathCount * attr.basicAttributes.attackPower;
                attackBonus = Mathf.Round(attackBonus * 100f) / 100f;
                
                attr.basicAttributes.health += healthBonus;
                attr.basicAttributes.attackPower += attackBonus;
                ModifyAttributes(attr.basicAttributes);
                // exp bonus
                int expGain = (level == MaxLevel && deathCount >= 6) ? expBonus : 0;
                CardManager.Instance.GainEXP(creatureID, expGain);
            }
        }
    }

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
    }
}