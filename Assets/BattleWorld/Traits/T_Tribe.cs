using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_Tribe : B_Trait, ITraitHolder
{
    [Header("Bouns Attributes")]
    private List<float> traitCardHpAttackBonusPercentage = new List<float> { 0f, 0.12f, 0.25f, 0.40f, 0.75f, 0.75f }; // HP and attack bonus percentages for levels 0 to 4
    private List<float> allCreaturesHpBounus = new List<float> { 0f, 4f, 10f, 10f, 20f, 35f }; // HP bonus percentages for all creatures for levels 0 to 4
    public float TrueDamagePercentageOfAttackPower = 0.15f; // 15% true damage of attack power
    void Start() => traitType = Trait.部落;

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            ModifyAttributes(creature.actAttribute);
            if (creature.actAttribute.traits.Contains(traitType) && (level == 4 || level == 5))
            {
                creature.attackEffetcts.Add(AttackEffetct.TrueDamagePercentageOfAttackPower);
            }
        }
    }

    public void OnBattleEnd()
    {

    }

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        if (baseAttributes.traits.Contains(traitType))
        {
            Debug.Log("Tribe Trait ModifyAttributes Called");
            float hp = baseAttributes.health;
            hp = (hp + allCreaturesHpBounus[level]) * (1 + traitCardHpAttackBonusPercentage[level]);
            hp = Mathf.Round(hp * 100f) / 100f;

            float attack = baseAttributes.attackPower;
            attack = attack * (1 + traitCardHpAttackBonusPercentage[level]);
            attack = Mathf.Round(attack * 100f) / 100f;

            baseAttributes.health = hp;
            baseAttributes.attackPower = attack;
        }
        else
        {
            baseAttributes.health += allCreaturesHpBounus[level];
        }
    }
}