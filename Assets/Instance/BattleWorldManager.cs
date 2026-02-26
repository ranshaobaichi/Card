using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Category;
using Category.Battle;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EnemyWaveData {
  public List<CreatureCardType> creatureType;
  public List<AxialCoordinate> spawnCoord;
  public int totalExpGain;
}

public class BattleWorldManager : MonoBehaviour {
  public static BattleWorldManager Instance;
  public static readonly string EnemyWavesResourcePath = "EnemyWaves/";
  public static readonly string EnemyWavesResourceName = "EnemyWave";
  [HideInInspector] public bool InBattle = false;
  public bool mannualTickControl = false;
  public bool canDragEnemy = false;
  public float TickInterval = 1.0f;

  public EnemyWaveData currentWaveData;

  // private List<CreatureCardType> testAddEnemyTypes = new List<CreatureCardType>();
  private static bool showFirstTimeBattleTutorial = true;

  [Header("Battle World Prefabs")] public GameObject BattleCreaturePrefab;
  public GameObject EquipmentSlotPrefab;
  public GameObject EquipmentPrefab;
  public GameObject tooltipPrefab;
  public GameObject attributeDisplayPrefab;
  public GameObject traitItemnPrefab;
  public GameObject rewardCardPrefab;

  [Header("Battle World Contents")] public GameObject PreparationAreaContent;
  public GameObject EquipmentAreaContent;
  public RectTransform CreatureScrollView;
  public Transform DraggingSlot;
  public GameObject PlayerTraitGameobject;
  public GameObject EnemyTraitGameobject;
  public GameObject BattleTutorialPanel;
  public GameObject FailedText;
  public Button StartBattleButton;
  [Header("Reward Panel")] public GameObject rewardPanel;
  public GameObject rewardCardPanel;
  public Button nextSceneBtn;
  [Header("Battle World References")] public List<B_Creature> playerCreatures = new List<B_Creature>();
  public List<B_Creature> enemyCreatures = new List<B_Creature>();
  public List<B_Equipment> equipments = new List<B_Equipment>();
  public List<B_Creature> InBattleCreatures => playerCreatures.FindAll(c => c.inBattle);
  public List<long> playerDeployedCreatureIDs = new List<long>();

  // Tick Events
  public event Action PlayerTick;
  public event Action EnemyTick;
  public event Action NormalActions;
  public event Action DamageActions;
  public event Action OnBattleStart;
  public event Action OnBattleEnd;
  public event Action<B_Creature> OnCreatureDead;

  #region TEST_FUNCTIONS_AND_DATA
  /// <summary>
  /// TEST FUNCTION, create a tmp battle creature which will not be saved
  /// </summary>
  /// <param name="lineUp"></param>
  /// <param name="testCreatureCardType"></param>
  public B_Creature AddObj(LineUp lineUp, CreatureCardType testCreatureCardType) {
    if (testCreatureCardType == CreatureCardType.None || testCreatureCardType == CreatureCardType.Any) {
      Debug.LogError($"BattleWorldManager: Invalid testCreatureCardType {testCreatureCardType}");
      return null;
    }

    Debug.Log($"BattleWorldManager: Adding test creature of type {testCreatureCardType} to {lineUp}");
    var creatureGO = Instantiate(BattleCreaturePrefab, PreparationAreaContent.transform.position, Quaternion.identity,
        PreparationAreaContent.transform);
    var creature = creatureGO.GetComponent<B_Creature>();
    var cardDescription = new Card.CardDescription {
        cardType = CardType.Creatures,
        creatureCardType = testCreatureCardType
    };
    var attr = DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardDescription);
    creature.creatureCardAttribute = attr;
    creature.curAttribute = (CardAttributeDB.CreatureCardAttribute.BasicAttributes)attr.basicAttributes.Clone();
    creature.actAttribute = (CardAttributeDB.CreatureCardAttribute.BasicAttributes)attr.basicAttributes.Clone();
    creature.lineUp = lineUp;
    creature.cardID = -1; // test creature has no valid cardID
    creature.displayCard.Initialize(cardDescription);

    if (lineUp == LineUp.Enemy) {
      enemyCreatures.Add(creature);
      // testAddEnemyTypes.Add(testCreatureCardType);
    }
    else {
      playerCreatures.Add(creature);
    }

    return creature;
  }

  public B_Equipment AddEquipment(ResourceCardType equipmentCardType) {
    if (!DataBaseManager.Instance.IsEquipmentCard(equipmentCardType)) {
      Debug.LogWarning($"BattleWorldManager: ResourceCardType {equipmentCardType} is not an equipment card.");
      return null;
    }

    var equipmentSlot = Instantiate(EquipmentSlotPrefab, EquipmentAreaContent.transform);
    var equipment = Instantiate(EquipmentPrefab, equipmentSlot.transform).GetComponent<B_Equipment>();
    var attr = DataBaseManager.Instance.GetEquipmentCardAttribute(equipmentCardType);
    equipment.equipmentAttribute = attr;
    equipment.cardID = -1;
    equipments.Add(equipment);
    equipment.equipmentSlot = equipmentSlot;
    return equipment;
  }
  #endregion

  private void Awake() {
    if (Instance == null) {
      Instance = this;
    }
    else {
      Destroy(gameObject);
    }

    nextSceneBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.ProductionScene));
  }

  private void UpdateSaveDataWaveIndex() {
    SaveDataManager.currentSaveData.currentWaveIndex = CardManager.Instance.currentWave;
  }

  private void Start() {
    SceneManager.BeforeSceneChanged += UpdateSaveDataWaveIndex;
  }

  private void OnDestroy() {
    SceneManager.BeforeSceneChanged -= UpdateSaveDataWaveIndex;
  }

  private void OnEnable() {
    // testAddEnemyTypes.Clear();

    // initialize battle objects from CardManager
    foreach (var id in CardManager.Instance.battleSceneCreatureCardIDs) {
      // Debug.Log($"BattleWorldManager adding battle object with card ID: {id}");
      AddBattleObject(id);
    }

    // initialize equipments from CardManager
    foreach (var (id, attr) in CardManager.Instance.GetResourceCardAttributes()) {
      if (attr.resourceClassification == ResourceCardClassification.Equipment) {
        AddBattleEquipment(id);
      }
    }

    // load current wave
    if (CardManager.Instance.currentWave >= 0) {
      Debug.Log($"BattleWorldManager: Loading wave index {CardManager.Instance.currentWave}");
      StartCoroutine(delayedLoadWave(frameCount: 3, CardManager.Instance.currentWave));
    }
    else {
      Debug.LogError($"BattleWorldManager: currentWaveIndex {CardManager.Instance.currentWave} is invalid.");
    }

    // show battle tutorial if first time
    if (showFirstTimeBattleTutorial) {
      BattleTutorialPanel.SetActive(value: true);
      showFirstTimeBattleTutorial = false;
    }

    StartBattleButton.onClick.AddListener(StartBattle);
  }

  private void Update() {
    if (InBattle && mannualTickControl && Input.GetKeyDown(KeyCode.Space)) {
      InvokeTick();
    }
  }

  private void StartBattle() {
    InBattle = true;
    Debug.Log("Battle Started");
    playerDeployedCreatureIDs.Clear();
    PlayerTick = null;
    EnemyTick = null;

    foreach (var creature in GetInBattleCreatures(LineUp.Player)) {
      playerDeployedCreatureIDs.Add(creature.cardID);
      // Debug.Log("player creature added to tick: " + creature.name);
      PlayerTick += creature.Tick;
    }

    foreach (var creature in enemyCreatures) {
      // Debug.Log("enemy creature added to tick: " + creature.name);
      EnemyTick += creature.Tick;
    }

    // make sure the OnBattleStart event is invoked before all creatures set their curAttribute
    // because some traits may modify the attributes at the start of battle
    OnBattleStart?.Invoke();

    foreach (var creature in playerCreatures) {
      creature.curAttribute.CopyFrom(creature.actAttribute);
    }

    foreach (var creature in enemyCreatures) {
      creature.curAttribute.CopyFrom(creature.actAttribute);
    }

    if (!mannualTickControl) {
      StartCoroutine(ReapeatTick());
    }

    StartBattleButton.interactable = false;
  }

  private IEnumerator ReapeatTick() {
    while (InBattle) {
      InvokeTick();
      yield return new WaitForSeconds(TickInterval);
    }
  }

  public void InstantiateLog(string logContent, TooltipText.TooltipMode mode = TooltipText.TooltipMode.Normal) {
    Instantiate(tooltipPrefab, GetComponentInParent<Canvas>().transform).GetComponent<TooltipText>()
        .SetTooltipText(logContent, mode);
  }

  # region BATTLEWORLD OBJS APIS
  public List<B_Creature> GetCreatures(LineUp lineUp) {
    return lineUp == LineUp.Player ? playerCreatures : enemyCreatures;
  }

  public List<B_Creature> GetInBattleCreatures(LineUp lineUp) {
    return lineUp == LineUp.Player ? InBattleCreatures : enemyCreatures;
  }

  /// <summary>
  /// Remove a creature from battle world, including from hex node and relevant lists
  /// Must use in battle
  /// </summary>
  /// <param name="creature"></param>
  public void RemoveObj(B_Creature creature) {
    if (!InBattle) {
      Debug.LogError("BattleWorldManager RemoveObj: Cannot remove creature when not in battle.");
      return;
    }

    if (creature.lineUp == LineUp.Player) {
      playerCreatures.Remove(creature);
      PlayerTick -= creature.Tick;
      // CardManager.Instance.RemoveCardAttribute(creature.cardID);
    }
    else {
      enemyCreatures.Remove(creature);
      EnemyTick -= creature.Tick;
    }

    var hexNode = creature.hexNode;
    if (hexNode != null) {
      HexNodeManager.MoveObject(creature, hexNode, to: null);
    }

    creature.StopAllCoroutines();
    Destroy(creature.gameObject);
  }

  public void AddBattleObject(long cardID) {
    var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
    var creatureGO = Instantiate(BattleCreaturePrefab, PreparationAreaContent.transform.position, Quaternion.identity,
        PreparationAreaContent.transform);

    var creature = creatureGO.GetComponent<B_Creature>();
    creature.creatureCardAttribute = attr;
    playerCreatures.Add(creature);

    creature.Init(cardID, LineUp.Player);
  }

  public B_Equipment AddBattleEquipment(long cardID) {
    var equipmentSlot = Instantiate(EquipmentSlotPrefab, EquipmentAreaContent.transform);
    var equipment = Instantiate(EquipmentPrefab, equipmentSlot.transform).GetComponent<B_Equipment>();
    equipment.Init(cardID, equipmentSlot);
    equipments.Add(equipment);
    return equipment;
  }

  /// <summary>
  /// Add a equipment to battle equipment area, which has been added to equipment list
  /// </summary>
  /// <param name="equipment"></param>
  public void AddBattleEquipment(B_Equipment equipment) {
    var equipmentSlot = Instantiate(EquipmentSlotPrefab, EquipmentAreaContent.transform);
    equipment.transform.SetParent(equipmentSlot.transform);
    equipment.transform.position = equipmentSlot.transform.position;
    equipment.equipmentSlot = equipmentSlot;
  }

  private IEnumerator delayedLoadWave(int frameCount, int waveIdx) {
    for (var i = 0; i < frameCount; i++) {
      yield return null;
    }

    LoadBattleWave(waveIdx);
  }

  public bool LoadBattleWave(int waveIdx) {
    var path = EnemyWavesResourcePath + EnemyWavesResourceName + '_' + waveIdx;
    var waveDataAsset = Resources.Load<TextAsset>(path);
    if (waveDataAsset == null) {
      Debug.LogError($"LoadBattleWave: 未找到敌人波次资源: {path}");
      return false;
    }

    var waveData = JsonUtility.FromJson<EnemyWaveData>(waveDataAsset.text);
    for (var i = 0; i < waveData.creatureType.Count; i++) {
      var creatureGO = AddObj(LineUp.Enemy, waveData.creatureType[i]);
      if (creatureGO == null) {
        Debug.LogError($"LoadBattleWave: 无法创建敌人波次中的生物对象，类型: {waveData.creatureType[i]}");
        continue;
      }

      HexNodeManager.MoveObject(creatureGO, from: null, HexNodeManager.Instance.Tiles[waveData.spawnCoord[i]]);
      creatureGO.displayCard.SetOnlyDisplayIllustration(value: true);
    }

    currentWaveData = waveData;
    return true;
  }

  public void SaveCurBattleWave(int waveIdx, int expGain) {
#if UNITY_EDITOR
    var waveData = new EnemyWaveData {
        creatureType = new List<CreatureCardType>(),
        spawnCoord = new List<AxialCoordinate>(),
        totalExpGain = expGain
    };

    foreach (var enemy in enemyCreatures) {
      waveData.creatureType.Add(enemy.creatureCardAttribute.creatureCardType);
      waveData.spawnCoord.Add(enemy.hexNode.coord);
    }

    var json = JsonUtility.ToJson(waveData);
    var resourcesDir = Path.Combine(Application.dataPath, "Resources", "EnemyWaves");
    if (!Directory.Exists(resourcesDir)) {
      Directory.CreateDirectory(resourcesDir);
    }

    var filename = $"{EnemyWavesResourceName}_{waveIdx}";
    if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) {
      filename += ".json";
    }

    var fullPath = Path.Combine(resourcesDir, filename);
    File.WriteAllText(fullPath, json);
    UnityEditor.AssetDatabase.Refresh();

    Debug.Log($"Saved enemy wave to: {fullPath}");
#else
        Debug.LogWarning("SaveCurBattleWave: 仅在编辑器下将文件写入 Assets/Resources。运行时请使用 Application.persistentDataPath。");
#endif
  }
  #endregion

  #region Life cycle Methods
  private void InvokeTick() {
    // Debug.Log("BattleWorldManager Invoke Tick");
    PlayerTick?.Invoke();
    EnemyTick?.Invoke();

    NormalActions?.Invoke();
    // Debug.Log("BattleWorldManager: DamageActions invoked.");
    DamageActions?.Invoke();
    // Debug.Log("BattleWorldManager: DamageActions finished.");

    NormalActions = null;
    DamageActions = null;
    // Debug.Log("BattleWorldManager Tick End");

    // Check whether battle end
    if (enemyCreatures.Count == 0 || InBattleCreatures.Count == 0) {
      EndBattle();
    }

    // Remove dead creatures
    foreach (var creature in InBattleCreatures.ToList()) {
      if (creature.curAttribute.health <= 0) {
        OnCreatureDead?.Invoke(creature);
        RemoveObj(creature);
      }
    }

    foreach (var creature in enemyCreatures.ToList()) {
      if (creature.curAttribute.health <= 0) {
        OnCreatureDead?.Invoke(creature);
        RemoveObj(creature);
      }
    }
  }

  public void OnCardClicked(B_Creature card) {
    var panel = Instantiate(attributeDisplayPrefab, GetComponentInParent<Canvas>().transform)
        .GetComponent<CreatureAttributeDisplay>();
    if (!InBattle) {
      panel.UpdateAttributes(card.creatureCardAttribute, card.actAttribute);
    }
    else {
      panel.UpdateAttributes(card.creatureCardAttribute, card.curAttribute);
    }
  }

  public void EndBattle() {
    PlayerTick = null;
    EnemyTick = null;

    OnBattleEnd?.Invoke();
    rewardPanel.SetActive(value: true);
    if (enemyCreatures.Count == 0) {
      Debug.Log("BattleWorldManager: Battle Won!");
      // gain exp
      var expGain = currentWaveData.totalExpGain / Math.Min(val1: 1, playerDeployedCreatureIDs.Count);
      foreach (var id in playerDeployedCreatureIDs) {
        CardManager.Instance.GainEXP(id, expGain);
      }

      // show reward cards
      CardManager.Instance.battleReward.Clear();
      var enemyTypes = new List<CreatureCardType>();
      enemyTypes.AddRange(currentWaveData.creatureType);
      // enemyTypes.AddRange(testAddEnemyTypes);
      foreach (var creature in enemyTypes) {
        // Debug.Log($"BattleWorldManager: Processing drop for creature {creature}");
        var creatureDescription = new Card.CardDescription {
            cardType = CardType.Creatures,
            creatureCardType = creature
        };
        var dropcards = DataBaseManager.Instance
            .GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(creatureDescription).basicAttributes.dropItem;

        // Drop card due to drop weight
        if (dropcards.Count == 0) {
          continue;
        }

        var totalWeight = 0;
        foreach (var dropcard in dropcards) {
          totalWeight += dropcard.dropWeight;
        }

        if (totalWeight <= 0) {
          Debug.LogWarning($"BattleWorldManager: Creature {creature} has total drop weight = 0, skipping drop.");
          continue;
        }

        var randomWeight = UnityEngine.Random.Range(minInclusive: 0, totalWeight);
        var currentWeight = 0;
        // Debug.Log($"BattleWorldManager: Dropping card for creature {creature}, Total weight {totalWeight}, Random weight {randomWeight}");
        foreach (var dropcard in dropcards) {
          currentWeight += dropcard.dropWeight;
          // Debug.Log($"BattleWorldManager: Current weight {currentWeight}, Random weight {randomWeight}");
          if (randomWeight < currentWeight) {
            Debug.Log($"BattleWorldManager: Dropped card {dropcard.cardDescription} from creature {creature}");
            // Create reward card
            for (var i = 0; i < dropcard.dropCount; i++) {
              var rewardCardGO = Instantiate(rewardCardPrefab, rewardCardPanel.transform);
              var rewardCard = rewardCardGO.GetComponent<DisplayCard>();
              rewardCard.Initialize(dropcard.cardDescription);
              CardManager.Instance.battleReward.Add(dropcard.cardDescription);
            }

            break;
          }
        }
      }

      CardManager.Instance.currentWave++;
    }
    else {
      Debug.Log("BattleWorldManager: Battle Lost!");
      FailedText.SetActive(value: true);
    }

    InBattle = false;
  }
  #endregion
}