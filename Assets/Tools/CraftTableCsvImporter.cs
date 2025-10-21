using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using Category.Production;
using Category;

public class CraftTableCsvImporter : EditorWindow
{
    private string csvFilePath = "";
    private CraftTableDB craftTableDB;
    private char delimiter = ','; // CSV分隔符
    private List<(string, string)> errorList = new(); // 记录可能未成功导入的行

    [MenuItem("Tools/Import Craft Recipes From CSV")]
    public static void ShowWindow()
    {
        GetWindow<CraftTableCsvImporter>("配方CSV导入工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("合成表CSV导入工具", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField("CSV文件路径:", csvFilePath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("选择CSV文件", "Assets/DataBase/Data", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        craftTableDB = EditorGUILayout.ObjectField("目标CraftTableDB:", craftTableDB,
            typeof(CraftTableDB), false) as CraftTableDB;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("导入CSV数据"))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择CSV文件！", "确定");
                return;
            }

            if (craftTableDB == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择目标CraftTableDB对象！", "确定");
                return;
            }

            ImportCsvData();
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("CSV文件格式说明", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. CSV第一行应为标题: RecipeID,RecipeName,RecipeDescription,Workload,WorkType,InputCards,OutputCards\n\n" +
            "2. 卡牌格式为: 类型前缀:卡牌子类型:数量，多个卡牌用逗号分隔(多个卡牌需用引号包围)\n" +
            "   - 类型前缀: R=Resource(资源), C=Creature(生物), E=Event(事件)\n" +
            "   - 例如：R:Wood:1 或 \"R:Water:1,R:Meat:1\"\n\n" +
            "3. 工作类型(WorkType)可用值: None, Crafting, Cooking, Taming, Building, Gathering等",
            MessageType.Info);
    }

    private void ImportCsvData()
    {
        try
        {
            // 读取所有文本内容
            string[] lines = File.ReadAllLines(csvFilePath);

            if (lines.Length <= 1)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件为空或仅包含标题行！", "确定");
                return;
            }

            // 解析标题行以获取列索引
            string[] headers = ParseCsvLine(lines[0]);
            int idIndex = Array.IndexOf(headers, "RecipeID");
            int nameIndex = Array.IndexOf(headers, "RecipeName");
            int descIndex = Array.IndexOf(headers, "RecipeDescription");
            int workloadIndex = Array.IndexOf(headers, "Workload");
            int workTypeIndex = Array.IndexOf(headers, "WorkType");
            int inputCardsIndex = Array.IndexOf(headers, "InputCards");
            int outputCardsIndex = Array.IndexOf(headers, "OutputCards");

            // 验证所有必要的列都存在
            if (idIndex < 0 || nameIndex < 0 || descIndex < 0 || workloadIndex < 0 ||
                workTypeIndex < 0 || inputCardsIndex < 0 || outputCardsIndex < 0)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件缺少必要的列！请检查标题行。", "确定");
                return;
            }

            // 开始记录操作以支持撤销
            Undo.RecordObject(craftTableDB, "Import Recipe Data from CSV");
            craftTableDB.recipeList.Clear();

            int successCount = 0;

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = ParseCsvLine(lines[i]);

                    if (values.Length <= Math.Max(idIndex, Math.Max(nameIndex, Math.Max(descIndex,
                        Math.Max(workloadIndex, Math.Max(workTypeIndex,
                        Math.Max(inputCardsIndex, outputCardsIndex)))))))
                    {
                        Debug.LogWarning($"第{i + 1}行数据列不足，已跳过");
                        continue;
                    }
                    int ID = int.Parse(values[idIndex]);
                    if (ID > 0)
                    {
                        CraftTableDB.Recipe recipe = new CraftTableDB.Recipe();
                        // 基本配方信息
                        recipe.recipeID = ID;
                        recipe.recipeName = values[nameIndex];
                        recipe.recipeDescription = values[descIndex];
                        recipe.workload = float.Parse(values[workloadIndex]);
                        recipe.workType = ParseWorkType(values[workTypeIndex]);

                        // 解析输入卡牌
                        recipe.inputCards = ParseInputCardsList(values[inputCardsIndex]);

                        // 解析输出卡牌
                        recipe.outputCards = ParseOutputCardsList(values[outputCardsIndex]);

                        craftTableDB.recipeList.Add(recipe);
                        successCount++;
                        Debug.Log($"成功导入配方: {recipe.recipeName}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"解析第{i + 1}行时出错: {ex.Message}");
                }
            }

            EditorUtility.SetDirty(craftTableDB);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("导入完成", $"成功导入 {successCount} 个配方", "确定");
            if (errorList.Count > 0)
            {
                StringBuilder errorMsg = new StringBuilder("以下行的卡牌类型可能有误:\n");
                foreach (var (fullEntry, invalidType) in errorList)
                {
                    errorMsg.AppendLine($"卡牌条目: \"{fullEntry}\", 无效类型: \"{invalidType}\"");
                }
                EditorUtility.DisplayDialog("导入警告", errorMsg.ToString(), "确定");
            }
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("导入错误", $"导入过程中出现错误: {ex.Message}", "确定");
            Debug.LogException(ex);
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

    private WorkType ParseWorkType(string workTypeStr)
    {
        if (Enum.TryParse<WorkType>(workTypeStr, out WorkType result))
        {
            return result;
        }
        return WorkType.None;
    }

    private List<DropCard> ParseOutputCardsList(string cardsString)
    {
        List<DropCard> cards = new List<DropCard>();

        if (string.IsNullOrEmpty(cardsString))
            return cards;

        string[] cardEntries = cardsString.Split(',');
        foreach (string cardEntry in cardEntries)
        {
            string[] parts = cardEntry.Trim().Split('-');
            if (parts.Length == 4)
            {
                char typePrefix = parts[0].Trim()[0]; // R, C, E
                string cardTypeStr = parts[1].Trim();
                int count = int.Parse(parts[2].Trim());
                int dropWeight = int.Parse(parts[3].Trim());

                for (int i = 0; i < count; i++)
                {
                    DropCard cardDesc = new DropCard
                    {
                        cardDescription = new Card.CardDescription(),
                        dropCount = count,
                        dropWeight = dropWeight
                    };

                    // 设置卡牌类型
                    switch (typePrefix)
                    {
                        case 'R': // 资源卡
                            cardDesc.cardDescription.cardType = CardType.Resources;
                            if (Enum.TryParse<ResourceCardType>(cardTypeStr, out ResourceCardType resourceType))
                                cardDesc.cardDescription.resourceCardType = resourceType;
                            else
                                errorList.Add((cardsString, cardTypeStr)); // 记录错误
                            break;

                        case 'C': // 生物卡
                            cardDesc.cardDescription.cardType = CardType.Creatures;
                            if (Enum.TryParse<CreatureCardType>(cardTypeStr, out CreatureCardType creatureType))
                                cardDesc.cardDescription.creatureCardType = creatureType;
                            else
                                errorList.Add((cardsString, cardTypeStr)); // 记录错误
                            break;

                        case 'E': // 事件卡
                            cardDesc.cardDescription.cardType = CardType.Events;
                            if (Enum.TryParse<EventCardType>(cardTypeStr, out EventCardType eventType))
                                cardDesc.cardDescription.eventCardType = eventType;
                            else
                                errorList.Add((cardsString, cardTypeStr)); // 记录错误
                            break;
                    }

                    cards.Add(cardDesc);
                }
            }
        }

        return cards;
    }
    
    private List<Card.CardDescription> ParseInputCardsList(string cardsString)
    {
        List<Card.CardDescription> cards = new List<Card.CardDescription>();

        if (string.IsNullOrEmpty(cardsString))
            return cards;
            
        string[] cardEntries = cardsString.Split(',');
        foreach (string cardEntry in cardEntries)
        {
            string[] parts = cardEntry.Trim().Split('-');
            if (parts.Length == 3)
            {
                char typePrefix = parts[0].Trim()[0]; // R, C, E
                string cardTypeStr = parts[1].Trim();
                int count = int.Parse(parts[2].Trim());

                Card.CardDescription cardDesc = new Card.CardDescription();
                // 设置卡牌类型
                switch (typePrefix)
                {
                    case 'R': // 资源卡
                        cardDesc.cardType = CardType.Resources;
                        if (Enum.TryParse<ResourceCardType>(cardTypeStr, out ResourceCardType resourceType))
                            cardDesc.resourceCardType = resourceType;
                        else
                            errorList.Add((cardsString, cardTypeStr)); // 记录错误
                        break;

                    case 'C': // 生物卡
                        cardDesc.cardType = CardType.Creatures;
                        if (Enum.TryParse<CreatureCardType>(cardTypeStr, out CreatureCardType creatureType))
                            cardDesc.creatureCardType = creatureType;
                        else
                            errorList.Add((cardsString, cardTypeStr)); // 记录错误
                        break;

                    case 'E': // 事件卡
                        cardDesc.cardType = CardType.Events;
                        if (Enum.TryParse<EventCardType>(cardTypeStr, out EventCardType eventType))
                            cardDesc.eventCardType = eventType;
                        else
                            errorList.Add((cardsString, cardTypeStr)); // 记录错误
                        break;
                }
                    
                for (int i = 0; i < count; i++)
                {
                    cards.Add(cardDesc);
                }
            }
        }
        
        return cards;
    }
}