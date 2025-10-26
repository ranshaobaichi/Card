using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using static CardAttributeDB.CreatureCardAttribute;
using Category.Battle;

public class B_Creature : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // 组件
    private Image image;

    // 引用
    public HexNode hexNode;

    // 属性
    public long cardID;
    // private readonly bool OnBattle => hexNode != null;
    public int cd;
    [HideInInspector] public CardAttributeDB.CreatureCardAttribute creatureAttribute;
    public BasicAttributes curAttribute;
    public LineUp lineUp;

    private Vector2 oriPosition;
    private GameObject oriParent;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Init(long cardID, LineUp lineUp)
    {
        this.cardID = cardID;
        this.lineUp = lineUp;
        var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
        creatureAttribute = attr;
        curAttribute = (BasicAttributes)creatureAttribute.basicAttributes.Clone();
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
                curAttribute.attackSpeed = creatureAttribute.basicAttributes.attackSpeed;
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
                curAttribute.moveSpeed = creatureAttribute.basicAttributes.moveSpeed;
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
        Category.Battle.DamageType damageType = curAttribute.normalAttackDamageType;
        Debug.Log($"{transform.name} attacks {target.transform.name} for {damage} damage of type {damageType}");
        if (target.TakeDamage(damage, damageType, true))
        {
            // Add attack effects here
        }
    }

    public bool TakeDamage(float damage, Category.Battle.DamageType damageType, bool isNormalAttack)
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
        curAttribute.health = Mathf.FloorToInt(curAttribute.health);
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

    public void GainEXP(int exp)
    {
        creatureAttribute.basicAttributes.EXP += exp;
        int experience = (1 + creatureAttribute.basicAttributes.level) * creatureAttribute.levelUpExpIncreasePercent;
        if (creatureAttribute.basicAttributes.EXP >= experience)
        {
            creatureAttribute.basicAttributes.level += 1;
            creatureAttribute.basicAttributes.EXP -= experience;
            // Increase other attributes on level up
            creatureAttribute.basicAttributes.satiety += creatureAttribute.levelUpAttributes.satietyGrowth;
            creatureAttribute.basicAttributes.health += creatureAttribute.levelUpAttributes.healthGrowth;
            creatureAttribute.basicAttributes.attackPower += creatureAttribute.levelUpAttributes.attackPowerGrowth;
            creatureAttribute.basicAttributes.spellPower += creatureAttribute.levelUpAttributes.spellPowerGrowth;
            creatureAttribute.basicAttributes.armor += creatureAttribute.levelUpAttributes.armorGrowth;
            creatureAttribute.basicAttributes.spellResistance += creatureAttribute.levelUpAttributes.spellResistanceGrowth;
            creatureAttribute.basicAttributes.moveSpeed -= creatureAttribute.levelUpAttributes.moveSpeedGrowth;
            creatureAttribute.basicAttributes.dodgeRate += creatureAttribute.levelUpAttributes.dodgeRateGrowth;
            creatureAttribute.basicAttributes.attackSpeed -= creatureAttribute.levelUpAttributes.attackSpeedGrowth;
            creatureAttribute.basicAttributes.attackRange += creatureAttribute.levelUpAttributes.attackRangeGrowth;
            Debug.Log($"{transform.name} leveled up to level {creatureAttribute.basicAttributes.level}!");
        }
    }

    # region DragAndDrop
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        oriPosition = transform.position;
        oriParent = transform.parent.gameObject;
        transform.SetParent(BattleWorldManager.Instance.DraggingSlot, false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
            }
        }
        
        if (!succPut)
        {
            transform.SetParent(oriParent.transform, false);
            transform.position = oriPosition;
        }
        image.raycastTarget = true;
    }
    # endregion
}