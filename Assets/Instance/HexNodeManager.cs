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
            {
                switch (tile.occupant)
                {
                    case B_Creatures:
                        tile.walkable = (tile.occupant as B_Creatures).IsMovingState;
                        break;
                    default:
                        Debug.LogError("Unknown occupant type");
                        tile.walkable = false;
                        break;
                }
            }
        }
    }

    public static void ReserveObject(B_Object obj, HexNode node)
    {
        if (node.occupant != null)
        {
            Debug.LogError($"Node at {node.coord} is already occupied by {node.occupant.name}");
            return;
        }
        node.walkable = false;
    }

    public static void MoveObject(B_Object obj, HexNode from, HexNode to)
    {
        if (from != null)
        {
            from.occupant = null;
            from.walkable = true;
        }
        to.occupant = obj;
        to.walkable = false;
        obj.hexNode = to;

        // TODO: Add some animation here
        obj.transform.position = new Vector3(to.transform.position.x, to.transform.position.y, obj.transform.position.z);
        Debug.Log($"Moved object {obj.name} to node at position {to}");
    }
}