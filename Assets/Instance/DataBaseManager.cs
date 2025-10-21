using System.Collections.Generic;
using UnityEngine;
using Category.Production;
using Category;

public class DataBaseManager : MonoBehaviour
{
    public static DataBaseManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 合成表管理
    [Header("合成表管理")]
    public CraftTableDB craftTableDB;
    public CraftTableDB.Recipe? GetRecipe(int id) => craftTableDB.GetRecipe(id);
    public CraftTableDB.Recipe? GetRecipe(string name) => craftTableDB.GetRecipe(name);
    public (List<Card>, CraftTableDB.Recipe)? GetRecipe(List<Card> inputCards) => craftTableDB.GetRecipe(inputCards);
    #endregion

    #region 卡牌属性管理
    [Header("卡牌属性管理")]
    public CardAttributeDB cardAttributeDB;
    // public ResourceCardClassification GetResourceCardClassification(ResourceCardType resourceCardType)
    //     => cardAttributeDB.GetResourceCardClassification(resourceCardType);
    // public WorkEfficiencyType GetWorkEfficiencyType(CreatureCardType creatureCardType)
    //     => cardAttributeDB.GetWorkEfficiencyType(creatureCardType);
    public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType)
        => cardAttributeDB.GetWorkEfficiencyValue(workEfficiencyType);
    // public float GetWorkEfficiencyValue(CreatureCardType creatureCardType)
    //     => cardAttributeDB.GetWorkEfficiencyValue(creatureCardType);
    // public bool IsResourcePoint(ResourceCardType resourceCardType)
    //     => cardAttributeDB.IsResourcePoint(resourceCardType);
    // public int GetDurability(ResourceCardType cardType)
    //     => cardAttributeDB.GetDurability(cardType);
    // public int GetSatietyValue(ResourceCardType cardType)
    //     => cardAttributeDB.GetSatietyValue(cardType);

    public T GetCardAttribute<T>(Card.CardDescription cardDescription) where T : struct
        => cardAttributeDB.GetCardAttribute<T>(cardDescription);

    #endregion

    #region 事件卡UI管理
    public EventCardUIDB eventCardUIDB;
    public bool TryGetEventCardUIPrefab(EventCardType eventCardType, out GameObject prefab)
        => eventCardUIDB.TryGetEventCardUIPrefab(eventCardType, out prefab);

    #endregion

}