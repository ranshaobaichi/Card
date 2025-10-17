using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Category;

public class CardIconImporter : EditorWindow
{
    private CardAttributeDB cardAttributeDB;
    private Vector2 scrollPosition;
    private Dictionary<CardType, bool> foldoutStates = new Dictionary<CardType, bool>();
    private string[] cardTypeNames;
    private SerializedObject serializedObject;

    [MenuItem("Tools/Card System/Card Icon Importer")]
    public static void ShowWindow()
    {
        GetWindow<CardIconImporter>("Card Icon Importer");
    }

    private void OnEnable()
    {
        cardTypeNames = System.Enum.GetNames(typeof(CardType));
        
        // 初始化折叠状态
        foreach (CardType cardType in System.Enum.GetValues(typeof(CardType)))
        {
            if (!foldoutStates.ContainsKey(cardType))
            {
                foldoutStates[cardType] = false;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Card Icon Importer", EditorStyles.boldLabel);

        // 选择CardAttributeDB
        EditorGUILayout.BeginHorizontal();
        cardAttributeDB = (CardAttributeDB)EditorGUILayout.ObjectField("Card Attribute DB", cardAttributeDB, typeof(CardAttributeDB), false);
        if (GUILayout.Button("Find", GUILayout.Width(60)))
        {
            string[] guids = AssetDatabase.FindAssets("t:CardAttributeDB");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cardAttributeDB = AssetDatabase.LoadAssetAtPath<CardAttributeDB>(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (cardAttributeDB == null)
        {
            EditorGUILayout.HelpBox("请先选择一个CardAttributeDB资源!", MessageType.Warning);
            return;
        }

        serializedObject = new SerializedObject(cardAttributeDB);

        // 批量导入按钮
        if (GUILayout.Button("批量导入图标"))
        {
            BatchImportIcons();
        }

        EditorGUILayout.Space();
        
        // 滚动视图
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 为每种卡牌类型创建一个折叠面板
        foreach (CardType cardType in System.Enum.GetValues(typeof(CardType)))
        {
            foldoutStates[cardType] = EditorGUILayout.Foldout(foldoutStates[cardType], cardType.ToString() + " Icons", true);
            
            if (foldoutStates[cardType])
            {
                EditorGUI.indentLevel++;
                DisplayCardTypeIcons(cardType);
                EditorGUI.indentLevel--;
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        // 应用更改
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(cardAttributeDB);
        }
    }

    private void DisplayCardTypeIcons(CardType cardType)
    {
        // 确保字典已初始化
        if (!cardAttributeDB.cardIcons.ContainsKey(cardType))
        {
            cardAttributeDB.cardIcons[cardType] = new Dictionary<int, Sprite>();
        }

        // 获取该类型的子类型枚举
        System.Type subTypeEnum = GetSubTypeEnum(cardType);
        if (subTypeEnum == null) return;
        
        string[] subTypeNames = System.Enum.GetNames(subTypeEnum);
        System.Array subTypeValues = System.Enum.GetValues(subTypeEnum);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加全部子类型"))
        {
            for (int i = 0; i < subTypeValues.Length; i++)
            {
                int value = (int)subTypeValues.GetValue(i);
                if (!cardAttributeDB.cardIcons[cardType].ContainsKey(value))
                {
                    cardAttributeDB.cardIcons[cardType][value] = null;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // 显示已有的图标
        List<int> keysToRemove = new List<int>();
        foreach (var pair in cardAttributeDB.cardIcons[cardType])
        {
            EditorGUILayout.BeginHorizontal();
            
            // 显示子类型名称（如果可以找到）
            string subTypeName = pair.Key.ToString();
            foreach (var value in subTypeValues)
            {
                if ((int)value == pair.Key)
                {
                    subTypeName = System.Enum.GetName(subTypeEnum, value);
                    break;
                }
            }
            
            // 图标字段
            Sprite newIcon = (Sprite)EditorGUILayout.ObjectField(
                subTypeName, 
                pair.Value, 
                typeof(Sprite), 
                false,
                GUILayout.Height(64)
            );
            
            if (newIcon != pair.Value)
            {
                cardAttributeDB.cardIcons[cardType][pair.Key] = newIcon;
                EditorUtility.SetDirty(cardAttributeDB);
            }
            
            // 移除按钮
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(64)))
            {
                keysToRemove.Add(pair.Key);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // 移除标记的项
        foreach (int key in keysToRemove)
        {
            cardAttributeDB.cardIcons[cardType].Remove(key);
            EditorUtility.SetDirty(cardAttributeDB);
        }
        
        // 添加新图标
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("添加新图标", GUILayout.Width(100));
        
        // 子类型下拉菜单
        EditorGUI.BeginChangeCheck();
        int newSubTypeIndex = EditorGUILayout.Popup(-1, subTypeNames, GUILayout.Width(100));
        if (EditorGUI.EndChangeCheck() && newSubTypeIndex >= 0)
        {
            int newValue = (int)subTypeValues.GetValue(newSubTypeIndex);
            if (!cardAttributeDB.cardIcons[cardType].ContainsKey(newValue))
            {
                cardAttributeDB.cardIcons[cardType][newValue] = null;
                EditorUtility.SetDirty(cardAttributeDB);
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private System.Type GetSubTypeEnum(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Creatures:
                return typeof(CreatureCardType);
            case CardType.Resources:
                return typeof(ResourceCardType);
            case CardType.Events:
                return typeof(EventCardType);
            default:
                return null;
        }
    }

    private void BatchImportIcons()
    {
        string folderPath = EditorUtility.OpenFolderPanel("选择图标文件夹", "", "");
        if (string.IsNullOrEmpty(folderPath)) return;

        // 处理所选文件夹
        foreach (CardType cardType in System.Enum.GetValues(typeof(CardType)))
        {
            string cardTypeFolder = Path.Combine(folderPath, cardType.ToString());
            if (Directory.Exists(cardTypeFolder))
            {
                ProcessCardTypeFolder(cardType, cardTypeFolder);
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.SetDirty(cardAttributeDB);
    }

    private void ProcessCardTypeFolder(CardType cardType, string folderPath)
    {
        if (!cardAttributeDB.cardIcons.ContainsKey(cardType))
        {
            cardAttributeDB.cardIcons[cardType] = new Dictionary<int, Sprite>();
        }

        System.Type subTypeEnum = GetSubTypeEnum(cardType);
        if (subTypeEnum == null) return;

        // 获取相对路径（相对于Assets文件夹）
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        foreach (FileInfo file in dirInfo.GetFiles("*.png"))
        {
            // 尝试从文件名解析子类型
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            
            // 尝试将文件名解析为枚举值
            object subTypeValue = null;
            try
            {
                subTypeValue = System.Enum.Parse(subTypeEnum, fileName, true);
            }
            catch
            {
                // 无法解析，跳过
                Debug.LogWarning($"无法将文件名 {fileName} 解析为 {subTypeEnum.Name} 的值");
                continue;
            }
            
            // 获取相对路径（相对于Assets文件夹）
            string relativePath = GetRelativePath(file.FullName);
            if (string.IsNullOrEmpty(relativePath)) continue;

            // 加载精灵
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            if (sprite != null)
            {
                int enumValue = (int)subTypeValue;
                cardAttributeDB.cardIcons[cardType][enumValue] = sprite;
            }
            else
            {
                Debug.LogWarning($"无法加载精灵: {relativePath}");
            }
        }
    }

    private string GetRelativePath(string fullPath)
    {
        string assetsFolder = Application.dataPath;
        if (fullPath.StartsWith(assetsFolder))
        {
            return "Assets" + fullPath.Substring(assetsFolder.Length);
        }
        
        // 如果文件在Assets外部，需要先将其导入项目
        string fileName = Path.GetFileName(fullPath);
        string destPath = Path.Combine(Application.dataPath, "Imported Icons", fileName);
        
        // 确保目标目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
        
        try
        {
            File.Copy(fullPath, destPath, true);
            return "Assets/Imported Icons/" + fileName;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"复制文件失败: {e.Message}");
            return null;
        }
    }
}