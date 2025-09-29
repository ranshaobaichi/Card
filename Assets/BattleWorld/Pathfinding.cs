using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding {
    private class HexNodeComparer : IComparer<HexNode>
    {
        public int Compare(HexNode x, HexNode y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int f = x.F.CompareTo(y.F);
            if (f != 0) return f;

            int h = x.H.CompareTo(y.H);
            if (h != 0) return h;

            // Tie-break：使用坐标（假设 AxialCoordinate 有 Q / R），再用哈希
            int qCmp = x.coord.Q.CompareTo(y.coord.Q);
            if (qCmp != 0) return qCmp;
            int rCmp = x.coord.R.CompareTo(y.coord.R);
            if (rCmp != 0) return rCmp;

            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }

    // 可选：初始化一个不可达初始值
    private const int INF = int.MaxValue / 4;

    private static void ResetNodes()
    {
        // 需要能访问全部节点；假设 HexNodeManager.Instance 已初始化
        foreach (var node in HexNodeManager.Instance.Tiles.Values)
        {
            node.SetG(INF);
            node.SetH(0);
        }
    }

    public static List<HexNode> FindPath(HexNode startNode, HexNode targetNode)
    {
        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("FindPath 传入空节点");
            return null;
        }

        // 若允许终点被占用但仍作为目标，可在此放宽 target.walkable 条件
        if (!startNode.walkable)
        {
            Debug.LogWarning("起点不可行走");
            return null;
        }

        ResetNodes();

        var open = new SortedSet<HexNode>(new HexNodeComparer());
        var closed = new HashSet<HexNode>();
        var cameFrom = new Dictionary<HexNode, HexNode>();

        startNode.SetG(0);
        startNode.SetH(startNode.GetDistance(targetNode));
        open.Add(startNode);

        while (open.Count > 0)
        {
            var current = open.Min;
            open.Remove(current);
            closed.Add(current);

            // Debug.Log($"当前节点 {current.coord} G={current.G} H={current.H} F={current.F}");

            if (current == targetNode)
            {
                // 重建路径
                var path = new List<HexNode>();
                var c = targetNode;
                int guard = 2000;
                while (c != null && guard-- > 0)
                {
                    path.Add(c);
                    if (c == startNode) break;
                    if (!cameFrom.TryGetValue(c, out c))
                    {
                        Debug.LogError("cameFrom 缺失，路径重建失败。");
                        return null;
                    }
                }
                if (guard <= 0)
                {
                    Debug.LogError("路径重建死循环防护触发");
                    return null;
                }
                path.Reverse();
                Debug.Log($"找到路径 长度={path.Count}  {string.Join(" -> ", path.Select(n => n.coord.ToString()))}");
                return path;
            }

            // 遍历邻居
            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null) continue;

                if (!neighbor.walkable && neighbor != targetNode) continue;
                if (closed.Contains(neighbor)) continue;

                int tentativeG = current.G + 1; // 邻接六边形移动代价为 1

                bool better = tentativeG < neighbor.G;
                bool notInOpen = !open.Contains(neighbor);

                if (better || notInOpen)
                {
                    neighbor.SetG(tentativeG);
                    neighbor.SetH(neighbor.GetDistance(targetNode));
                    cameFrom[neighbor] = current;
                    // Debug.Log($"更新节点 {neighbor.coord} G={tentativeG} H={neighbor.H} 来自 {current.coord}");

                    if (notInOpen)
                    {
                        open.Add(neighbor);
                    }
                    else
                    {
                        // 需要强制重排：移除再加
                        open.Remove(neighbor);
                        open.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("未找到可达路径");
        return null;
    }
}