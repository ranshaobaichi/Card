using System.Collections.Generic;
using Category;
using UnityEngine;

public class B_Creatures : B_Object
{
    [SerializeField]
    private B_CreatureState currentState;
    public bool IsMovingState => currentState switch { B_CreatureState.FindingPath => true, _ => false };
    public LineUp LineUp;

    public int attackDistance;
    public float attackDamage;
    private B_Creatures targetCreature;
    private HexNode targetNode;

    public B_CreatureState GetCreatureState() => currentState;
    public void ChangeState(B_CreatureState newState)
    {
        if (currentState == newState) return;
        Debug.Log($"Creature {transform.name} changed state from {currentState} to {newState}");
        currentState = newState;
    }
    /// <summary>
    /// Tick前调用，决定当前状态：
    /// 若有敌人在攻击范围内，切换到攻击状态
    /// 若无敌人在攻击范围内，切换到寻路状态
    /// </summary>
    public override void BeforeTick()
    {
        List<B_Creatures> enemies = BattleWorldObjManager.GetCreatures(LineUp == LineUp.Player ? LineUp.Enemy : LineUp.Player);

        // Check if there is any enemy in attack range
        bool hasEnemyInRange = false;
        foreach (var enemy in enemies)
        {
            if (hexNode.GetDistance(enemy.hexNode) < attackDistance)
            {
                hasEnemyInRange = true;
                ChangeState(B_CreatureState.Attacking);
                targetCreature = enemy;
                targetNode = enemy.hexNode;
                break;
            }
        }

        // If no enemy in range, switch to Findi  ngPath state
        if (!hasEnemyInRange)
        {
            ChangeState(B_CreatureState.FindingPath);
            targetCreature = enemies.Count > 0 ? enemies[0] : null; // 先简单处理，选第一个敌人
        }
    }

    /// <summary>
    /// Tick中调用，执行当前状态的逻辑
    /// 若是攻击状态，执行攻击逻辑，按照优先级预扣除血量，若击杀则切换状态
    /// 若是寻路状态，执行寻路逻辑，预留移动位置
    /// 若是Idle或Dead状态，不执行任何逻辑
    /// </summary>
    /// <exception cref="System.Exception"></exception>
    public override void Tick()
    {
        switch (currentState)
        {
            case B_CreatureState.Dead:
            case B_CreatureState.Idle:
                // Do nothing
                return;
            case B_CreatureState.FindingPath:
                // Find path to target
                var path = Pathfinding.FindPath(hexNode, targetCreature.hexNode);
                if (path != null && path.Count > 1)
                {
                    HexNodeManager.ReserveObject(this, path[1]);
                    targetNode = path[1];
                }
                return;
            case B_CreatureState.Attacking:
                // Attack the target
                return;
            default:
                throw new System.Exception("Unknown State");
        }
    }

    /// <summary>
    /// Tick后调用，执行当前状态的收尾工作
    /// 若是攻击状态，播放攻击动画，确认是否击中
    /// 若是寻路状态，按照预留位置执行移动动画
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void AfterTick()
    {
        switch (currentState)
        {
            case B_CreatureState.Dead:
            // TODO: Remove from battlefield

            case B_CreatureState.Idle:
                // Do nothing
                return;
            case B_CreatureState.FindingPath:
                HexNodeManager.MoveObject(this, hexNode, targetNode);
                return;
            case B_CreatureState.Attacking:
                // Play attack animation and hit the target
                HitTarget();
                return;
            default:
                throw new System.Exception("Unknown State");
        }
    }

    private void HitTarget()
    {
        // Implement hit logic here
        Debug.Log($"Creature {transform.name} hits {targetCreature.transform.name} for {attackDamage} damage");
        targetCreature.TakeDamage(attackDamage);
    }

    public void TakeDamage(float damage)
    {
        // Implement damage logic here
        Debug.Log($"Creature {transform.name} took {damage} damage");
    }
}