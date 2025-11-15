using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class HierarchyExpandCollapse
{
    static MethodInfo s_SetExpandedRecursive;
    static EditorWindow s_SceneHierarchyWindow;

    static void EnsureReflection()
    {
        if (s_SetExpandedRecursive != null && s_SceneHierarchyWindow != null) return;

        var asm = typeof(Editor).Assembly;
        var sceneType = asm.GetType("UnityEditor.SceneHierarchyWindow");
        if (sceneType == null) return;

        s_SceneHierarchyWindow = EditorWindow.GetWindow(sceneType);
        s_SetExpandedRecursive = sceneType.GetMethod("SetExpandedRecursive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int), typeof(bool) }, null);

        // 兼容不同 Unity 版本（尝试备用方法名）
        if (s_SetExpandedRecursive == null)
            s_SetExpandedRecursive = sceneType.GetMethod("SetExpanded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int), typeof(bool) }, null);
    }

    [MenuItem("GameObject/Expand Selected Hierarchy %#e", false, 0)]
    public static void ExpandSelected()
    {
        EnsureReflection();
        if (s_SetExpandedRecursive == null || s_SceneHierarchyWindow == null)
        {
            Debug.LogWarning("无法访问 SceneHierarchyWindow.SetExpandedRecursive（当前 Unity 版本可能不支持）。");
            return;
        }

        foreach (var go in Selection.gameObjects)
            s_SetExpandedRecursive.Invoke(s_SceneHierarchyWindow, new object[] { go.GetInstanceID(), true });
    }

    [MenuItem("GameObject/Collapse Selected Hierarchy %#q", false, 0)]
    public static void CollapseSelected()
    {
        EnsureReflection();
        if (s_SetExpandedRecursive == null || s_SceneHierarchyWindow == null)
        {
            Debug.LogWarning("无法访问 SceneHierarchyWindow.SetExpandedRecursive（当前 Unity 版本可能不支持）。");
            return;
        }

        foreach (var go in Selection.gameObjects)
            s_SetExpandedRecursive.Invoke(s_SceneHierarchyWindow, new object[] { go.GetInstanceID(), false });
    }

    // 仅在有选中物体时启用菜单
    [MenuItem("GameObject/Expand Selected Hierarchy %#e", true)]
    [MenuItem("GameObject/Collapse Selected Hierarchy %#q", true)]
    public static bool ValidateSelection()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }
}