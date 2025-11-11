using System.Collections.Generic;
using Category.Battle;

public class T_Hunter : B_Trait, ITraitHolder
{
    void Start() => traitType = Trait.猎手;
    private List<int> bounceTargetCountPerLevel = new List<int> {0, 1, 3, 5};
    private List<float> bounceDamageDecreasePerLevel = new List<float> {1f, 0.6f, 0.3f, 0f};
    public int bounceTargetCount => bounceTargetCountPerLevel[level];
    public float bounceDamageDecrease => bounceDamageDecreasePerLevel[level];

    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
    }

    public void OnBattleEnd()
    {
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            if (creature.actAttribute.traits.Contains(traitType) && level > 0)
            {
                creature.attackEffetcts.Add(AttackEffetct.BounceAttack);
            }
        }
    }
}