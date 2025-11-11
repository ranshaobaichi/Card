using System.Collections.Generic;
using Category.Battle;

public class T_Cavalry : B_Trait, ITraitHolder
{
    private List<int> durationTickCountPerLevel = new List<int> {0, 72, 90, 108};
    private List<float> damageBonusPerLevel = new List<float> {0f, 0.6f, 1.2f, 2.4f};
    private List<int> resitanceBonusPerLevel = new List<int> { 0, 50, 100, 220 };
    private Dictionary<B_Creature, int> speedUpCreature = new Dictionary<B_Creature, int>();
    private int tickCount = 0;

    void Start() => traitType = Trait.冲击骑兵;
    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        if (creature.actAttribute.traits.Contains(traitType))
        {
            baseAttributes.moveSpeed = 3;
        }
    }

    public void OnBattleEnd()
    {        
        if (lineUp == LineUp.Player)
            BattleWorldManager.Instance.PlayerTick -= RecoverAttr;
        else
            BattleWorldManager.Instance.EnemyTick -= RecoverAttr;

    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            if (creature.actAttribute.traits.Contains(traitType) && level > 0)
            {
                speedUpCreature[creature] = creature.actAttribute.moveSpeed;
                ModifyAttributes(creature.actAttribute);
            }
        }
        tickCount = durationTickCountPerLevel[level];
        BattleWorldManager.Instance.OnCreatureDead += (B_Creature deadCreature) =>
        {
            if (speedUpCreature.ContainsKey(deadCreature)) speedUpCreature.Remove(deadCreature);
        };
        if (lineUp == LineUp.Player)
            BattleWorldManager.Instance.PlayerTick += RecoverAttr;
        else
            BattleWorldManager.Instance.EnemyTick += RecoverAttr;
    }

    public void RecoverAttr()
    {
        tickCount--;
        if (tickCount <= 0)
        {
            int level = this.level;
            foreach ((B_Creature creature, int originalSpeed) in speedUpCreature)
            {
                var baseAttributes = creature.actAttribute;
                baseAttributes.moveSpeed = originalSpeed;


                baseAttributes.armor -= resitanceBonusPerLevel[level];
                baseAttributes.spellResistance -= resitanceBonusPerLevel[level];
                creature.curAttribute.armor -= resitanceBonusPerLevel[level];
                creature.curAttribute.spellResistance -= resitanceBonusPerLevel[level];
            }
            speedUpCreature.Clear();
        }
    }
}   