using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Category;
using System.Linq;

public class CardIconImporter : EditorWindow
{
    private CardIconsDB cardIconsDB;
    private Vector2 scrollPosition;
    private string creatureIconPath = "";
    private string resourceIconPath = "";
    private List<string> errorList = new List<string>();
    private List<string> missingList = new List<string>();

    [MenuItem("Tools/Import Card Icons")]
    public static void ShowWindow()
    {
        GetWindow<CardIconImporter>("Card Icon Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Card Icon Importer", EditorStyles.boldLabel);

        // 选择CardIconsDB
        EditorGUILayout.BeginHorizontal();
        cardIconsDB = (CardIconsDB)EditorGUILayout.ObjectField("Card Icons DB", cardIconsDB, typeof(CardIconsDB), false);
        if (GUILayout.Button("Find", GUILayout.Width(60)))
        {
            string[] guids = AssetDatabase.FindAssets("t:CardIconsDB");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cardIconsDB = AssetDatabase.LoadAssetAtPath<CardIconsDB>(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (cardIconsDB == null)
        {
            EditorGUILayout.HelpBox("请先选择一个CardIconsDB资源!", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(10);

        // 生物卡图标路径选择
        EditorGUILayout.BeginHorizontal();
        creatureIconPath = EditorGUILayout.TextField("生物卡图标路径:", creatureIconPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("选择生物卡图标文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                creatureIconPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        // 资源卡图标路径选择
        EditorGUILayout.BeginHorizontal();
        resourceIconPath = EditorGUILayout.TextField("资源卡图标路径:", resourceIconPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("选择资源卡图标文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                resourceIconPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 导入按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("导入生物卡图标"))
        {
            ImportCreatureIcons();
        }

        if (GUILayout.Button("导入资源卡图标"))
        {
            ImportResourceIcons();
        }

        if (GUILayout.Button("导入全部图标"))
        {
            ImportCreatureIcons();
            ImportResourceIcons();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 滚动视图显示当前数据
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("当前数据库内容:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"生物卡插画数量: {cardIconsDB.cardIllustrations?.Count ?? 0}");
        EditorGUILayout.LabelField($"资源卡图标数量: {cardIconsDB.resourcesCardIcons?.Count ?? 0}");
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndScrollView();
    }

    private void ImportCreatureIcons()
    {
        try
        {
            if (string.IsNullOrEmpty(creatureIconPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择生物卡图标文件夹！", "确定");
                return;
            }

            if (!Directory.Exists(creatureIconPath))
            {
                EditorUtility.DisplayDialog("错误", "指定的生物卡图标路径不存在！", "确定");
                return;
            }

            Undo.RecordObject(cardIconsDB, "Import Creature Icons");
            
            if (cardIconsDB.cardIllustrations == null)
                cardIconsDB.cardIllustrations = new List<CardIconsDB.CardIllustration>();
            
            cardIconsDB.cardIllustrations.Clear();
            errorList.Clear();
            missingList.Clear();

            int successCount = 0;
            string[] supportedExtensions = new[] { "*.png", "*.jpg", "*.jpeg" };

            // 获取所有CreatureCardType枚举值
            var creatureTypes = System.Enum.GetValues(typeof(CreatureCardType)).Cast<CreatureCardType>();

            foreach (var creatureType in creatureTypes)
            {
                if (creatureType == CreatureCardType.None || creatureType == CreatureCardType.Any)
                    continue;
                string typeName = creatureType.ToString();
                bool found = false;

                // 搜索所有支持的图片格式
                foreach (var extension in supportedExtensions)
                {
                    string[] files = Directory.GetFiles(creatureIconPath, $"{typeName}{extension.Replace("*", "")}");
                    
                    if (files.Length > 0)
                    {
                        string relativePath = GetRelativePath(files[0]);
                        if (string.IsNullOrEmpty(relativePath))
                        {
                            errorList.Add($"{typeName}: 无法获取文件相对路径");
                            continue;
                        }

                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
                        if (sprite != null)
                        {
                            CardIconsDB.CardIllustration illustration = new CardIconsDB.CardIllustration
                            {
                                cardDescription = creatureType,
                                illustration = sprite
                            };
                            cardIconsDB.cardIllustrations.Add(illustration);
                            successCount++;
                            found = true;
                            break;
                        }
                        else
                        {
                            errorList.Add($"{typeName}: 无法加载精灵 {relativePath}");
                        }
                    }
                }

                if (!found)
                {
                    missingList.Add($"生物卡: {typeName}");
                }
            }

            EditorUtility.SetDirty(cardIconsDB);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DisplayResults("生物卡图标", successCount, creatureTypes.Count() - 2);
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("导入错误", $"导入生物卡图标过程中出现错误: {ex.Message}", "确定");
            Debug.LogException(ex);
        }
    }

    private void ImportResourceIcons()
    {
        try
        {
            if (string.IsNullOrEmpty(resourceIconPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择资源卡图标文件夹！", "确定");
                return;
            }

            if (!Directory.Exists(resourceIconPath))
            {
                EditorUtility.DisplayDialog("错误", "指定的资源卡图标路径不存在！", "确定");
                return;
            }

            Undo.RecordObject(cardIconsDB, "Import Resource Icons");
            
            if (cardIconsDB.resourcesCardIcons == null)
                cardIconsDB.resourcesCardIcons = new List<CardIconsDB.ResourcesCardIcons>();
            
            cardIconsDB.resourcesCardIcons.Clear();
            errorList.Clear();
            missingList.Clear();

            int successCount = 0;
            string[] supportedExtensions = new[] { "*.png", "*.jpg", "*.jpeg" };

            // 获取所有ResourceCardType枚举值
            var resourceTypes = System.Enum.GetValues(typeof(ResourceCardType)).Cast<ResourceCardType>();

            foreach (var resourceType in resourceTypes)
            {
                if (resourceType == ResourceCardType.None)
                    continue;
                string typeName = resourceType.ToString();
                bool found = false;

                // 搜索所有支持的图片格式
                foreach (var extension in supportedExtensions)
                {
                    string[] files = Directory.GetFiles(resourceIconPath, $"{typeName}{extension.Replace("*", "")}");
                    
                    if (files.Length > 0)
                    {
                        string relativePath = GetRelativePath(files[0]);
                        if (string.IsNullOrEmpty(relativePath))
                        {
                            errorList.Add($"{typeName}: 无法获取文件相对路径");
                            continue;
                        }

                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
                        if (sprite != null)
                        {
                            CardIconsDB.ResourcesCardIcons resourceIcon = new CardIconsDB.ResourcesCardIcons
                            {
                                resourceCardType = resourceType,
                                icon = sprite
                            };
                            cardIconsDB.resourcesCardIcons.Add(resourceIcon);
                            successCount++;
                            found = true;
                            break;
                        }
                        else
                        {
                            errorList.Add($"{typeName}: 无法加载精灵 {relativePath}");
                        }
                    }
                }

                if (!found)
                {
                    missingList.Add($"资源卡: {typeName}");
                }
            }

            EditorUtility.SetDirty(cardIconsDB);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DisplayResults("资源卡图标", successCount, resourceTypes.Count() - 1);
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("导入错误", $"导入资源卡图标过程中出现错误: {ex.Message}", "确定");
            Debug.LogException(ex);
        }
    }

    private string GetRelativePath(string fullPath)
    {
        string assetsFolder = Application.dataPath;
        if (fullPath.StartsWith(assetsFolder))
        {
            return "Assets" + fullPath.Substring(assetsFolder.Length).Replace("\\", "/");
        }
        
        Debug.LogError($"文件不在Assets文件夹内: {fullPath}");
        return null;
    }

    private void DisplayResults(string iconType, int successCount, int totalCount)
    {
        System.Text.StringBuilder message = new System.Text.StringBuilder();
        message.AppendLine($"成功导入 {successCount}/{totalCount} 个{iconType}");
        
        if (missingList.Count > 0)
        {
            message.AppendLine($"\n未找到以下 {missingList.Count} 个卡牌的图标:");
            foreach (var missing in missingList)
            {
                message.AppendLine($"  • {missing}");
            }
        }
        
        if (errorList.Count > 0)
        {
            message.AppendLine($"\n导入时出现以下 {errorList.Count} 个错误:");
            foreach (var error in errorList)
            {
                message.AppendLine($"  • {error}");
            }
        }

        EditorUtility.DisplayDialog("导入完成", message.ToString(), "确定");
        
        // 同时输出到Console
        if (missingList.Count > 0)
        {
            Debug.LogWarning($"[{iconType}] 未找到的卡牌:\n" + string.Join("\n", missingList));
        }
        
        if (errorList.Count > 0)
        {
            Debug.LogWarning($"[{iconType}] 导入错误:\n" + string.Join("\n", errorList));
        }
    }
}