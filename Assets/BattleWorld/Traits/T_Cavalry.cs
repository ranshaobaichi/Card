using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class T_Cavalry : B_Trait, ITraitHolder
{
    private struct CavalryAttribute
    {
        public int speed;
        public float attackPower;
        public float armor;
        public float spellResistance;
    }
    private List<int> durationTickCountPerLevel = new List<int> {0, 72, 90, 108};
    private List<float> damageBonusPerLevel = new List<float> {0f, 0.6f, 1.2f, 2.4f};
    private List<int> resitanceBonusPerLevel = new List<int> { 0, 50, 100, 220 };
    private Dictionary<B_Creature, CavalryAttribute> speedUpCreature = new Dictionary<B_Creature, CavalryAttribute>();
    private int tickCount = 0;

    void Start() => traitType = Trait.冲击骑兵;
    public void ModifyAttributes(CardAttributeDB.CreatureCardAttribute.BasicAttributes baseAttributes, B_Creature creature = null)
    {
        if (baseAttributes.traits.Contains(traitType))
        {
            int level = this.level;
            baseAttributes.moveSpeed = 3;
            baseAttributes.attackPower *= 1 + damageBonusPerLevel[level];
            baseAttributes.armor += resitanceBonusPerLevel[level];
            baseAttributes.spellResistance += resitanceBonusPerLevel[level];
        }
    }

    public void OnBattleEnd()
    {        
        if (lineUp == LineUp.Player)
            BattleWorldManager.Instance.PlayerTick -= RecoverAttr;
        else
            BattleWorldManager.Instance.EnemyTick -= RecoverAttr;
        
        // 取消订阅死亡事件
        BattleWorldManager.Instance.OnCreatureDead -= OnCreatureDeadHandler;
    }

    // 处理生物死亡事件的方法
    private void OnCreatureDeadHandler(B_Creature deadCreature)
    {
        if (speedUpCreature.ContainsKey(deadCreature)) 
            speedUpCreature.Remove(deadCreature);
    }

    public void OnBattleStart()
    {
        foreach (var creature in inBattleCreatures)
        {
            if (creature.actAttribute.traits.Contains(traitType) && level > 0)
            {
                speedUpCreature[creature] = new CavalryAttribute
                {
                    speed = creature.actAttribute.moveSpeed,
                    attackPower = creature.actAttribute.attackPower,
                    armor = creature.actAttribute.armor,
                    spellResistance = creature.actAttribute.spellResistance
                };
                ModifyAttributes(creature.actAttribute, creature);
            }
        }
        tickCount = durationTickCountPerLevel[level];
        BattleWorldManager.Instance.OnCreatureDead += OnCreatureDeadHandler;
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
            Debug.Log("Recovering attributes for cavalry trait.");
            List<B_Creature> creaturesToRecover = new List<B_Creature>(speedUpCreature.Keys);

            foreach (B_Creature creature in creaturesToRecover)
            {
                if (creature == null) continue; // 跳过已死亡或不存在的生物
                CavalryAttribute originalAttributes = speedUpCreature[creature];
                creature.actAttribute.moveSpeed = originalAttributes.speed;
                creature.actAttribute.attackPower = originalAttributes.attackPower;
                creature.actAttribute.armor = originalAttributes.armor;
                creature.actAttribute.spellResistance = originalAttributes.spellResistance;

                creature.curAttribute.attackPower = originalAttributes.attackPower;
                creature.curAttribute.armor = originalAttributes.armor;
                creature.curAttribute.spellResistance = originalAttributes.spellResistance;
            }
            speedUpCreature.Clear();
            if (lineUp == LineUp.Player)
                BattleWorldManager.Instance.PlayerTick -= RecoverAttr;
            else
                BattleWorldManager.Instance.EnemyTick -= RecoverAttr;
        }
    }
}   