using System;
using System.Collections.Generic;
using System.IO;
using Category;
using Category.Battle;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EnemyWaveData
{
    public CreatureCardType creatureType;
    public AxialCoordinate spawnCoord;
}
public class BattleWorldManager : MonoBehaviour
{
    public static BattleWorldManager Instance;
    public static readonly string EnemyWavesResourcePath = "EnemyWaves/";
    public static readonly string EnemyWavesResourceName = "EnemyWave";
    public static bool InBattle = false;
    [Header("Battle World Prefabs")]
    public GameObject BattleCreaturePrefab;
    public GameObject EquipmentSlotPrefab;
    public GameObject EquipmentPrefab;
    public GameObject tooltipPrefab;
    public GameObject attributeDisplayPrefab;

    [Header("Battle World Contents")]
    public GameObject PreparationAreaContent;
    public GameObject EquipmentAreaContent;
    public RectTransform CreatureScrollView;
    public Transform DraggingSlot;
    public GameObject TraitGameobject;
    public Button StartBattleButton;
    [Header("Battle World References")]
    public List<B_Creature> playerCreatures = new List<B_Creature>();
    public List<B_Creature> enemyCreatures = new List<B_Creature>();
    public List<B_Equipment> equipments = new List<B_Equipment>();
    [Header("Traits")]
    // (Trait, count), if count == 0, the trait is inactive but has relevant creatures on the field
    public Dictionary<Trait, ITraitHolder> traitObjDict = new Dictionary<Trait, ITraitHolder>();
    public List<(Trait, int)> activeTraits = new List<(Trait, int)>();

    // Tick Events
    public event Action PlayerTick;
    public event Action EnemyTick;
    public event Action NormalActions;
    public event Action DamageActions;
    public event Action OnBattleStart;
    public event Action OnBattleEnd;

    #region TEST_FUNCTIONS_AND_DATA
    public Button nextSceneBtn;
    /// <summary>
    /// TEST FUNCTION, create a tmp battle creature which will not be saved
    /// </summary>
    /// <param name="lineUp"></param>
    /// <param name="testCreatureCardType"></param>
    public B_Creature AddObj(LineUp lineUp, CreatureCardType testCreatureCardType)
    {
        var creatureGO = Instantiate(BattleCreaturePrefab, PreparationAreaContent.transform.position, Quaternion.identity, PreparationAreaContent.transform);
        var creature = creatureGO.GetComponent<B_Creature>();
        var image = creatureGO.GetComponent<Image>();
        if (lineUp == LineUp.Player)
        {
            playerCreatures.Add(creature);
            PlayerTick += creature.Tick;
            image.color = Color.blue;
        }
        else
        {
            enemyCreatures.Add(creature);
            EnemyTick += creature.Tick;
            image.color = Color.red;
        }

        Card.CardDescription cardDescription = new Card.CardDescription
        {
            cardType = CardType.Creatures,
            creatureCardType = testCreatureCardType
        };
        var attr = DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardDescription);
        creature.curAttribute = (CardAttributeDB.CreatureCardAttribute.BasicAttributes)attr.basicAttributes.Clone();
        creature.lineUp = lineUp;
        return creature;
    }

    public B_Equipment AddEquipment(ResourceCardType equipmentCardType)
    {
        if (!DataBaseManager.Instance.IsEquipmentCard(equipmentCardType))
        {
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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        nextSceneBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.ProductionScene));
    }

    void OnEnable()
    {
        foreach (var id in CardManager.Instance.battleSceneCreatureCardIDs)
        {
            Debug.Log($"BattleWorldManager adding battle object with card ID: {id}");
            AddBattleObject(id);
        }

        foreach (var (id, attr) in CardManager.Instance.GetResourceCardAttributes())
            if (attr.resourceClassification == ResourceCardClassification.Equipment)
            {
                AddBattleEquipment(id);
            }

        foreach (var trait in TraitGameobject.GetComponentsInChildren<B_Trait>())
        {
            if (trait is ITraitHolder traitHolder)
            {
                traitObjDict[trait.traitType] = traitHolder;
                OnBattleStart += traitHolder.OnBattleStart;
            }
        }

        StartBattleButton.onClick.AddListener(() =>
        {
            InBattle = true;
            foreach (var creature in playerCreatures)
            {
                creature.curAttribute = creature.actAttribute;
            }
            foreach (var creature in enemyCreatures)
            {
                creature.curAttribute = creature.actAttribute;
            }
            OnBattleStart?.Invoke();
        });
    }

    void Update()
    {
        if (InBattle && Input.GetKeyDown(KeyCode.Space))
        {
            InvokeTick();
        }
    }

    public void InstantiateLog(string logContent, TooltipText.TooltipMode mode = TooltipText.TooltipMode.Normal)
    {
        Instantiate(tooltipPrefab, GetComponentInParent<Canvas>().transform).GetComponent<TooltipText>()
            .SetTooltipText(logContent, mode);
    }

    # region BATTLEWORLD OBJS APIS
    public void RemoveObj(B_Creature creature)
    {
        if (creature.lineUp == LineUp.Player)
        {
            playerCreatures.Remove(creature);
            PlayerTick -= creature.Tick;
            CardManager.Instance.RemoveCardAttribute(creature.cardID);
        }
        else
        {
            enemyCreatures.Remove(creature);
            EnemyTick -= creature.Tick;
        }
        HexNode hexNode = creature.hexNode;
        if (hexNode != null)
        {
            HexNodeManager.MoveObject(creature, hexNode, null);
        }
        Destroy(creature.gameObject);
    }

    public void AddBattleObject(long cardID)
    {
        var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardID);
        var creatureGO = Instantiate(BattleCreaturePrefab, PreparationAreaContent.transform.position, Quaternion.identity, PreparationAreaContent.transform);

        var creature = creatureGO.GetComponent<B_Creature>();
        playerCreatures.Add(creature);
        PlayerTick += creature.Tick;

        creature.Init(cardID, LineUp.Player);
    }

    public B_Equipment AddBattleEquipment(long cardID)
    {
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
    public void AddBattleEquipment(B_Equipment equipment)
    {
        var equipmentSlot = Instantiate(EquipmentSlotPrefab, EquipmentAreaContent.transform);
        equipment.transform.SetParent(equipmentSlot.transform, false);
        equipment.equipmentSlot = equipmentSlot;
    }

    public bool LoadBattleWave(int waveIdx)
    {
        string path = EnemyWavesResourcePath + EnemyWavesResourceName + '_' + waveIdx;
        TextAsset waveDataAsset = Resources.Load<TextAsset>(path);
        if (waveDataAsset == null)
        {
            Debug.LogError($"LoadBattleWave: 未找到敌人波次资源: {path}");
            return false;
        }

        string[] lines = waveDataAsset.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            EnemyWaveData data = JsonUtility.FromJson<EnemyWaveData>(line);
            var creatureGO = AddObj(LineUp.Enemy, data.creatureType);
            HexNodeManager.MoveObject(creatureGO, null, HexNodeManager.Instance.Tiles[data.spawnCoord]);
        }
        return true;
    }

    public void SaveCurBattleWave(int waveIdx)
    {
#if UNITY_EDITOR
        string json = "";
        foreach (var enemy in enemyCreatures)
        {
            EnemyWaveData data = new EnemyWaveData
            {
                creatureType = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(enemy.cardID).creatureCardType,
                spawnCoord = enemy.hexNode.coord
            };
            string enemyJson = JsonUtility.ToJson(data);
            json += enemyJson + "\n";
        }

        string resourcesDir = Path.Combine(Application.dataPath, "Resources", "EnemyWaves");
        if (!Directory.Exists(resourcesDir))
            Directory.CreateDirectory(resourcesDir);

        string filename = $"{EnemyWavesResourceName}_{waveIdx}";
        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            filename += ".json";

        string fullPath = Path.Combine(resourcesDir, filename);
        File.WriteAllText(fullPath, json);
        UnityEditor.AssetDatabase.Refresh();

        Debug.Log($"Saved enemy wave to: {fullPath}");
#else
        Debug.LogWarning("SaveCurBattleWave: 仅在编辑器下将文件写入 Assets/Resources。运行时请使用 Application.persistentDataPath。");
#endif
    }

    #endregion

    #region Life cycle Methods
    private void InvokeTick()
    {
        Debug.Log("BattleWorldManager Invoke Tick");
        PlayerTick?.Invoke();
        EnemyTick?.Invoke();

        NormalActions?.Invoke();
        DamageActions?.Invoke();

        NormalActions = null;
        DamageActions = null;
        Debug.Log("BattleWorldManager Tick End");
    }


    public void OnCardClicked(B_Creature card)
    {
        CreatureAttributeDisplay panel = Instantiate(attributeDisplayPrefab, GetComponentInParent<Canvas>().transform).GetComponent<CreatureAttributeDisplay>();
        panel.UpdateAttributes(CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(card.cardID), basicAttributes: card.actAttribute);
    }
    #endregion

    #region Trait APIS
    public void UpdateActiveTraits()
    {
        activeTraits.Clear();
        // TODO: update activeTraits
        throw new NotImplementedException();

        // set currentTraitCreatureCount for each trait object
        foreach (var (trait, count) in activeTraits)
        {
            B_Trait traitObj = traitObjDict[trait] as B_Trait;
            traitObj.currentTraitCreatureCount = count;
        }
    }
    #endregion
}