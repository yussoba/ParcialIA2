using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public static List<Node> FindPath(Node start, Node goal)
    {
        var openSet = new HashSet<Node>();
        var closedSet = new HashSet<Node>();

        start.gCost = 0f;
        openSet.Add(start);
        
        while (openSet.Count > 0)
        {
            // Obtener el nodo con el fScore más bajo de la lista abierta
            Node currentNode = GetLowestFCostNode(openSet);

            // Si el nodo actual es el objetivo, se ha encontrado el camino óptimo
            if (currentNode == goal)
                return ReconstructPath(start, goal);

            // Mover el nodo actual de la lista abierta a la cerrada
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Explorar los vecinos del nodo actual
            foreach (var neighbor in currentNode.neighbors)
            {
                // Si el vecino está en el conjunto cerrado, ignorarlo
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                // Calcular el costo G tentativo desde el nodo de inicio hasta el vecino
                var tentativeGCost = currentNode.gCost + Heuristic(currentNode.transform.position, neighbor.transform.position);

                // Si el vecino no está en el conjunto abierto o el costo G es menor
                if (!openSet.Contains(neighbor) || tentativeGCost < neighbor.gCost)
                {
                    // Actualizar los valores del vecino
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Heuristic(neighbor.transform.position, goal.transform.position);
                    neighbor.parent = currentNode;

                    // Si el vecino no está en el conjunto abierto, agregarlo
                    openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private static List<Node> ReconstructPath(Node start, Node goal)
    {
        var path = new List<Node>();
        var current = goal;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private static Node GetLowestFCostNode(HashSet<Node> nodes)
    {
        Node lowestNode = null;
        var lowestFCost = float.MaxValue;

        foreach (var node in nodes)
        {
            if (node.fCost < lowestFCost)
            {
                lowestFCost = node.fCost;
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private static float Heuristic(Vector3 start, Vector3 goal)
    {
        return Vector3.Distance(start, goal);
    }

    public static Node FindClosestNodeToPos(Vector3 position, List<Node> nodes = null)
    {
        var allNodes = nodes == null ? FindObjectsOfType<Node>().ToList() : nodes;
        var currentNode = allNodes[0];
        
        foreach (var node in allNodes)
        {
            if (Heuristic(currentNode.position, position) > Heuristic(node.position, position))
            {
                currentNode = node;
            }
        }

        return currentNode;
    }
}
