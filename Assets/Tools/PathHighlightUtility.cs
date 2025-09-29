#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PathHighlightUtility
{
    private struct ColorRecord
    {
        public Graphic graphic;
        public Color graphicColor;

        public Renderer renderer;
        public Color materialColor;
    }

    private static readonly Dictionary<Object, ColorRecord> _original = new();

    public static void HighlightPath(
        List<HexNode> nodes,
        Color pathColor,
        HexNode startNode,
        Color startColor,
        HexNode targetNode,
        Color targetColor)
    {
        RestoreAll();
        if (nodes == null) return;

        for (int i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (n == null) continue;

            var colorToSet = pathColor;
            if (n == startNode) colorToSet = startColor;
            else if (n == targetNode) colorToSet = targetColor;

            ApplyColor(n.gameObject, colorToSet);
        }
    }

    public static void RestoreAll()
    {
        if (_original.Count == 0) return;

        foreach (var kv in _original)
        {
            var rec = kv.Value;
            if (rec.graphic != null)
                rec.graphic.color = rec.graphicColor;
            else if (rec.renderer != null)
                SafeSetRendererColor(rec.renderer, rec.materialColor);
        }
        _original.Clear();
    }

    private static void ApplyColor(GameObject go, Color c)
    {
        if (go == null) return;
        // 优先 UI
        var g = go.GetComponent<Graphic>();
        if (g != null)
        {
            if (!_original.ContainsKey(g))
            {
                _original[g] = new ColorRecord { graphic = g, graphicColor = g.color };
            }
            g.color = c;
            return;
        }

        // 再尝试普通 Renderer
        var r = go.GetComponent<Renderer>();
        if (r != null && r.sharedMaterial != null)
        {
            if (!_original.ContainsKey(r))
            {
                var col = r.sharedMaterial.HasProperty("_Color")
                    ? r.sharedMaterial.color
                    : Color.white;
                _original[r] = new ColorRecord { renderer = r, materialColor = col };
            }
            SafeSetRendererColor(r, c);
        }
    }

    private static void SafeSetRendererColor(Renderer r, Color c)
    {
        if (r == null) return;
        // 使用 material 会实例化，调试工具场景中可接受
        if (r.material.HasProperty("_Color"))
        {
            r.material.color = c;
        }
    }
}
#endif