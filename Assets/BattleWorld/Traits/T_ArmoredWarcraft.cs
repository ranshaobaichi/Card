using System.Collections;
using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_ArmoredWarcraft : B_Trait, ITraitHolder
{
    private List<int> ArmorSpellResistanceBonusPerLevel = new List<int> { 0, 5, 10, 15 };
    private int bonusIntervaslSeconds = 5;
    void Start() => traitType = Trait.装甲魔兽;

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
    }

    public void OnBattleEnd()
    {
        StopAllCoroutines();
    }

    public void OnBattleStart()
    {
        StartCoroutine(AddArmorAndSpellResistanceBuffs());
    }

    private IEnumerator AddArmorAndSpellResistanceBuffs()
    {
        while (BattleWorldManager.Instance.InBattle)
        {
            yield return new WaitForSeconds(bonusIntervaslSeconds);
            int level = this.level;
            if (level > 0)
            {
                int bonus = ArmorSpellResistanceBonusPerLevel[level];
                foreach (var creature in inBattleCreatures)
                {
                    creature.curAttribute.armor += bonus;
                    creature.curAttribute.spellResistance += bonus;
                }
            }
        }
    }
}