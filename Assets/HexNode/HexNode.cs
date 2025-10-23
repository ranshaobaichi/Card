using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct AxialCoordinate
{
    public int R; // Row
    public int Q; // Column

    public AxialCoordinate(int r = 0, int q = 0)
    {
        R = r;
        Q = q;
    }

    // 计算距离
    public readonly int GetDistance(AxialCoordinate other)
    {
        AxialCoordinate upCoord, downCoord;
        if (R == other.R)
        {
            return Math.Abs(Q - other.Q);
        }
        if (R < other.R)
        {
            upCoord = this;
            downCoord = other;
        }
        else
        {
            upCoord = other;
            downCoord = this;
        }
        int l, r;
        int verticalSteps = downCoord.R - upCoord.R;
        int targetMaxColumns = downCoord.R % 2 == 0 ? HexNodeManager.Instance.evenColumns : HexNodeManager.Instance.evenColumns + 1;

        // Get the range of Q in the target row
        if (upCoord.R % 2 == 0)
        {
            l = Math.Abs(upCoord.Q - verticalSteps / 2);
            r = Math.Min(upCoord.Q + (verticalSteps + 1) / 2, targetMaxColumns);
        }
        else
        {
            l = Math.Abs(upCoord.Q - (verticalSteps + 1) / 2);
            r = Math.Min(upCoord.Q + verticalSteps / 2, targetMaxColumns);
        }
        
        if (downCoord.Q >= l && downCoord.Q <= r)
            return verticalSteps;
        else
            return verticalSteps + Math.Min(Math.Abs(downCoord.Q - l), Math.Abs(downCoord.Q - r)) + 1;
    }

    public override readonly string ToString() => $"({R}, {Q})";
}

public class HexNode : MonoBehaviour
{
    static readonly (int, int)[][] directions = new (int, int)[][]
    {
        new (int, int)[] { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) },
        new (int, int)[] { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) },
    };

    public B_Creature occupant;
    public AxialCoordinate coord;
    public bool walkable;
    public int GetDistance(HexNode other) => coord.GetDistance(other.coord);

    void Start()
    {
        walkable = true;
    }

    #region Pathfinding
    public List<HexNode> neighbors { get; protected set; }
    public int G { get; private set; }
    public int H { get; private set; }
    public int F => G + H;

    public void CacheNeighbors()
    {
        neighbors?.Clear();
        foreach (var dir in directions[coord.R & 1])
        {
            var neighborCoord = new AxialCoordinate(coord.R + dir.Item1, coord.Q + dir.Item2);
            if (HexNodeManager.Instance.Tiles.TryGetValue(neighborCoord, out var neighborNode))
            {
                neighbors ??= new List<HexNode>();
                neighbors.Add(neighborNode);
            }
        }


        /// TEST
        Text text = GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = coord.ToString();
            // Debug.Log($"Node {coord} has {neighbors?.Count ?? 0} neighbors.");
        }
    }

    public void SetG(int g)
    {
        G = g;
    }

    public void SetH(int h)
    {
        H = h;
    }
    #endregion
}