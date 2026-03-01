using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Category;
using Category.Battle;
using Category.BattleWorld;

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

  [HideInInspector]
  public bool InBattle;

  public bool mannualTickControl;
  public bool canDragEnemy;
  public float TickInterval = 1.0f;

  public EnemyWaveData currentWaveData;

  // private List<CreatureCardType> testAddEnemyTypes = new List<CreatureCardType>();
  private static bool showFirstTimeBattleTutorial = true;

  [Header("Battle World Prefabs")]
  public GameObject BattleCreaturePrefab;
  public GameObject tooltipPrefab;
  public GameObject attributeDisplayPrefab;
  public GameObject rewardCardPrefab;

  [Header("Battle World Contents")]
  public GameObject PreparationAreaContent;
  public RectTransform CreatureScrollView;
  public Transform DraggingSlot;
  public GameObject BattleTutorialPanel;
  public GameObject FailedText;
  public Button StartBattleButton;

  [Header("Reward Panel")]
  public GameObject rewardPanel;
  public GameObject rewardCardPanel;
  public Button nextSceneBtn;

  [Header("Battle World References")]
  private Dictionary<LineUp, List<B_Obj>> battleObjs;
  public List<long> playerDeployedCreatureIDs = new List<long>();

  // Tick Events
  public EventActionDictionary<LineUp> OnTick;
  public event Action NormalActions;
  public event Action DamageActions;

  #region TEST_FUNCTIONS_AND_DATA
  /// <summary>
  ///   TEST FUNCTION, create a tmp battle creature which will not be saved
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
    creature.lineUp = lineUp;
    creature.cardID = -1; // test creature has no valid cardID
    creature.displayCard.Initialize(cardDescription);

    battleObjs[lineUp].Add(creature);
    return creature;
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
    
    battleObjs = new Dictionary<LineUp, List<B_Obj>>();
    battleObjs.Add(LineUp.Player, new List<B_Obj>());
    battleObjs.Add(LineUp.Enemy, new List<B_Obj>());
  }

  private void UpdateSaveDataWaveIndex() {
    SaveDataManager.currentSaveData.currentWaveIndex = CardManager.Instance.currentWave;
  }

  private void Start() {
    SceneManager.BeforeSceneChanged += UpdateSaveDataWaveIndex;
  }

  private void OnDestroy() { SceneManager.BeforeSceneChanged -= UpdateSaveDataWaveIndex; }

  private void OnEnable() {
    // initialize battle objects from CardManager
    foreach (var id in CardManager.Instance.battleSceneCreatureCardIDs) {
      AddBattleObject(id);
    }
    
    // load current wave
    if (CardManager.Instance.currentWave >= 0) {
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
  }
  
  private void Update() {
    if (InBattle && mannualTickControl && Input.GetKeyDown(KeyCode.Space)) {
      InvokeTick();
    }
  }

  public void StartBattle() {
    InBattle = true;
    Debug.Log("Battle Started");
    playerDeployedCreatureIDs.Clear();
    OnTick?.ClearAllHandlers();

    foreach (var creature in battleObjs[LineUp.Player]) {
      if (creature is B_Creature playerCard) {
        playerDeployedCreatureIDs.Add(playerCard.cardID);
      }
    }

    foreach (var (lineUp, bObjs) in battleObjs) {
      foreach (var bObj in bObjs) {
        if (bObj is IBattleUpdateable battleObj) {
          OnTick?.Add(lineUp, battleObj.Tick);
        }
      }
    }
    
    // make sure the OnBattleStart event is invoked before all creatures set their curAttribute
    // because some traits may modify the attributes at the start of battle
    foreach (var (_, bObjs) in battleObjs) {
      foreach (var bObj in bObjs) {
        if (bObj is IBattleObj battleObj) {
          battleObj.OnBattleStart();
        }
      }
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
  /// <summary>
  ///   Remove a creature from battle world, including from hex node and relevant lists
  ///   Must use in battle
  /// </summary>
  /// <param name="obj"></param>
  public void RemoveObj(B_Obj obj) {
    if (!InBattle) {
      Debug.LogError("BattleWorldManager RemoveObj: Cannot remove creature when not in battle.");
      return;
    }

    LineUp lineUp = obj.lineUp;
    battleObjs[lineUp].Remove(obj);
    if (obj is IBattleUpdateable updateableObj) {
      OnTick.Remove(lineUp, updateableObj.Tick);
    }
    
    var hexNode = obj.hexNode;
    if (hexNode != null) {
      HexNodeManager.MoveObject(obj, hexNode, to: null);
    }

    obj.StopAllCoroutines();
    Destroy(obj.gameObject);
  }

  public void AddBattleObject(long cardID) {
    var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
    var creatureGO = Instantiate(BattleCreaturePrefab, PreparationAreaContent.transform.position, Quaternion.identity,
        PreparationAreaContent.transform);

    var creature = creatureGO.GetComponent<B_Creature>();
    creature.creatureCardAttribute = attr;
    battleObjs[LineUp.Player].Add(creature);

    creature.Init(cardID, LineUp.Player);
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

    foreach (var enemyObj in battleObjs[LineUp.Enemy]) {
      var enemy = enemyObj as B_Creature;
      if (enemy != null) {
        waveData.creatureType.Add(enemy.creatureCardAttribute.creatureCardType);
        waveData.spawnCoord.Add(enemy.hexNode.coord);
      }
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
    AssetDatabase.Refresh();

    Debug.Log($"Saved enemy wave to: {fullPath}");
#else
        Debug.LogWarning("SaveCurBattleWave: 仅在编辑器下将文件写入 Assets/Resources。运行时请使用 Application.persistentDataPath。");
#endif
  }

  public bool GetClosestDamageableOpponents(B_Obj thisObj, out IBattleDamageable opponent) {
    var thisLineUp = thisObj.lineUp;
    var thisHexNode = thisObj.hexNode;
    var closestDistance = int.MaxValue;
    opponent = null;
    foreach (var (lineUp, enemies) in battleObjs) {
      if (lineUp == thisLineUp || enemies == null || enemies.Count < 0) {
        continue;
      }

      foreach (var enemy in enemies) {
        var distance = thisHexNode.GetDistance(enemy.hexNode);
        if (distance < closestDistance && enemy is IBattleDamageable damageableEnemy && !damageableEnemy.IsDead()) {
          opponent = damageableEnemy;
          closestDistance = distance;
        }
      }
    }
    return opponent != null;
  }
  #endregion

  #region Life cycle Methods
  private void InvokeTick() {
    OnTick?[LineUp.Player].Invoke();
    OnTick?[LineUp.Enemy].Invoke();

    NormalActions?.Invoke();
    DamageActions?.Invoke();

    NormalActions = null;
    DamageActions = null;
    
    // Remove dead creatures
    foreach (var (_, bObjs) in battleObjs) {
      foreach (var bObj in bObjs) {
        if (bObj is IBattleDamageable damageableObj && damageableObj.IsDead()) {
          RemoveObj(bObj);
        }
      }
    }
    
    // Check whether battle end
    if ((battleObjs.TryGetValue(LineUp.Enemy, out var enemyObjs) && enemyObjs.Count == 0)
        ||
        (battleObjs.TryGetValue(LineUp.Player, out var playerObjs) && playerObjs.Count == 0)) {
      EndBattle();
    }
  }

  public void OnCardClicked(B_Creature card) {
    var panel = Instantiate(attributeDisplayPrefab, GetComponentInParent<Canvas>().transform)
        .GetComponent<CreatureAttributeDisplay>();
    panel?.UpdateAttributes(card.creatureCardAttribute, card.curAttribute);
  }

  public void EndBattle() {
    // Life cycle methods
    OnTick?.ClearAllHandlers();
    foreach (var (_, bObjs) in battleObjs) {
      foreach (var bObj in bObjs) {
        if (bObj is IBattleObj battleObj) {
          battleObj.OnBattleEnd();
        }
      }
    }
    
    rewardPanel.SetActive(value: true);
    if (battleObjs.TryGetValue(LineUp.Enemy, out var enemyObjs) && enemyObjs.Count == 0) {
      // gain exp
      var expGain = currentWaveData.totalExpGain / Math.Max(val1: 1, playerDeployedCreatureIDs.Count);
      foreach (var id in playerDeployedCreatureIDs) {
        CardManager.Instance.GainEXP(id, expGain);
      }

      // show reward cards
      CardManager.Instance.battleReward.Clear();
      var enemyTypes = new List<CreatureCardType>();
      enemyTypes.AddRange(currentWaveData.creatureType);
      foreach (var creature in enemyTypes) {
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

        var randomWeight = Random.Range(minInclusive: 0, totalWeight);
        var currentWeight = 0;
        foreach (var dropcard in dropcards) {
          currentWeight += dropcard.dropWeight;
          if (randomWeight < currentWeight) {
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
      FailedText.SetActive(value: true);
    }

    InBattle = false;
  }
  #endregion
}