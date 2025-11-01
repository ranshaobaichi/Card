using System.Collections.Generic;
using Category;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardAttributeDB;

public class B_Equipment : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image image;
    [Header("卡牌图像列表 - 顺序为：类型、背景、顶部装饰、立绘、底部装饰、侧边装饰")]
    public List<Image> cardImages = new List<Image>();
    public Text nameText;
    public EquipmentCardAttribute equipmentAttribute;
    public GameObject equipmentSlot;
    public B_Creature ownerCreature;
    private B_Creature oriCreature;
    public long cardID;

    public void Init(long cardID, GameObject equipmentSlot)
    {
        this.equipmentSlot = equipmentSlot;
        var attr = CardManager.Instance.GetCardAttribute<EquipmentCardAttribute>(cardID);
        equipmentAttribute = attr;

        // Set the card images
        var succ = CardManager.Instance.TryGetCardIconAttribute(CardType.Resources, out var cardIconAttrribute, ResourceCardClassification.Equipment);
        if (succ)
        {
            nameText.text = equipmentAttribute.equipmentCardType.ToString();
            cardImages[0].sprite = cardIconAttrribute.type;
            cardImages[1].sprite = cardIconAttrribute.background;
            cardImages[2].sprite = cardIconAttrribute.top;
            cardImages[3].sprite = cardIconAttrribute.illustration;
            cardImages[4].sprite = cardIconAttrribute.bottom;
            cardImages[5].sprite = cardIconAttrribute.side;
        }
        else
        {
            Debug.LogError($"Card icon attribute not found for card ID {cardID} of type {CardType.Creatures}.");
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(BattleWorldManager.Instance.DraggingSlot);
        image.raycastTarget = false;

        if (ownerCreature != null)
        {
            oriCreature = ownerCreature;
            ownerCreature.RemoveEquipment();
            ownerCreature = null;
        }
        if (equipmentSlot != null)
        {
            Destroy(equipmentSlot);
            equipmentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool hitObj = eventData.pointerCurrentRaycast.gameObject != null;
        if (oriCreature != null)
        {
            if (hitObj)
            {
                // if put on another creature
                if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent<B_Creature>(out var creature) 
                    && creature != ownerCreature 
                    && creature.lineUp == Category.Battle.LineUp.Player)
                {
                    creature.Equip(this);
                    ownerCreature = creature;
                    transform.SetParent(creature.equiptmentSlot.transform, false);
                }
                // if put back to equipment area
                else if (eventData.pointerCurrentRaycast.gameObject.CompareTag("EquipmentArea"))
                {
                    oriCreature.RemoveEquipment();
                    BattleWorldManager.Instance.AddBattleEquipment(this);
                }
                else
                {
                    Debug.LogError($"Dropped on {eventData.pointerCurrentRaycast.gameObject.name} with ownerCreature not null but not on creature or equipment area, return to original creature");
                    oriCreature.Equip(this);
                    ownerCreature = oriCreature;
                    transform.SetParent(oriCreature.equiptmentSlot.transform, false);
                }
            }
            // else return to original creature
            else
            {
                oriCreature.Equip(this);
                ownerCreature = oriCreature;
                transform.SetParent(oriCreature.equiptmentSlot.transform, false);
            }
        }
        // not equipped
        else
        {
            if (hitObj)
            {
                Debug.Log($"Dropped on {eventData.pointerCurrentRaycast.gameObject.name}");
            }
            // if put on a creature
            if (hitObj && eventData.pointerCurrentRaycast.gameObject.TryGetComponent<B_Creature>(out var creature))
            {
                creature.Equip(this);
                ownerCreature = creature;
                transform.SetParent(creature.equiptmentSlot.transform, false);
            }
            // else put back to equipment area
            else
            {
                BattleWorldManager.Instance.AddBattleEquipment(this);
            }
        }

        oriCreature = null;
        image.raycastTarget = true;
    }
}