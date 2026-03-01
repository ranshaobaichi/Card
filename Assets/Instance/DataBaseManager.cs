using System.Collections.Generic;
using UnityEngine;
using Category.Production;
using Category;
using System.Collections.ObjectModel;

public class DataBaseManager : MonoBehaviour {
  public static DataBaseManager Instance;

  private void Awake() {
    if (Instance == null) {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else {
      Destroy(gameObject);
    }

    cardAttributeDB.InitializeCardAttributeDict();
    eventCardUIDB.InitializeEventUIDict();
    cardIconsDB.InitializeCardIconDict();
  }

  #region 合成表管理
  [Header("合成表管理")] public CraftTableDB craftTableDB;

  public CraftTableDB.Recipe? GetRecipe(int id) {
    return craftTableDB.GetRecipe(id);
  }

  public CraftTableDB.Recipe? GetRecipe(string name) {
    return craftTableDB.GetRecipe(name);
  }

  public (List<Card>, CraftTableDB.Recipe)? GetRecipe(List<Card> inputCards) {
    return craftTableDB.GetRecipe(inputCards);
  }

  public List<CraftTableDB.Recipe> GetRecipes(List<Card> inputCards, List<CraftTableDB.Recipe> fromList = null) {
    return craftTableDB.GetRecipes(inputCards, fromList);
  }

  public List<CraftTableDB.Recipe> GetAllRecipes() {
    return craftTableDB.recipeList;
  }
  #endregion

  #region 卡牌属性管理
  [Header("卡牌属性管理")] public CardAttributeDB cardAttributeDB;

  // public ResourceCardClassification GetResourceCardClassification(ResourceCardType resourceCardType)
  //     => cardAttributeDB.GetResourceCardClassification(resourceCardType);
  // public WorkEfficiencyType GetWorkEfficiencyType(CreatureCardType creatureCardType)
  //     => cardAttributeDB.GetWorkEfficiencyType(creatureCardType);
  public float GetWorkEfficiencyValue(WorkEfficiencyType workEfficiencyType) {
    return cardAttributeDB.GetWorkEfficiencyValue(workEfficiencyType);
  }

  // public float GetWorkEfficiencyValue(CreatureCardType creatureCardType)
  //     => cardAttributeDB.GetWorkEfficiencyValue(creatureCardType);
  // public bool IsResourcePoint(ResourceCardType resourceCardType)
  //     => cardAttributeDB.IsResourcePoint(resourceCardType);
  // public int GetDurability(ResourceCardType cardType)
  //     => cardAttributeDB.GetDurability(cardType);
  // public int GetSatietyValue(ResourceCardType cardType)
  //     => cardAttributeDB.GetSatietyValue(cardType);
  public T GetCardAttribute<T>(Card.CardDescription cardDescription) where T : class {
    return cardAttributeDB.GetCardAttribute<T>(cardDescription);
  }
  #endregion

  #region 事件卡UI管理
  public EventUIDB eventCardUIDB;

  public ReadOnlyDictionary<EventCardType, EventUIDB.EventUIAttribute> GetAllEventUIAttributes() {
    return eventCardUIDB.GetEventCardUIDict();
  }

  public EventUIDB.EventUIAttribute? GetEventUIAttribute(EventCardType eventCardType) {
    return eventCardUIDB.GetEventUIAttribute(eventCardType);
  }
  #endregion

  #region 卡牌图标管理
  public CardIconsDB cardIconsDB;

  public bool TryGetCardIconAttribute(CardType cardType, out CardIconsDB.CardIconAttribute attribute,
      ResourceCardClassification resourceCardClassification = ResourceCardClassification.None) {
    return cardIconsDB.TryGetCardIconAttribute(cardType, out attribute, resourceCardClassification);
  }

  public bool TryGetCardIllustration(CreatureCardType cardDescription, out CardIconsDB.CardIllustration illustration) {
    return cardIconsDB.TryGetCardIllustration(cardDescription, out illustration);
  }

  public bool TryGetResourcesCardIcon(ResourceCardType resourceCardType,
      out CardIconsDB.ResourcesCardIcons resourceIcon) {
    return cardIconsDB.TryGetResourcesCardIcon(resourceCardType, out resourceIcon);
  }
  #endregion
}