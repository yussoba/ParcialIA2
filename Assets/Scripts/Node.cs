using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool walkable;
    public int gridX;
    public int gridY;
    public Vector3 Position { get; }
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public Node parent;
    public List<Node> neighbors;

    public Vector3 position => transform.position;
    public float FCost => gCost + hCost;

    public void SetNeighbors(List<Node> connectedNodes)
    {
        neighbors = connectedNodes;
    }

    public Node(bool walkable, Vector3 position, int gridX, int gridY)
    {
        this.walkable = walkable;
        Position = position;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
