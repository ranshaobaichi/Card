using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using Category; 
using Category.Production;

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
            "2. 资源卡CSV格式:\n   ResourceType,ResourceCardClassification,Durability\n\n" +
            "   - ResourceType: 资源类型枚举值\n" +
            "   - ResourceCardClassification: None, Food, Equipment, ResourcePoint, Others\n" +
            "   - Durability: 资源卡耐久值\n" +
            "   - SatietyValue: 资源卡饱腹值 (仅当分类为 Food 时有效)",
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
            
            // 获取各列索引
            Dictionary<string, int> columnIndices = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
            {
                columnIndices[headers[i]] = i;
            }
            
            // 验证必要的列是否存在
            if (!columnIndices.ContainsKey("CreatureType"))
            {
                EditorUtility.DisplayDialog("错误", "CSV文件缺少必要的CreatureType列！", "确定");
                return;
            }

            // 开始记录操作以支持撤销
            Undo.RecordObject(cardAttributeDB, "Import Creature Card Data from CSV");
            cardAttributeDB.creatureCardAttributes = new List<CardAttributeDB.CreatureCardAttribute>();
            cardAttributeDB.creatureCardAttributes.Clear();
            errorList.Clear();

            int successCount = 0;
            int valNums = 25;

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = ParseCsvLine(lines[i]);

                    // 跳过注释行
                    if (values[0].StartsWith("#"))
                        continue;

                    if (values.Length <= columnIndices["CreatureType"])
                    {
                        errorList.Add($"第{i + 1}行: 数据列不足，已跳过");
                        continue;
                    }

                    // 解析生物卡类型
                    if (!Enum.TryParse<CreatureCardType>(values[columnIndices["CreatureType"]], out CreatureCardType creatureType))
                    {
                        errorList.Add($"第{i + 1}行: 无效的生物卡类型 '{values[columnIndices["CreatureType"]]}'");
                        continue;
                    }

                    // 创建新的生物卡属性
                    CardAttributeDB.CreatureCardAttribute attribute = new CardAttributeDB.CreatureCardAttribute();
                    attribute.basicAttributes = new CardAttributeDB.CreatureCardAttribute.BasicAttributes();
                    attribute.basicAttributes.workEfficiencyAttributes =
                        new CardAttributeDB.CreatureCardAttribute.BasicAttributes.WorkEfficiencyAttributes();
                    attribute.levelUpAttributes = new CardAttributeDB.CreatureCardAttribute.LevelUpAttributes();

                    attribute.creatureCardType = creatureType;
                    attribute.basicAttributes.EXP = 0;
                    attribute.basicAttributes.level = 1;

                    int valCount = 0;

                    // 解析工作效率
                    Func<string, string> ParseWorkType = value => value.Split('-').Last();
                
                    if (columnIndices.ContainsKey("CraftEfficiency") && values.Length > columnIndices["CraftEfficiency"])
                    {
                        string craftTypeString = ParseWorkType(values[columnIndices["CraftEfficiency"]]);
                        if (Enum.TryParse<WorkEfficiencyType>(craftTypeString, out WorkEfficiencyType craftEfficiency))
                        {
                            attribute.basicAttributes.workEfficiencyAttributes.craftWorkEfficiency = craftEfficiency;
                            valCount++;
                        }
                        else
                            errorList.Add($"第{i + 1}行: 无效的合成效率类型 '{values[columnIndices["CraftEfficiency"]]}'");
                    }

                    if (columnIndices.ContainsKey("ExploreEfficiency") && values.Length > columnIndices["ExploreEfficiency"])
                    {
                        string exploreTypeString = ParseWorkType(values[columnIndices["ExploreEfficiency"]]);
                        if (Enum.TryParse<WorkEfficiencyType>(exploreTypeString, out WorkEfficiencyType exploreEfficiency))
                        {
                            attribute.basicAttributes.workEfficiencyAttributes.exploreWorkEfficiency = exploreEfficiency;
                            valCount++;
                        }
                        else
                            errorList.Add($"第{i + 1}行: 无效的探索效率类型 '{values[columnIndices["ExploreEfficiency"]]}'");
                    }

                    if (columnIndices.ContainsKey("InteractEfficiency") && values.Length > columnIndices["InteractEfficiency"])
                    {
                        string interactTypeString = ParseWorkType(values[columnIndices["InteractEfficiency"]]);
                        if (Enum.TryParse<WorkEfficiencyType>(interactTypeString, out WorkEfficiencyType interactEfficiency))
                        {
                            attribute.basicAttributes.workEfficiencyAttributes.interactWorkEfficiency = interactEfficiency;
                            valCount++;
                        }
                        else
                            errorList.Add($"第{i + 1}行: 无效的互动效率类型 '{values[columnIndices["InteractEfficiency"]]}'");
                    }
                
                    // 解析常规属性
                    if (columnIndices.ContainsKey("Satiety") && values.Length > columnIndices["Satiety"])
                    {
                        if (TryParseIntStat(values[columnIndices["Satiety"]], out int satietyBase, out int satietyGrowth))
                        {
                            attribute.basicAttributes.satiety = satietyBase;
                            attribute.levelUpAttributes.satietyGrowth = satietyGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("Health") && values.Length > columnIndices["Health"])
                    {
                        if (TryParseStat(values[columnIndices["Health"]], out float healthBase, out float healthGrowth))
                        {
                            attribute.basicAttributes.health = healthBase;
                            attribute.levelUpAttributes.healthGrowth = healthGrowth;
                            valCount += 2;
                        }
                    }
                
                    if (columnIndices.ContainsKey("AttackPower") && values.Length > columnIndices["AttackPower"])
                    {
                        if (TryParseStat(values[columnIndices["AttackPower"]], out float attackBase, out float attackGrowth))
                        {
                            attribute.basicAttributes.attackPower = attackBase;
                            attribute.levelUpAttributes.attackPowerGrowth = attackGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("SpellPower") && values.Length > columnIndices["SpellPower"])
                    {
                        if (TryParseStat(values[columnIndices["SpellPower"]], out float spellBase, out float spellGrowth))
                        {
                            attribute.basicAttributes.spellPower = spellBase;
                            attribute.levelUpAttributes.spellPowerGrowth = spellGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("Armor") && values.Length > columnIndices["Armor"])
                    {
                        if (TryParseStat(values[columnIndices["Armor"]], out float armorBase, out float armorGrowth))
                        {
                            attribute.basicAttributes.armor = armorBase;
                            attribute.levelUpAttributes.armorGrowth = armorGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("SpellResistance") && values.Length > columnIndices["SpellResistance"])
                    {
                        if (TryParseStat(values[columnIndices["SpellResistance"]], out float srBase, out float srGrowth))
                        {
                            attribute.basicAttributes.spellResistance = srBase;
                            attribute.levelUpAttributes.spellResistanceGrowth = srGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("MoveSpeed") && values.Length > columnIndices["MoveSpeed"])
                    {
                        if (TryParseIntStat(values[columnIndices["MoveSpeed"]], out int moveBase, out int moveGrowth))
                        {
                            attribute.basicAttributes.moveSpeed = moveBase;
                            attribute.levelUpAttributes.moveSpeedGrowth = moveGrowth;
                            // 注意：移动速度成长需要存储减少值，不是增加值
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("DodgeRate") && values.Length > columnIndices["DodgeRate"])
                    {
                        if (TryParseStat(values[columnIndices["DodgeRate"]], out float dodgeBase, out float dodgeGrowth))
                        {
                            attribute.basicAttributes.dodgeRate = dodgeBase;
                            attribute.levelUpAttributes.dodgeRateGrowth = dodgeGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("AttackSpeed") && values.Length > columnIndices["AttackSpeed"])
                    {
                        if (TryParseIntStat(values[columnIndices["AttackSpeed"]], out int asBase, out int asGrowth))
                        {
                            attribute.basicAttributes.attackSpeed = asBase;
                            attribute.levelUpAttributes.attackSpeedGrowth = asGrowth;
                            valCount += 2;
                        }
                    }

                    if (columnIndices.ContainsKey("AttackRange") && values.Length > columnIndices["AttackRange"])
                    {
                        if (TryParseIntStat(values[columnIndices["AttackRange"]], out int arBase, out int arGrowth))
                        {
                            attribute.basicAttributes.attackRange = arBase;
                            attribute.levelUpAttributes.attackRangeGrowth = arGrowth;
                            valCount += 2;
                        }
                    }
                
                    // 解析掉落物品
                    if (columnIndices.ContainsKey("DropItem") && values.Length > columnIndices["DropItem"])
                    {
                        attribute.basicAttributes.dropItem = ParseDropItems(values[columnIndices["DropItem"]]);
                    }

                    // 解析伤害类型
                    if (columnIndices.ContainsKey("NormalAttackDamageType") && values.Length > columnIndices["NormalAttackDamageType"])
                    {
                        if (Enum.TryParse<Category.Battle.DamageType>(values[columnIndices["NormalAttackDamageType"]], out var damageType))
                        {
                            attribute.basicAttributes.normalAttackDamageType = damageType;
                            valCount++;
                        }
                        else
                        {
                            errorList.Add($"第{i + 1}行: 无效的伤害类型 '{values[columnIndices["NormalAttackDamageType"]]}'");
                        }
                    }

                    // 解析经验值
                    if (columnIndices.ContainsKey("LevelUpEXPIncreasePercent") && values.Length > columnIndices["LevelUpEXPIncreasePercent"])
                    {
                        if (int.TryParse(values[columnIndices["LevelUpEXPIncreasePercent"]], out int levelUpExpIncreasePercent))
                        {
                            attribute.levelUpExpIncreasePercent = levelUpExpIncreasePercent;
                            valCount++;
                        }
                        else
                        {
                            errorList.Add($"第{i + 1}行: 无效的 '{values[columnIndices["LevelUpEXPIncreasePercent"]]}' 经验值提升百分比");
                        }
                    }

                    // 添加到字典
                    cardAttributeDB.creatureCardAttributes.Add(attribute);

                    if (valCount >= valNums)
                    {
                        successCount++;
                        Debug.Log($"导入生物卡属性: {creatureType}");
                    }
                    else
                        errorList.Add($"第{i + 1}行: 生物卡 {creatureType} 属性值不完整，仅设置了 {valCount}/{valNums} 个属性值");
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
            int classificationIndex = Array.IndexOf(headers, "ResourceCardClassification");
            int durabilityIndex = Array.IndexOf(headers, "Durability");
            int satietyIndex = Array.IndexOf(headers, "SatietyValue");

            // 验证类型列存在
            if (typeIndex < 0)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件缺少 ResourceType 列！请检查标题行。", "确定");
                return;
            }

            // 如果分类或耐久列缺失，允许继续但会使用默认值
            if (classificationIndex < 0)
                Debug.LogWarning("CSV 未包含 ResourceCardClassification 列，所有分类将使用 None。");
            if (durabilityIndex < 0)
                Debug.LogWarning("CSV 未包含 Durability 列，所有耐久度将使用默认值 1。");
            if (satietyIndex < 0)
                Debug.LogWarning("CSV 未包含 SatietyValue 列，所有饱腹值将使用默认值 1。");

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

                    if (values.Length <= typeIndex)
                    {
                        Debug.LogWarning($"第{i + 1}行数据列不足（缺少 ResourceType），已跳过");
                        continue;
                    }

                    // 跳过注释行
                    if (values[0].StartsWith("#"))
                        continue;

                    // 解析资源卡类型
                    if (Enum.TryParse<ResourceCardType>(values[typeIndex], out ResourceCardType resourceType))
                    {
                        CardAttributeDB.ResourceCardAttribute attribute = new CardAttributeDB.ResourceCardAttribute();
                        attribute.resourceCardType = resourceType;
                        // 解析资源卡分类，默认 None
                        ResourceCardClassification classification = ResourceCardClassification.None;
                        if (classificationIndex >= 0 && values.Length > classificationIndex && !string.IsNullOrWhiteSpace(values[classificationIndex]))
                        {
                            if (!Enum.TryParse<ResourceCardClassification>(values[classificationIndex], out classification))
                                errorList.Add($"第{i + 1}行{resourceType}: 无效的资源卡分类 '{values[classificationIndex]}'，已使用 None");
                        }
                        attribute.resourceClassification = classification;

                        // 解析耐久度，默认 1（当 CSV 为空时）
                        int durability = 1;
                        if (durabilityIndex >= 0 && values.Length > durabilityIndex && !string.IsNullOrWhiteSpace(values[durabilityIndex]))
                        {
                            if (!int.TryParse(values[durabilityIndex], out durability))
                            {
                                errorList.Add($"第{i + 1}行{resourceType}: 无效的耐久度值 '{values[durabilityIndex]}'，已使用默认值 1");
                                durability = 1;
                            }
                        }
                        attribute.durability = durability;

                        // 解析饱腹值，默认 1（当 CSV 为空时）
                        if (classification == ResourceCardClassification.Food)
                        {
                            int satietyValue = 1;
                            if (satietyIndex >= 0 && values.Length > satietyIndex && !string.IsNullOrWhiteSpace(values[satietyIndex]))
                            {
                                if (!int.TryParse(values[satietyIndex], out satietyValue))
                                {
                                    errorList.Add($"第{i + 1}行{resourceType}: 无效的饱腹值 '{values[satietyIndex]}'，已使用默认值 1");
                                    satietyValue = 1;
                                }
                            }
                            attribute.satietyValue = satietyValue;
                        }
                        else
                        {
                            // 非食物分类，饱腹值设为0
                            attribute.satietyValue = 0;
                        }

                        // 添加到字典
                        cardAttributeDB.resourceCardAttributes.Add(attribute);
                        successCount++;
                        // if (classification != ResourceCardClassification.Food)
                        //     Debug.Log($"成功导入资源卡属性: {resourceType} 分类: {attribute.resourceClassification} 耐久值: {attribute.durability}");
                        // else
                        //     Debug.Log($"成功导入资源卡属性: {resourceType} 分类: {attribute.resourceClassification} 耐久值: {attribute.durability} 饱腹值: {attribute.satietyValue}");
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

    // 解析float类型的基础值和成长值 (格式: "基础值-成长值")
    private bool TryParseStat(string value, out float baseValue, out float growthValue)
    {
        baseValue = 0;
        growthValue = 0;
        
        if (string.IsNullOrEmpty(value) || value.StartsWith("#"))
            return false;
            
        string[] parts = value.Split('-');
        if (parts.Length != 2)
            return false;
            
        return float.TryParse(parts[0], out baseValue) && float.TryParse(parts[1], out growthValue);
    }

    // 解析int类型的基础值和成长值
    private bool TryParseIntStat(string value, out int baseValue, out int growthValue)
    {
        baseValue = 0;
        growthValue = 0;
        
        if (string.IsNullOrEmpty(value) || value.StartsWith("#"))
            return false;
            
        string[] parts = value.Split('-');
        if (parts.Length != 2)
            return false;
            
        return int.TryParse(parts[0], out baseValue) && int.TryParse(parts[1], out growthValue);
    }

    // 解析掉落物品列表
    private List<DropCard> ParseDropItems(string cardsString)
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
                                errorList.Add(cardsString + "填写错误"); // 记录错误
                            break;

                        case 'C': // 生物卡
                            cardDesc.cardDescription.cardType = CardType.Creatures;
                            if (Enum.TryParse<CreatureCardType>(cardTypeStr, out CreatureCardType creatureType))
                                cardDesc.cardDescription.creatureCardType = creatureType;
                            else
                                errorList.Add(cardsString + "填写错误"); // 记录错误
                            break;

                        case 'E': // 事件卡
                            cardDesc.cardDescription.cardType = CardType.Events;
                            if (Enum.TryParse<EventCardType>(cardTypeStr, out EventCardType eventType))
                                cardDesc.cardDescription.eventCardType = eventType;
                            else
                                errorList.Add(cardsString + "填写错误"); // 记录错误
                            break;
                    }

                    cards.Add(cardDesc);
                }
            }
        }

        return cards;
    }
}