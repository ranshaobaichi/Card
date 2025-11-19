using System.Collections.Generic;
using Category;
using UnityEngine;

public class HexNodeManager : MonoBehaviour
{
    public static HexNodeManager Instance;
    public Dictionary<AxialCoordinate, HexNode> Tiles { get; private set; }
    public GameObject HexNodeBoard;
    public int evenColumns; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitHexNodeBoard()
    {
        // Generate grids
        Tiles = new Dictionary<AxialCoordinate, HexNode>();
        var childHexNodes = HexNodeBoard.GetComponentsInChildren<HexNode>();
        // Debug.Log($"Found {childHexNodes.Length} hex nodes in children.");
        foreach (var node in childHexNodes)
        {
            Tiles[node.coord] = node;
        }
        // Debug.Log($"Generated {Tiles.Count} hex nodes.");

        // Generate neighbours
        foreach (var tile in Tiles.Values) tile.CacheNeighbors();
    }

    public void UpdateWalkableState()
    {
        foreach (var tile in Tiles.Values)
        {
            if (tile.occupant == null)
                tile.walkable = true;
            else
                tile.walkable = false;
        }
    }

    public static void ReserveObject(B_Creature obj, HexNode node)
    {
        if (node.occupant != null)
        {
            Debug.LogError($"Node at {node.coord} is already occupied by {node.occupant.name}");
            return;
        }
        node.walkable = false;
    }

    public static void MoveObject(B_Creature obj, HexNode from, HexNode to)
    {
        if (from != null)
        {
            from.occupant = null;
            from.walkable = true;
        }
        obj.hexNode = to;
        if (to != null)
        {
            to.occupant = obj;
            to.walkable = false;
            // TODO: Add some animation here
            obj.transform.SetParent(to.transform);
            obj.transform.position = new Vector3(to.transform.position.x, to.transform.position.y, 0f);
        }
        // Debug.Log($"Moved object {obj.name} to node at position {to}");
    }
}