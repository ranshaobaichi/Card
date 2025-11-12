using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

[CustomEditor(typeof(GlobalTestFunction))]
public class GlobalTestFunctionEditor : Editor
{
    SerializedProperty lineUpProp;
    SerializedProperty battleCreatureCardTypeProp;
    SerializedProperty cardDescProp;
    SerializedProperty waveIndexToAddProp;
    SerializedProperty waveIndexToLoadProp;
    SerializedProperty equipmentCardTypeProp;
    SerializedProperty expGainToAddProp;

    void OnEnable()
    {
        lineUpProp = serializedObject.FindProperty("lineUp");
        battleCreatureCardTypeProp = serializedObject.FindProperty("creatureCardType");
        cardDescProp = serializedObject.FindProperty("cardDescription");
        waveIndexToAddProp = serializedObject.FindProperty("waveIndexToAdd");
        waveIndexToLoadProp = serializedObject.FindProperty("waveIndexToLoad");
        equipmentCardTypeProp = serializedObject.FindProperty("equipmentCardType");
        expGainToAddProp = serializedObject.FindProperty("expGainToAdd");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var globalTestFunc = target as GlobalTestFunction;

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("生产世界卡牌", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cardDescProp, new GUIContent("卡牌种类"));
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.ProductionScene;
        if (GUILayout.Button("创建 生产世界 卡牌"))
        {
            globalTestFunc.CreateProductionWorldObj();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("清除所有卡牌", EditorStyles.boldLabel);
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.ProductionScene;
        if (GUILayout.Button("清除所有卡牌"))
        {
            globalTestFunc.ClearAllCards();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("战斗世界卡牌", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(lineUpProp, new GUIContent("队伍"));
        EditorGUILayout.PropertyField(battleCreatureCardTypeProp, new GUIContent("生物卡牌类型"));
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.BattleScene;
        if (GUILayout.Button("创建 战斗世界 对象"))
        {
            globalTestFunc.CreateBattleWorldObj();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("战斗世界装备卡牌", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(equipmentCardTypeProp, new GUIContent("装备卡牌类型"));
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.BattleScene;
        if (GUILayout.Button("创建 战斗世界 装备"))
        {
            globalTestFunc.CreateBattleEquipmentObj();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("保存当前场上敌人信息作为配置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(waveIndexToAddProp, new GUIContent("第几波次"));
        EditorGUILayout.PropertyField(expGainToAddProp, new GUIContent("经验值奖励"));
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.BattleScene;
        if (GUILayout.Button("创建 敌人波次"))
        {
            globalTestFunc.CreateEnemyWave();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("加载敌人波次配置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(waveIndexToLoadProp, new GUIContent("第几波次"));
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.BattleScene;
        if (GUILayout.Button("加载 敌人波次"))
        {
            if (!globalTestFunc.LoadEnemyWave())
            {
                Debug.LogError("加载敌人波次失败");
            }
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("保存游戏数据", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("仅在生产世界场景下可用");
        GUI.enabled = EditorApplication.isPlaying && SceneManager.currentScene == SceneManager.ProductionScene;
        if (GUILayout.Button("保存游戏数据"))
        {
            globalTestFunc.SaveGameData();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("游戏阶段切换", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("当前阶段为：" + (FindAnyObjectByType<TimeManager>()?.GetCurrentState().ToString() ?? "未知"));
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("切换游戏阶段"))
        {
            globalTestFunc.ChangeGameState();
        }
        EditorGUILayout.Space();

        /// 
        GUI.enabled = true;
        EditorGUILayout.LabelField("退出游戏", EditorStyles.boldLabel);
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("退出游戏"))
        {
            SceneManager.QuitToStartScene();
        }
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }
}