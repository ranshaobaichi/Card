using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using Category;
using System.Linq;

public class CardAttributeDBCsvImporter : EditorWindow
{
    private string creatureCardCsvPath = "";
    private string resourceCardCsvPath = "";
    private CardAttributeDB cardAttributeDB;
    private char delimiter = ','; // CSV分隔符
    private List<string> errorList = new List<string>(); // 记录导入错误

    [MenuItem("Tools/Import Card Attributes From CSV")]
    public static void ShowWindow()
    {
        GetWindow<CardAttributeDBCsvImporter>("卡牌属性CSV导入工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("卡牌属性CSV导入工具", EditorStyles.boldLabel);

        // 生物卡CSV选择
        EditorGUILayout.BeginHorizontal();
        creatureCardCsvPath = EditorGUILayout.TextField("生物卡CSV文件:", creatureCardCsvPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("选择生物卡CSV文件", "Assets/DataBase/Data", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                creatureCardCsvPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        // 资源卡CSV选择
        EditorGUILayout.BeginHorizontal();
        resourceCardCsvPath = EditorGUILayout.TextField("资源卡CSV文件:", resourceCardCsvPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("选择资源卡CSV文件", "Assets/DataBase/Data", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                resourceCardCsvPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        cardAttributeDB = EditorGUILayout.ObjectField("目标CardAttributeDB:", cardAttributeDB,
            typeof(CardAttributeDB), false) as CardAttributeDB;

        EditorGUILayout.Space(10);

        // 导入按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("导入生物卡数据"))
        {
            ImportCreatureCardData();
        }
        
        if (GUILayout.Button("导入资源卡数据"))
        {
            ImportResourceCardData();
        }
        
        if (GUILayout.Button("导入全部数据"))
        {
            ImportCreatureCardData();
            ImportResourceCardData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);
        GUILayout.Label("CSV文件格式说明", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. 生物卡CSV格式:\n   CreatureType,CraftEfficiency,ExploreEfficiency,InteractEfficiency\n\n" +
            "   - CreatureType: 生物类型枚举值\n" +
            "   - 效率类型可用值: None, Frenzy, Fast, Normal, Slow, VerySlow\n\n" +
            "2. 资源卡CSV格式:\n   ResourceType,IsResourcePoint\n\n" +
            "   - ResourceType: 资源类型枚举值\n" +
            "   - IsResourcePoint: true或false",
            MessageType.Info);
    }

    private void ImportCreatureCardData()
    {
        try
        {
            if (string.IsNullOrEmpty(creatureCardCsvPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择生物卡CSV文件！", "确定");
                return;
            }

            if (cardAttributeDB == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择目标CardAttributeDB对象！", "确定");
                return;
            }

            // 读取所有文本内容
            string[] lines = File.ReadAllLines(creatureCardCsvPath);

            if (lines.Length <= 1)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件为空或仅包含标题行！", "确定");
                return;
            }

            // 解析标题行以获取列索引
            string[] headers = ParseCsvLine(lines[0]);
            int typeIndex = Array.IndexOf(headers, "CreatureType");
            int craftIndex = Array.IndexOf(headers, "CraftEfficiency");
            int exploreIndex = Array.IndexOf(headers, "ExploreEfficiency");
            int interactIndex = Array.IndexOf(headers, "InteractEfficiency");

            // 验证所有必要的列都存在
            if (typeIndex < 0 || craftIndex < 0 || exploreIndex < 0 || interactIndex < 0)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件缺少必要的列！请检查标题行。", "确定");
                return;
            }

            // 开始记录操作以支持撤销
            Undo.RecordObject(cardAttributeDB, "Import Creature Card Data from CSV");
            cardAttributeDB.creatureCardAttributes.Clear();
            errorList.Clear();

            int successCount = 0;

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = ParseCsvLine(lines[i]);

                    if (values.Length <= Math.Max(typeIndex, Math.Max(craftIndex, Math.Max(exploreIndex, interactIndex))))
                    {
                        Debug.LogWarning($"第{i + 1}行数据列不足，已跳过");
                        continue;
                    }

                    // 跳过注释行
                    if (values[0].StartsWith("#"))
                        continue;

                    // 解析生物卡类型
                    if (Enum.TryParse<CreatureCardType>(values[typeIndex], out CreatureCardType creatureType))
                    {
                        CardAttributeDB.CreatureCardAttribute attribute = new CardAttributeDB.CreatureCardAttribute();

                        Func<string, string> ParseWorkType = value => value.Split('-').Last();
                        string craftTypeString = ParseWorkType(values[craftIndex]);
                        string exploreTypeString = ParseWorkType(values[exploreIndex]);
                        string interactTypeString = ParseWorkType(values[interactIndex]);

                        // 解析效率类型
                        if (Enum.TryParse<WorkEfficiencyType>(craftTypeString, out WorkEfficiencyType craftEfficiency))
                            attribute.craftWorkEfficiency = craftEfficiency;
                        else
                            errorList.Add($"第{i + 1}行: 无效的合成效率类型 '{craftTypeString}'");

                        if (Enum.TryParse<WorkEfficiencyType>(exploreTypeString, out WorkEfficiencyType exploreEfficiency))
                            attribute.exploreWorkEfficiency = exploreEfficiency;
                        else
                            errorList.Add($"第{i + 1}行: 无效的探索效率类型 '{exploreTypeString}'");

                        if (Enum.TryParse<WorkEfficiencyType>(interactTypeString, out WorkEfficiencyType interactEfficiency))
                            attribute.interactWorkEfficiency = interactEfficiency;
                        else
                            errorList.Add($"第{i + 1}行: 无效的建造效率类型 '{interactTypeString}'");

                        // 添加到字典
                        cardAttributeDB.creatureCardAttributes[creatureType] = attribute;
                        successCount++;
                        Debug.Log($"成功导入生物卡属性: {creatureType}");
                    }
                    else
                    {
                        errorList.Add($"第{i + 1}行: 无效的生物卡类型 '{values[typeIndex]}'");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"解析第{i + 1}行时出错: {ex.Message}");
                    errorList.Add($"第{i + 1}行: {ex.Message}");
                }
            }

            EditorUtility.SetDirty(cardAttributeDB);
            AssetDatabase.SaveAssets();

            DisplayResults("生物卡", successCount);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("导入错误", $"导入生物卡过程中出现错误: {ex.Message}", "确定");
            Debug.LogException(ex);
        }
    }

    private void ImportResourceCardData()
    {
        try
        {
            if (string.IsNullOrEmpty(resourceCardCsvPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择资源卡CSV文件！", "确定");
                return;
            }

            if (cardAttributeDB == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择目标CardAttributeDB对象！", "确定");
                return;
            }

            // 读取所有文本内容
            string[] lines = File.ReadAllLines(resourceCardCsvPath);

            if (lines.Length <= 1)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件为空或仅包含标题行！", "确定");
                return;
            }

            // 解析标题行以获取列索引
            string[] headers = ParseCsvLine(lines[0]);
            int typeIndex = Array.IndexOf(headers, "ResourceType");
            int isResourcePointIndex = Array.IndexOf(headers, "IsResourcePoint");

            // 验证所有必要的列都存在
            if (typeIndex < 0 || isResourcePointIndex < 0)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件缺少必要的列！请检查标题行。", "确定");
                return;
            }

            // 开始记录操作以支持撤销
            Undo.RecordObject(cardAttributeDB, "Import Resource Card Data from CSV");
            cardAttributeDB.resourceCardAttributes.Clear();
            errorList.Clear();

            int successCount = 0;

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = ParseCsvLine(lines[i]);

                    if (values.Length <= Math.Max(typeIndex, isResourcePointIndex))
                    {
                        Debug.LogWarning($"第{i + 1}行数据列不足，已跳过");
                        continue;
                    }

                    // 跳过注释行
                    if (values[0].StartsWith("#"))
                        continue;

                    // 解析资源卡类型
                    if (Enum.TryParse<ResourceCardType>(values[typeIndex], out ResourceCardType resourceType))
                    {
                        CardAttributeDB.ResourceCardAttribute attribute = new CardAttributeDB.ResourceCardAttribute();

                        // 解析是否为资源点
                        if (bool.TryParse(values[isResourcePointIndex], out bool isResourcePoint))
                            attribute.isResourcePoint = isResourcePoint;
                        else
                            errorList.Add($"第{i + 1}行: 无效的布尔值 '{values[isResourcePointIndex]}'");

                        // 添加到字典
                        cardAttributeDB.resourceCardAttributes[resourceType] = attribute;
                        successCount++;
                        Debug.Log($"成功导入资源卡属性: {resourceType}");
                    }
                    else
                    {
                        errorList.Add($"第{i + 1}行: 无效的资源卡类型 '{values[typeIndex]}'");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"解析第{i + 1}行时出错: {ex.Message}");
                    errorList.Add($"第{i + 1}行: {ex.Message}");
                }
            }

            EditorUtility.SetDirty(cardAttributeDB);
            AssetDatabase.SaveAssets();

            DisplayResults("资源卡", successCount);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("导入错误", $"导入资源卡过程中出现错误: {ex.Message}", "确定");
            Debug.LogException(ex);
        }
    }

    private void DisplayResults(string cardType, int successCount)
    {
        EditorUtility.DisplayDialog("导入完成", $"成功导入 {successCount} 个{cardType}属性", "确定");
        if (errorList.Count > 0)
        {
            StringBuilder errorMsg = new StringBuilder($"导入{cardType}时出现以下错误:\n");
            foreach (var error in errorList)
            {
                errorMsg.AppendLine(error);
            }
            EditorUtility.DisplayDialog("导入警告", errorMsg.ToString(), "确定");
        }
    }

    // 解析CSV行，处理引号内的逗号
    private string[] ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == delimiter && !inQuotes)
            {
                result.Add(field.ToString().Trim());
                field.Clear();
                continue;
            }

            field.Append(c);
        }

        // 添加最后一个字段
        if (field.Length > 0)
            result.Add(field.ToString().Trim());

        return result.ToArray();
    }
}