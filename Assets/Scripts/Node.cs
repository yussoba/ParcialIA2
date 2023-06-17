using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector]public float gCost;
    [HideInInspector]public float hCost;
    [HideInInspector] public Node cameFrom;
    public float fCost => gCost + hCost;
    public List<Node> neighbors;
    [HideInInspector] public Node parent;
    public Vector3 position => transform.position;
}
