using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreatureAttributeDisplay : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static GameObject preDisplayPanel;
    public TraitPanel traitPanel;
    public Button exitBtn;
    public Text nameText; // 生物名称
    public Text EXPText;  // 当前经验值
    public Text levelText;  // 等级
    public Text healthText; // 生命值
    public Text attackPowerText; // 攻击力
    public Text spellPowerText; // 法术强度
    public Text normalAttackDamageTypeText; // 普通攻击伤害类型
    public Text armorText; // 护甲值
    public Text spellResistanceText; // 魔法抗性
    public Text moveSpeedText; // 移动速度
    public Text dodgeRateText; // 闪避率
    public Text attackSpeedText; // 攻击速度
    public Text attackRangeText; // 攻击距离
    public Image illustrationImage; // 立绘
    public List<Button> traitsImages; // 特性图标列表

    private Canvas _canvas;
    private Vector2 _pointerOffset;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    public static void ClearAllDisplays()
    {
        if (preDisplayPanel != null)
        {
            Destroy(preDisplayPanel);
            preDisplayPanel = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos - (Vector3)_pointerOffset;
    }

    public void Start()
    {
        exitBtn.onClick.AddListener(() => Destroy(this.gameObject));
    }

    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         RectTransform rt = transform as RectTransform;
    //         Camera cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
    //         if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam))
    //         {
    //             Destroy(this.gameObject);
    //         }
    //     }
    // }

    public void UpdateAttributes(CardAttributeDB.CreatureCardAttribute creatureAttribute, CardAttributeDB.CreatureCardAttribute.BasicAttributes basicAttributes = null)
    {
        if (preDisplayPanel != null)
        {
            Destroy(preDisplayPanel);
        }
        preDisplayPanel = this.gameObject;

        // Set position to center
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform rt = transform as RectTransform;
        if (canvas != null && rt != null)
        {
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                cam,
                out Vector2 localPoint
            );
            rt.anchoredPosition = localPoint;
        }
        else
        {
            // fallback for non-UI or missing canvas
            Vector3 mousePos = Input.mousePosition;
            transform.position = mousePos;
        }

        // Set the attributes display
        if (basicAttributes == null)
        {
            basicAttributes = creatureAttribute.basicAttributes;
        }
        nameText.text = creatureAttribute.creatureCardType.ToString();
        EXPText.text = creatureAttribute.levelUpExpNeeded < 500 ?
            $"{basicAttributes.EXP} / {creatureAttribute.levelUpExpNeeded}" :
            "∞";
        levelText.text = basicAttributes.level.ToString();
        healthText.text = basicAttributes.health.ToString(format: "F2");
        attackPowerText.text = basicAttributes.attackPower.ToString();
        spellPowerText.text = basicAttributes.spellPower.ToString();
        normalAttackDamageTypeText.text = basicAttributes.normalAttackDamageType switch
        {
            Category.Battle.DamageType.Physical => "物理",
            Category.Battle.DamageType.Spell => "魔法",
            Category.Battle.DamageType.TrueDamage => "真实伤害",
            _ => throw new System.NotImplementedException(),
        };
        armorText.text = basicAttributes.armor.ToString();
        spellResistanceText.text = basicAttributes.spellResistance.ToString();
        moveSpeedText.text = basicAttributes.moveSpeed.ToString();
        dodgeRateText.text = basicAttributes.dodgeRate.ToString();
        attackSpeedText.text = basicAttributes.attackSpeed.ToString();
        attackRangeText.text = basicAttributes.attackRange.ToString();

        // Set the illustration image
        if (DataBaseManager.Instance.TryGetCardIllustration(creatureAttribute.creatureCardType, out var illustration))
        {
            illustrationImage.sprite = illustration.illustration;
        }
        else
        {
            illustrationImage.gameObject.SetActive(false);
        }

        // Set the traits images
        var spriteDict = DataBaseManager.Instance.GetAllTraitAttributes();
        for (int i = 0; i < traitsImages.Count; i++) 
        {
            if (i < basicAttributes.traits.Count)
            {
                traitsImages[i].gameObject.SetActive(true);

                var traitKey = basicAttributes.traits[i];
                if (spriteDict.TryGetValue(traitKey, out var traitAttr))
                    traitsImages[i].image.sprite = traitAttr.icon;
                else
                    traitsImages[i].image.sprite = null;

                traitsImages[i].onClick.RemoveAllListeners();
                int traitIndex = i;
                var capturedTrait = basicAttributes.traits[traitIndex];
                traitsImages[i].onClick.AddListener(() =>
                {
                    traitPanel.InitializeNormal(capturedTrait, _canvas);
                });
            }
            else
            {
                traitsImages[i].gameObject.SetActive(false);
                traitsImages[i].image.sprite = null;
                traitsImages[i].onClick.RemoveAllListeners();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        _pointerOffset = mousePos - new Vector3(transform.position.x, transform.position.y);
    }
}
