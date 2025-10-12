using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalTestFunction))]
public class GlobalTestFunctionEditor : Editor
{
    SerializedProperty lineUpProp;
    SerializedProperty priorityProp;
    SerializedProperty cardDescProp;

    void OnEnable()
    {
        lineUpProp = serializedObject.FindProperty("lineUp");
        priorityProp = serializedObject.FindProperty("priority");
        cardDescProp = serializedObject.FindProperty("cardDescription");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var globalTestFunc = target as GlobalTestFunction;

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("战斗世界卡牌", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(lineUpProp, new GUIContent("队伍"));
        EditorGUILayout.PropertyField(priorityProp, new GUIContent("行动优先级"));
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("创建 战斗世界 对象"))
        {
            globalTestFunc.CreateBattleWorldObj();
        }
        EditorGUILayout.Space();

        ///
        GUI.enabled = true;
        EditorGUILayout.LabelField("生产世界卡牌", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cardDescProp, new GUIContent("卡牌种类"));
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("创建 生产世界 卡牌"))
        {
            globalTestFunc.CreateProductionWorldObj();
        }

        serializedObject.ApplyModifiedProperties();
    }
}