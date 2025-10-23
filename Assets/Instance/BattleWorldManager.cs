using System;
using System.Collections.Generic;
using Category;
using Category.Battle;
using UnityEngine;

public class BattleWorldManager : MonoBehaviour
{
    public static BattleWorldManager Instance;
    public GameObject PreparationArea;
    public GameObject BattleCreaturePrefab;
    public Transform DraggingSlot;
    public List<B_Creature> playerCreatures = new List<B_Creature>();
    public List<B_Creature> enemyCreatures = new List<B_Creature>();
    // Tick 事件
    public event Action PlayerTick;
    public event Action EnemyTick;
    public event Action NormalActions;
    public event Action DamageActions;

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
    }

    void OnEnable()
    {
        foreach (var id in CardManager.Instance.battleSceneCreatureCardIDs)
        {
            Debug.Log($"BattleWorldManager adding battle object with card ID: {id}");
            AddBattleObject(id);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InvokeTick();
        }
    }

    public void InvokeTick()
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

    // Test
    [Header("Test Methods")]
    public CreatureCardType testCreatureCardType;
    public LineUp lineUp;
    [ContextMenu("Add Obj")]
    public void AddObj()
    {
        var creatureGO = Instantiate(BattleCreaturePrefab, PreparationArea.transform.position, Quaternion.identity, PreparationArea.transform);
        var creature = creatureGO.GetComponent<B_Creature>();
        if (lineUp == LineUp.Player)
        {
            playerCreatures.Add(creature);
            PlayerTick += creature.Tick;
        }
        else
        {
            enemyCreatures.Add(creature);
            EnemyTick += creature.Tick;
        }

        Card.CardDescription cardDescription = new Card.CardDescription
        {
            cardType = CardType.Creatures,
            creatureCardType = testCreatureCardType
        };
        var attr = DataBaseManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(cardDescription);
        creature.creatureAttribute = attr;
        creature.curAttribute = (CardAttributeDB.CreatureCardAttribute.BasicAttributes)attr.basicAttributes.Clone();
        creature.lineUp = lineUp;
    }

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
        var creatureGO = Instantiate(BattleCreaturePrefab, PreparationArea.transform.position, Quaternion.identity, PreparationArea.transform);
        creatureGO.transform.position += new Vector3(UnityEngine.Random.Range(-15f, 15f), UnityEngine.Random.Range(-15f, 15f), 0);

        var creature = creatureGO.GetComponent<B_Creature>();
        playerCreatures.Add(creature);
        PlayerTick += creature.Tick;

        creature.creatureAttribute = attr;
        creature.curAttribute = (CardAttributeDB.CreatureCardAttribute.BasicAttributes)attr.basicAttributes.Clone();
        creature.lineUp = LineUp.Player;
    }
}