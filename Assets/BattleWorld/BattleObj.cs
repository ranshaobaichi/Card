using UnityEngine;

namespace Category.BattleWorld {
  public interface IBattleObj {
    public void OnBattleStart();
    public void OnBattleEnd();
  }
  
  public interface IBattleUpdateable {
    public void Tick();
  }

  public interface IBattleDamageable {
    /// <summary>
    /// Damageable objects will take damage
    /// </summary>
    /// <returns>whether the attack was successful (e.g. not dodged or blocked)</returns>
    public bool TakeDamage(float damage, Battle.DamageType damageType, bool isNormalAttack);

    public bool IsDead();
  }

  public interface IBattleCreature : IBattleObj, IBattleUpdateable { }
  
  /// <summary>
  /// 所有战斗世界对象的基类。
  /// 挂载在战斗场景中的实体上，统一管理其所在格子、战斗状态与阵营等信息。
  /// </summary>
  public class B_Obj : MonoBehaviour {
    /// <summary>
    /// 当前对象所在的六边形格子节点引用。
    /// 为 <c>null</c> 时表示当前不在任何战斗格子中。
    /// </summary>
    public HexNode hexNode;

    /// <summary>
    /// 是否处于战斗状态。
    /// 当且仅当 <see cref="hexNode"/> 不为 <c>null</c> 时，认为对象已进入战斗世界。
    /// </summary>
    public bool inBattle => hexNode != null;

    /// <summary>
    /// 当前对象所属的阵营信息。
    /// </summary>
    public Battle.LineUp lineUp;
  }
}