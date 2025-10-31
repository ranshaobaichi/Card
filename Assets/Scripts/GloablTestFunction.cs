using Category;
using UnityEngine;

// 该脚本用于直接挂载在场景中的GameObject上，方便调用一些测试功能
public class GlobalTestFunction : MonoBehaviour
{
    public static GlobalTestFunction Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    // /// <summary>
    // /// 创建一个战斗世界对象
    // /// </summary>
    [SerializeField] public Category.Battle.LineUp lineUp;
    [SerializeField] public Category.CreatureCardType creatureCardType;
    public void CreateBattleWorldObj()
    {
        FindAnyObjectByType<BattleWorldManager>().AddObj(lineUp, creatureCardType);
    }

    /// <summary>
    /// 创建一个战斗装备对象
    /// </summary>
    [SerializeField] public EquipmentCardType equipmentCardType;
    public void CreateBattleEquipmentObj()
    {
        FindAnyObjectByType<BattleWorldManager>().AddEquipment(equipmentCardType);
    }

    /// <summary>
    /// 创建一个敌人波次
    /// </summary>
    [SerializeField] public int waveIndexToAdd;
    public void CreateEnemyWave()
    {
        FindAnyObjectByType<BattleWorldManager>().SaveCurBattleWave(waveIndexToAdd);
    }

    /// <summary>
    /// 加载一个敌人波次
    /// </summary>
    [SerializeField] public int waveIndexToLoad;
    public bool LoadEnemyWave()
    {
        return FindAnyObjectByType<BattleWorldManager>().LoadBattleWave(waveIndexToLoad);
    }

    /// <summary>
    /// 创建一个生产世界的卡牌
    /// </summary>
    [SerializeField] public Card.CardDescription cardDescription;
    public void CreateProductionWorldObj()
    {
        // Get the centre of the screen in world coordinates
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        FindAnyObjectByType<CardManager>().CreateCard(cardDescription, screenCenter);
    }

    /// <summary>
    /// 切换游戏阶段
    /// </summary>
    public void ChangeGameState()
    {
        FindAnyObjectByType<TimeManager>().ChangeState();
    }


    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void SaveGameData()
    {
        FindAnyObjectByType<SaveDataManager>().SaveGame();
    }
}