using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Category.Battle;
using static CardAttributeDB.CreatureCardAttribute;
using System.Collections.Generic;
using Category.BattleWorld;
using Random = UnityEngine.Random;

public class B_Creature : B_Obj,
    IBattleCreature, IBattleDamageable,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {
  public long cardID;
  public DisplayCard displayCard;
  [NonSerialized]
  public CardAttributeDB.CreatureCardAttribute creatureCardAttribute; // readonly attribute from CardAttributeDB
  [NonSerialized]
  public BasicAttributes curAttribute; // the real-time attribute that will change during battle,
                                       // and reset to creatureCardAttribute.basicAttributes at the beginning of each battle
  [NonSerialized]
  public List<AttackEffetct> attackEffects = new List<AttackEffetct>();

  private Image m_image;
  private HexNode m_oriHexNode;
  private bool isDragging = false;

  public bool IsDead() => curAttribute.health <= 0;

  private void Awake() {
    m_image = GetComponent<Image>();
  }

  public void Init(long cardID, LineUp lineUp) {
    this.cardID = cardID;
    this.lineUp = lineUp;
    var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
    curAttribute = (BasicAttributes)attr.basicAttributes.Clone();

    var cardDescription = new Card.CardDescription {
        cardType = Category.CardType.Creatures,
        creatureCardType = attr.creatureCardType
    };
    displayCard.Initialize(cardDescription);
  }

  public void OnBattleStart() {
    // Reset current attributes to basic attributes at the beginning of each battle
    curAttribute = (BasicAttributes)creatureCardAttribute.basicAttributes.Clone();
  }
  
  public void Tick() {
    if (!inBattle || curAttribute.health <= 0) {
      return;
    }
    
    if (!BattleWorldManager.Instance.GetClosestDamageableOpponents(this, out var closestOpponents)
        ||
        !(closestOpponents is B_Obj closestOpponentObj)) {
      return;
    }

    var closestDistance = hexNode.GetDistance(closestOpponentObj.hexNode);
    if (closestDistance <= curAttribute.attackRange) {
      // Check whether there is any enemy in attack range and decrease curAttr's attack speed
      // if already to attack, add DamageActions to BattleWorldManager
      if (curAttribute.attackSpeed == 0) {
        BattleWorldManager.Instance.DamageActions += () =>
            StartCoroutine(AttackAnimation(closestOpponents, BattleWorldManager.Instance.TickInterval));
        curAttribute.attackSpeed = creatureCardAttribute.basicAttributes.attackSpeed;
      }
      else {
        curAttribute.attackSpeed -= 1;
      }
    }
    else {
      // Find the closest enemy and decrease the move speed 
      // if already to move, find path and add NormalActions to BattleWorldManager
      if (curAttribute.moveSpeed == 0) {
        var path = Pathfinding.FindPath(hexNode, closestOpponentObj.hexNode);
        if (path != null && path.Count > 1) {
          HexNodeManager.ReserveObject(this, path[index: 1]);
          BattleWorldManager.Instance.NormalActions += () =>
              StartCoroutine(MoveAnimation(path[index: 1], BattleWorldManager.Instance.TickInterval));
        }

        curAttribute.moveSpeed = creatureCardAttribute.basicAttributes.moveSpeed;
      }
      else {
        curAttribute.moveSpeed -= 1;
      }
    }
  }

  public void OnBattleEnd() {
    
  }

  private System.Collections.IEnumerator AttackAnimation(IBattleDamageable target, float duration) {
    if (target == null) {
      yield break;
    }

    if (target is not B_Obj bObj) {
      Attack(target);
      yield break;
    }

    var hasAttacked = false;
    var originalPosition = transform.position;
    var targetPosition = bObj.transform.position;
    var elapsed = 0f;
    var halfDuration = duration / 2f;
    while (elapsed < duration) {
      if (!hasAttacked && elapsed >= duration / 2f) {
        // 攻击前再次检查目标是否存在
        if (target != null) {
          Attack(target);
        }

        hasAttacked = true;
      }

      transform.position = elapsed < halfDuration ? 
          Vector3.Lerp(originalPosition, targetPosition, elapsed / halfDuration) : 
          Vector3.Lerp(targetPosition, originalPosition, (elapsed - halfDuration) / halfDuration);

      elapsed += Time.deltaTime;
      yield return null;
    }

    transform.position = originalPosition;
  }

  private void Attack(IBattleDamageable target) {
    var damage = curAttribute.attackPower;
    var damageType = curAttribute.normalAttackDamageType;
    var damageList =
        new List<(DamageType damageType, float damage, IBattleDamageable target)> 
            { new ValueTuple<DamageType, float, IBattleDamageable>(damageType, damage, target) };

    /*
     * 这里是删去的攻击效果处理逻辑，原本是直接在攻击时根据attackEffects列表中的效果来修改伤害列表
     * 目前删去羁绊但保留了attackEffects列表，后续可以根据需要重新设计攻击效果的处理方式，但需要重新设计获取数值方式
     */
    // attackEffects.ForEach(effect =>
    // {
    //     switch (effect)
    //     {
    //         case AttackEffect.TrueDamagePercentageOfAttackPower:
    //             float percentage = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.部落] is T_Tribe tribeTrait ? 
    //                 tribeTrait.TrueDamagePercentageOfAttackPower : 
    //                 0f;
    //             float trueDamage = damageList[0].damage * percentage;
    //             trueDamage = Mathf.Round(trueDamage * 100f) / 100f;
    //             damageList.Add((DamageType.TrueDamage, trueDamage, target));
    //             break;
    //         case AttackEffect.ProbabilityDoubleDamage:
    //             float probability = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.刺客] is T_Assassin assassinTrait ? assassinTrait.doubleDamageProbability : 0f;
    //             float roll = Random.Range(0f, 1f);
    //             roll = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;
    //             if (roll <= probability)
    //             {
    //                 damageList[0] = (damageList[0].damageType, damageList[0].damage * 2, damageList[0].target);
    //                 Debug.Log($"{transform.name} triggered double damage!");
    //             }
    //             break;
    //         case AttackEffect.PhysicalDamagePercentageOfTargetHealth:
    //             if (damageType == DamageType.Physical)
    //             {
    //                 float healthPercentage = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.破阵者] is T_Destroyer destroyerTrait ? destroyerTrait.percentageOfTargetHealth : 0f;
    //                 float extraDamage = target.actAttribute.health * healthPercentage;
    //                 extraDamage = Mathf.Round(extraDamage * 100f) / 100f;
    //                 damageList.Add((DamageType.Physical, extraDamage, target));
    //                 Debug.Log($"{transform.name} dealt extra {extraDamage} physical damage based on target's health!");
    //             }
    //             break;
    //         case AttackEffect.BounceAttack:
    //             T_Hunter hunterTrait = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.猎手] as T_Hunter;
    //             int bounceCount = hunterTrait.bounceTargetCount;
    //             float damageDecrease = hunterTrait.bounceDamageDecrease;
    //             // random select bounce targets
    //             List<B_Creature> possibleTargets = new List<B_Creature>();
    //             var opponents = lineUp == LineUp.Player ?
    //                 BattleWorldManager.Instance.GetInBattleCreatures(LineUp.Enemy) :
    //                 BattleWorldManager.Instance.GetInBattleCreatures(LineUp.Player);
    //             if (opponents.Count <= bounceCount)
    //             {
    //                 possibleTargets = opponents;
    //             }
    //             else
    //             {
    //                 List<int> l = new List<int>();
    //                 for (int i = 0; i < opponents.Count; i++) l.Add(i);
    //                 // Fisher-Yates shuffle (using UnityEngine.Random)
    //                 for (int i = l.Count - 1; i > 0; i--)
    //                 {
    //                     int j = Random.Range(0, i + 1);
    //                     (l[i], l[j]) = (l[j], l[i]);
    //                 }
    //                 // pick bounceCount distinct random targets
    //                 possibleTargets = new List<B_Creature>();
    //                 for (int i = 0; i < bounceCount && i < l.Count; i++)
    //                 {
    //                     possibleTargets.Add(opponents[l[i]]);
    //                 }
    //             }
    //
    //             float damageForBounce = damageList[0].damage * (1 - damageDecrease);
    //             foreach (var bounceTarget in possibleTargets)
    //             {
    //                 damageList.Add((curAttribute.normalAttackDamageType, damageForBounce, bounceTarget));
    //             }
    //             break;
    //         default:
    //             throw new System.NotImplementedException();
    //     }
    // });

    foreach (var (dt, d, t) in damageList) {
      // 跳过已被销毁的目标
      if (t == null) {
        continue;
      }

      t.TakeDamage(d, dt, isNormalAttack: true);
    }
  }

  public bool TakeDamage(float damage, DamageType damageType, bool isNormalAttack) {
    if (curAttribute.health <= 0) {
      Debug.LogWarning($"{transform.name} is already defeated and cannot take more damage.");
      return true;
    }

    // first check whether it can dodge
    if (isNormalAttack) {
      var dodgeChance = curAttribute.dodgeRate;
      var roll = Random.Range(minInclusive: 0f, maxInclusive: 1f);
      roll = Mathf.Round(Random.Range(minInclusive: 0f, maxInclusive: 1f) * 100f) / 100f;

      if (roll <= dodgeChance) {
        DamageTextPool.Instance?.ShowDamageText(transform.position, "闪避!", DamageText.PresetColors.Dodge);
        return false;
      }
    }

    // if it cannot dodge, take damage
    var actualDamage = damage;
    switch (damageType) {
      case DamageType.Physical:
        actualDamage = damage * (1 - curAttribute.armor / 100f);
        break;
      case DamageType.Spell:
        actualDamage = damage * (1 - curAttribute.spellResistance / 100f);
        break;
      case DamageType.TrueDamage:
        break;
      default:
        break;
    }

    // Take damage
    actualDamage = Mathf.Max(actualDamage, b: 0);
    curAttribute.health -= actualDamage;
    curAttribute.health = Mathf.Round(curAttribute.health * 100f) / 100f;
    Debug.Log($"{transform.name} actual damage taken: {actualDamage}, health after: {curAttribute.health}");

    // Show damage text
    DamageTextPool.Instance?.ShowDamageText(transform.position, actualDamage,
        DamageText.PresetColors.GetDamageColor(damageType),
        BattleWorldManager.Instance?.DraggingSlot);

    if (curAttribute.health <= 0) {
      Debug.Log($"{transform.name} has been defeated!");
      // Handle death (e.g., remove from battle world)
      // BattleWorldManager.Instance.RemoveObj(this);
    }

    return true;
  }

  private System.Collections.IEnumerator MoveAnimation(HexNode targetNode, float duration) {
    var startPos = transform.position;
    var endPos = targetNode.transform.position;
    var elapsed = 0f;

    while (elapsed < duration) {
      transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
      elapsed += Time.deltaTime;
      yield return null;
    }

    HexNodeManager.MoveObject(this, hexNode, targetNode);
  }
  
  # region DragAndDrop
  public void OnDrag(PointerEventData eventData) {
    // 只有在真正开始拖拽（OnBeginDrag 允许）时才响应 OnDrag
    if (!isDragging) {
      return;
    }

    transform.position = Input.mousePosition;
  }

  public void OnBeginDrag(PointerEventData eventData) {
    // 如果不允许拖拽则直接返回并确保标记为未拖拽
    if (lineUp == LineUp.Enemy && !BattleWorldManager.Instance.canDragEnemy) {
      isDragging = false;
      return;
    }

    if (BattleWorldManager.Instance.InBattle) {
      isDragging = false;
      BattleWorldManager.Instance.InstantiateLog("战斗中不能挪动物体!", TooltipText.TooltipMode.Warning);
      return;
    }

    m_image.raycastTarget = false;
    m_oriHexNode = hexNode;
    HexNodeManager.MoveObject(this, hexNode, to: null);
    transform.SetParent(BattleWorldManager.Instance.DraggingSlot);
    displayCard.SetOnlyDisplayIllustration(value: false);
    isDragging = true;
  }

  public void OnEndDrag(PointerEventData eventData) {
    // 如果没有处于拖拽状态则不做处理
    if (!isDragging) {
      return;
    }

    isDragging = false;

    bool IsPointerOverRect(RectTransform rect) {
      if (rect == null) {
        return false;
      }

      var canvas = rect.GetComponentInParent<Canvas>();
      var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
      return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
    }

    var successPut = false;
    if (eventData.pointerCurrentRaycast.gameObject != null) {
      bool inBack(AxialCoordinate coord) {
        if (lineUp == LineUp.Player) {
          return coord.R >= 4;
        }

        return true;
      }

      var hitObj = eventData.pointerCurrentRaycast.gameObject;
      if (hitObj.TryGetComponent<HexNode>(out var node) && node.walkable && inBack(node.coord)) {
        Debug.Log("Dropped on HexNode");
        successPut = true;
        HexNodeManager.MoveObject(this, hexNode, node);
      }

      if (IsPointerOverRect(BattleWorldManager.Instance.CreatureScrollView)) {
        Debug.Log("Dropped on Preparation Area");
        successPut = true;
        transform.SetParent(BattleWorldManager.Instance.PreparationAreaContent.transform);
        HexNodeManager.MoveObject(this, hexNode, to: null);
      }
    }

    if (!successPut) {
      if (m_oriHexNode != null) {
        HexNodeManager.MoveObject(this, hexNode, m_oriHexNode);
      }
      else {
        transform.SetParent(BattleWorldManager.Instance.PreparationAreaContent.transform);
      }
    }

    displayCard.SetOnlyDisplayIllustration(value: inBattle);
    m_image.raycastTarget = true;
  }

  public void OnPointerClick(PointerEventData eventData) {
    BattleWorldManager.Instance.OnCardClicked(this);
  }
  #endregion
}