using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Category.Battle;
using System.Collections.Generic;
using static CardAttributeDB.CreatureCardAttribute;

public class B_Creature : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 组件
    private Image image;
    public Text nameText;

    // 引用
    public HexNode hexNode;
    public bool inBattle => hexNode != null;
    public Transform equiptmentSlot;

    // 属性
    public long cardID;
    // private readonly bool OnBattle => hexNode != null;
    public int cd;
    public CardAttributeDB.CreatureCardAttribute creatureCardAttribute; // readonly attribute from CardAttributeDB
    public BasicAttributes curAttribute; // the attr that apply before traits and buffs
                                         // will be replaced with actActtribute when the game is running 
    private BasicAttributes _actAttribute;
    public BasicAttributes actAttribute // the attr that apply after equipment and buffs
    {
        get
        {
            if (!BattleWorldManager.Instance.InBattle)
            {
                BasicAttributes totalAttr = (BasicAttributes)curAttribute.Clone();
                if (equipment != null)
                {
                    CardAttributeDB.EquipmentCardAttribute.EquipmentBasicAttributesBonus equipmentCardAttribute = equipment.equipmentAttribute.basicAttributesBonus;
                    totalAttr.health += equipmentCardAttribute.health;
                    totalAttr.attackPower += equipmentCardAttribute.attackPower;
                    totalAttr.spellPower += equipmentCardAttribute.spellPower;
                    totalAttr.armor += equipmentCardAttribute.armor;
                    totalAttr.spellResistance += equipmentCardAttribute.spellResistance;
                    totalAttr.moveSpeed += equipmentCardAttribute.moveSpeed;
                    totalAttr.dodgeRate += equipmentCardAttribute.dodgeRate;
                    totalAttr.attackSpeed += equipmentCardAttribute.attackSpeed;
                    totalAttr.attackRange += equipmentCardAttribute.attackRange;
                }
                foreach ((Trait trait, B_Trait traitObj) in BattleWorldManager.Instance.GetTraitObjDict(lineUp))
                {
                    if (traitObj.currentTraitCreatureCount > 0 && traitObj is ITraitHolder traitHolder)
                    {
                        Debug.Log($"Modifying attributes for trait {trait}");
                        traitHolder.ModifyAttributes(totalAttr, this);
                    }
                }
                _actAttribute = totalAttr;
            }
            if (_actAttribute == null)
            {
                Debug.LogError($"actAttribute is null for {transform.name}");
            }
            return _actAttribute;
        }
        set
        {
            _actAttribute = value;
        }
    }
    public LineUp lineUp;
    public List<AttackEffetct> attackEffetcts = new List<AttackEffetct>();
    public B_Equipment equipment;

    private Vector2 oriPosition;
    private GameObject oriParent;
    private bool isDragging = false;
    
    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Init(long cardID, LineUp lineUp)
    {
        this.cardID = cardID;
        this.lineUp = lineUp;
        var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
        curAttribute = (BasicAttributes)attr.basicAttributes.Clone();
        _actAttribute = (BasicAttributes)curAttribute.Clone();

        nameText.text = attr.creatureCardType.ToString();
        // Debug.Log("Attack Range: " + attr.basicAttributes.attackRange + " Basic is " + curAttribute.attackRange);
    }

    public void Tick()
    {
        var opponents = lineUp == LineUp.Player ?
            BattleWorldManager.Instance.enemyCreatures :
            BattleWorldManager.Instance.playerCreatures;
        B_Creature closestOpponents = null;
        int closestDistance = int.MaxValue;
        foreach (var enemy in opponents)
        {
            int distance = hexNode.GetDistance(enemy.hexNode);
            if (distance < closestDistance)
            {
                closestOpponents = enemy;
                closestDistance = distance;
            }
        }
        cd = closestDistance;

        if (closestOpponents == null)
            return;

        if (closestDistance <= curAttribute.attackRange)
        {
            // Check whether there is any enemy in attack range and decrease curAttr's attack speed
            // if already to attack, add DamageActions to BattleWorldManager
            if (curAttribute.attackSpeed == 0)
            {
                BattleWorldManager.Instance.DamageActions += () => Attack(closestOpponents);
                curAttribute.attackSpeed = actAttribute.attackSpeed;
            }
            else
            {
                curAttribute.attackSpeed -= 1;
            }
        }
        else
        {
            // Find the closest enemy and decrease the move speed 
            // if already to move, find path and add NormalActions to BattleWorldManager
            if (curAttribute.moveSpeed == 0)
            {
                var path = Pathfinding.FindPath(hexNode, closestOpponents.hexNode);
                if (path != null && path.Count > 1)
                {
                    HexNodeManager.ReserveObject(this, path[1]);
                    BattleWorldManager.Instance.NormalActions += () => MoveTo(path[1]);
                }
                curAttribute.moveSpeed = actAttribute.moveSpeed;
            }
            else
            {
                curAttribute.moveSpeed -= 1;
            }
        }
    }

    public void Attack(B_Creature target)
    {
        float damage = curAttribute.attackPower;
        DamageType damageType = curAttribute.normalAttackDamageType;
        List<(DamageType damageType, float damage, B_Creature target)> damageList = new List<(DamageType damageType, float damage, B_Creature target)>
        {
            (damageType, damage, target)
        };
        attackEffetcts.ForEach(effect =>
        {
            switch (effect)
            {
                case AttackEffetct.TrueDamagePercentageOfAttackPower:
                    float percentage = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.部落] is T_Tribe tribeTrait ? tribeTrait.TrueDamagePercentageOfAttackPower : 0f;
                    float trueDamage = damageList[0].damage * percentage;
                    trueDamage = Mathf.Round(trueDamage * 100f) / 100f;
                    damageList.Add((DamageType.TrueDamage, trueDamage, target));
                    break;
                case AttackEffetct.ProbabilityDoubleDamage:
                    float probability = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.刺客] is T_Assassin assassinTrait ? assassinTrait.doubleDamageProbability : 0f;
                    float roll = Random.Range(0f, 1f);
                    roll = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;
                    if (roll <= probability)
                    {
                        damageList[0] = (damageList[0].damageType, damageList[0].damage * 2, damageList[0].target);
                        Debug.Log($"{transform.name} triggered double damage!");
                    }
                    break;
                case AttackEffetct.PhysicalDamagePercentageOftargetHealth:
                    if (damageType == DamageType.Physical)
                    {
                        float healthPercentage = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.破阵者] is T_Destroyer destroyerTrait ? destroyerTrait.percentageOftargetHealth : 0f;
                        float extraDamage = target.actAttribute.health * healthPercentage;
                        extraDamage = Mathf.Round(extraDamage * 100f) / 100f;
                        damageList.Add((DamageType.Physical, extraDamage, target));
                        Debug.Log($"{transform.name} dealt extra {extraDamage} physical damage based on target's health!");
                    }
                    break;
                case AttackEffetct.BounceAttack:
                    T_Hunter hunterTrait = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[Trait.猎手] as T_Hunter;
                    int bounceCount = hunterTrait.bounceTargetCount;
                    float damageDecrease = hunterTrait.bounceDamageDecrease;
                    // random select bounce targets
                    List<B_Creature> possibleTargets = new List<B_Creature>();
                    var opponents = lineUp == LineUp.Player ?
                        BattleWorldManager.Instance.GetInBattleCreatures(LineUp.Enemy) :
                        BattleWorldManager.Instance.GetInBattleCreatures(LineUp.Player);
                    if (opponents.Count <= bounceCount)
                    {
                        possibleTargets = opponents;
                    }
                    else
                    {
                        List<int> l = new List<int>();
                        for (int i = 0; i < opponents.Count; i++) l.Add(i);
                        // Fisher-Yates shuffle (using UnityEngine.Random)
                        for (int i = l.Count - 1; i > 0; i--)
                        {
                            int j = Random.Range(0, i + 1);
                            int tmp = l[i];
                            l[i] = l[j];
                            l[j] = tmp;
                        }
                        // pick bounceCount distinct random targets
                        possibleTargets = new List<B_Creature>();
                        for (int i = 0; i < bounceCount && i < l.Count; i++)
                        {
                            possibleTargets.Add(opponents[l[i]]);
                        }
                    }

                    float damageForBounce = damageList[0].damage * (1 - damageDecrease);
                    foreach (var bounceTarget in possibleTargets)
                    {
                        damageList.Add((curAttribute.normalAttackDamageType, damageForBounce, bounceTarget));
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        });

        foreach (var (dt, d, t) in damageList)
        {
            t.TakeDamage(d, dt, true);
        }
    }

    public bool TakeDamage(float damage, DamageType damageType, bool isNormalAttack)
    {
        // first check whether can dodge
        if (isNormalAttack)
        {
            float dodgeChance = curAttribute.dodgeRate;
            float roll = Random.Range(0f, 1f);
            roll = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;

            if (roll <= dodgeChance)
            {
                Debug.Log($"{transform.name} dodged the attack!");
                return false;
            }
        }

        // if cannot dodge, take damage
        float actualDamage = damage;
        switch (damageType)
        {
            case Category.Battle.DamageType.Physical:
                actualDamage = damage * (1 - curAttribute.armor / 100f);
                break;
            case Category.Battle.DamageType.Spell:
                actualDamage = damage * (1 - curAttribute.spellResistance / 100f);
                break;
            case Category.Battle.DamageType.TrueDamage:
                break;
            default:
                break;
        }

        // Take damage
        actualDamage = Mathf.Max(actualDamage, 0);
        curAttribute.health -= actualDamage;
        curAttribute.health = Mathf.Round(curAttribute.health * 100f) / 100f;
        Debug.Log($"{transform.name} actual damage taken: {actualDamage}, health after: {curAttribute.health}");

        if (curAttribute.health <= 0)
        {
            Debug.Log($"{transform.name} has been defeated!");
            // Handle death (e.g., remove from battle world)
            BattleWorldManager.Instance.RemoveObj(this);
        }

        return true;
    }

    public void MoveTo(HexNode targetNode)
    {
        HexNodeManager.MoveObject(this, hexNode, targetNode);
    }

    public void RemoveEquipment()
    {
        equipment = null;
    }

    public void Equip(B_Equipment equipment)
    {
        this.equipment = equipment;
    }

    # region DragAndDrop
    public void OnDrag(PointerEventData eventData)
    {
        // 只有在真正开始拖拽（OnBeginDrag 允许）时才响应 OnDrag
        if (!isDragging) return;
        transform.position = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 如果不允许拖拽则直接返回并确保标记为未拖拽
        if (lineUp == LineUp.Enemy && !BattleWorldManager.Instance.canDragEnemy)
        {
            isDragging = false;
            return;
        }
        if (BattleWorldManager.Instance.InBattle)
        {
            isDragging = false;
            BattleWorldManager.Instance.InstantiateLog("战斗中不能挪动物体!", TooltipText.TooltipMode.Warning);
            return;
        }
        image.raycastTarget = false;
        oriPosition = transform.position;
        oriParent = transform.parent.gameObject;
        transform.SetParent(BattleWorldManager.Instance.DraggingSlot, false);
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 如果没有处于拖拽状态则不做处理
        if (!isDragging) return;
        isDragging = false;
        bool IsPointerOverRect(RectTransform rect)
        {
            if (rect == null) return false;
            var canvas = rect.GetComponentInParent<Canvas>();
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
        }
        
        bool succPut = false;
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hitObj = eventData.pointerCurrentRaycast.gameObject;
            if (hitObj.TryGetComponent<HexNode>(out var node) && node.walkable)
            {
                Debug.Log("Dropped on HexNode");
                succPut = true;
                HexNodeManager.MoveObject(this, hexNode, node);
            }

            if (IsPointerOverRect(BattleWorldManager.Instance.CreatureScrollView))
            {
                Debug.Log("Dropped on Preparation Area");
                succPut = true;
                transform.SetParent(BattleWorldManager.Instance.PreparationAreaContent.transform, false);
                HexNodeManager.MoveObject(this, hexNode, null);
            }
        }

        if (!succPut)
        {
            transform.SetParent(oriParent.transform, false);
            transform.position = oriPosition;
        }
        
        BattleWorldManager.Instance.UpdateActiveTraits(lineUp);
        image.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleWorldManager.Instance.OnCardClicked(this);
    }
    #endregion
}