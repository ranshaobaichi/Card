using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathfindingShowing : EditorWindow
{
    private HexNode startNode;
    private HexNode targetNode;

    private Color pathColor = Color.yellow;
    private Color startColor = new Color(0.2f, 0.8f, 0.2f);
    private Color targetColor = new Color(0.9f, 0.3f, 0.3f);

    private List<HexNode> currentPath;
    private bool autoRecalculateWhenChanged = true;
    private bool includeStartNode = true;
    private bool includeTargetNode = true;

    [MenuItem("Tools/Pathfinding Viewer")]
    public static void Open()
    {
        GetWindow<PathfindingShowing>("Pathfinding Viewer");
    }

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        ClearHighlight();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Hex Pathfinding", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        startNode = (HexNode)EditorGUILayout.ObjectField("Start", startNode, typeof(HexNode), true);
        targetNode = (HexNode)EditorGUILayout.ObjectField("Target", targetNode, typeof(HexNode), true);
        if (EditorGUI.EndChangeCheck() && autoRecalculateWhenChanged)
        {
            TryCalculate();
        }

        pathColor = EditorGUILayout.ColorField("pathColor", pathColor);
        startColor = EditorGUILayout.ColorField("startColor", startColor);
        targetColor = EditorGUILayout.ColorField("targetColor", targetColor);

        includeStartNode = EditorGUILayout.Toggle("includeStartNode", includeStartNode);
        includeTargetNode = EditorGUILayout.Toggle("includeTargetNode", includeTargetNode);
        autoRecalculateWhenChanged = EditorGUILayout.Toggle("autoRecalculateWhenChanged", autoRecalculateWhenChanged);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Calculate Path", GUILayout.Height(28)))
            {
                TryCalculate();
            }
            if (GUILayout.Button("Clear/Restore", GUILayout.Height(28)))
            {
                ClearHighlight();
            }
        }

        if (currentPath != null)
        {
            EditorGUILayout.HelpBox($"Current Path Node Count: {currentPath.Count}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No Path", MessageType.None);
        }

        if (startNode == null || targetNode == null)
        {
            EditorGUILayout.HelpBox("Please select start and target HexNode.", MessageType.Warning);
        }
    }

    private void EditorUpdate()
    {
        // 若节点被删除或置空则恢复
        if ((startNode == null || targetNode == null) && currentPath != null)
        {
            ClearHighlight();
        }
    }

    private void TryCalculate()
    {
        ClearHighlight();

        if (startNode == null || targetNode == null)
            return;

        // 确保邻居已缓存（如果你的生成流程已经调用可忽略）
        EnsureNeighborsCached(startNode);
        EnsureNeighborsCached(targetNode);

        var rawPath = Pathfinding.FindPath(startNode, targetNode);
        if (!includeStartNode && rawPath.Count > 0 && rawPath[0] == startNode)
            rawPath.RemoveAt(0);
        if (!includeTargetNode && rawPath.Count > 0 && rawPath[^1] == targetNode)
            rawPath.RemoveAt(rawPath.Count - 1);

        currentPath = rawPath;
        PathHighlightUtility.HighlightPath(currentPath, pathColor, startNode, startColor, targetNode, targetColor);
    }

    private void ClearHighlight()
    {
        if (currentPath != null)
        {
            PathHighlightUtility.RestoreAll();
            currentPath = null;
        }
    }

    private void EnsureNeighborsCached(HexNode node)
    {
        if (node == null) return;
        if (node.neighbors == null || node.neighbors.Count == 0)
        {
            node.CacheNeighbors();
        }
    }
}